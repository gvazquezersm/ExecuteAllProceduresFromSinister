using ExecuteAllProceduresFromSinister.Business.Dto;
using ExecuteAllProceduresFromSinister.Business.Models;
using ExecuteAllProceduresFromSinister.Common;
using ExecuteAllProceduresFromSinister.Common.Enums;
using ExecuteAllProceduresFromSinister.Core.HttpClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ExecuteAllProceduresFromSinister
{
    public static class ExecuteAllProceduresFromSinister
    {
        private static ILogger _log;
        [FunctionName("ExecuteAllProceduresFromSinister")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            _log = log;
            _log.LogInformation("Processing data from email...");
            var result = CommonConstants.NotProcessed;
            var isAuditMode = req.Query["audit"] == "true";
            var patternFound = false;

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dataSinisterFilterDto = JsonConvert.DeserializeObject<AutomationMailHeaderFilterDto>(requestBody);
            if (dataSinisterFilterDto != null)
            {
                var isExceptionCase = IsExceptionMailCase(dataSinisterFilterDto.Body, dataSinisterFilterDto.Subject, dataSinisterFilterDto.OriginMail, dataSinisterFilterDto.Attachments);
                if (IsDebugEnabled()) _log.LogInformation("[EXCEPTION_CHECK] Subject: {Subject} | OriginMail: {Mail} | IsException: {IsException}",
    dataSinisterFilterDto.Subject, dataSinisterFilterDto.OriginMail, isExceptionCase);
                if (!isExceptionCase)
                { // SI NO ES UN CASO EXCEPCIONAL...
                    try
                    {
                        /// <summary>
                        /// Obtiene la referencia de siniestro/póliza analizando el asunto y el remitente.
                        /// Prioridad: 1. Caso específico por Email | 2. Reglas por Dominio/Palabra clave | 3. Casos generales por Asunto.
                        /// </summary>
                        var cleanedSubject = System.Text.RegularExpressions.Regex.Replace(
                            dataSinisterFilterDto.Subject ?? string.Empty,
                            @"^(RE|RV|FW|FWD)\s*:\s*", string.Empty,
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
                        var dataReferenceModel = GetReferenceModelFromSubjectCase(cleanedSubject, dataSinisterFilterDto.OriginMail);

                        if (IsDebugEnabled()) _log.LogInformation("[REF_MODEL] Reference: {Ref} | IsGenericTask: {Generic} | OnlyLoad: {Only}",
                            dataReferenceModel?.Reference, dataReferenceModel?.IsGenericTask, dataReferenceModel?.OnlyLoad);

                        if (dataReferenceModel != null && !string.IsNullOrEmpty(dataReferenceModel.Reference))
                        {
                            patternFound = true;
                            var successProcess = await ExecuteProcessFromSinister(dataSinisterFilterDto, dataReferenceModel);
                            if (successProcess)
                            {
                                _log.LogInformation("Process success!");
                                result = CommonConstants.Success;
                            }

                            await UpdateCountMail(successProcess ? UpdateCountMailTypeEnum.Processed : UpdateCountMailTypeEnum.NotProcessed);
                        }

                    }
                    catch (System.Exception e)
                    {
                        _log.LogInformation(e.ToString());
                        throw new System.Exception();
                    }
                }
                else
                {
                    _log.LogInformation("Exception case found!");
                    result = CommonConstants.ExceptionCase;
                }
            }

            _log.LogInformation("Process finished data from email...");

            if (isAuditMode && result == CommonConstants.NotProcessed)
            {
                return new OkObjectResult(patternFound ? "Not processed: sinister not found" : "Pattern not found");
            }

            return new OkObjectResult(result);
        }

        private static async Task UpdateCountMail(UpdateCountMailTypeEnum updateCountMailType)
        {
            await BaseHttpClientService.PostAsync<UpdateCountMailDataDto, bool>(CommonConstants.EndpointUpdateCountMail, new UpdateCountMailDataDto()
            {
                Tipology = TipologyMailEnum.Sinister,
                UpdateType = updateCountMailType
            });
        }

        private static async Task<bool> ExecuteProcessFromSinister(AutomationMailHeaderFilterDto automationMailHeaderFilterDto, DataReferenceModel referenceModel)
        {
            var result = false;
            var effectiveOriginMail = referenceModel.LookupOriginMail ?? automationMailHeaderFilterDto.OriginMail;
            var sinisterDataFilterDto = new AutomationSinisterDataFilterDto()
            {
                OriginMail = referenceModel.IsSinAlias ? null : effectiveOriginMail,
                SinRefCompany = referenceModel.IsSinAlias ? null : referenceModel.Reference,
                SinAlias = referenceModel.IsSinAlias ? referenceModel.Reference : null,
                IsSinAlias = referenceModel.IsSinAlias
            };
            if (IsDebugEnabled()) _log.LogInformation($"Sinister Filter Data: {Newtonsoft.Json.JsonConvert.SerializeObject(sinisterDataFilterDto)}");

            await AddLogLogicApp(automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject, JsonConvert.SerializeObject(sinisterDataFilterDto));

            var requestUrl = new Uri(new Uri(CommonConstants.BaseUrl), CommonConstants.EndpointGetSinisterData).ToString();
            if (IsDebugEnabled()) _log.LogInformation($"Llamando a la url: {requestUrl}");

            //HACEMOS LA LLAMADA A LA API ERSM
            var sinisterData = await BaseHttpClientService.PostAsync<AutomationSinisterDataFilterDto, AutomationSinisterDataDto>(CommonConstants.EndpointGetSinisterData, sinisterDataFilterDto);
            if (sinisterData != null && sinisterData.SinisterId > 0)
            {
                var automationSinisterDataAttachs = new AutomationSinisterDataWithAttachmentsDto()
                {
                    GciAlias = sinisterData.GciAlias,
                    Path = sinisterData.Path,
                    SinisterId = sinisterData.SinisterId,
                    Subject = automationMailHeaderFilterDto.Subject,
                    Body = automationMailHeaderFilterDto.Body,
                    Attachments = automationMailHeaderFilterDto.Attachments,
                    DateTimeReceived = automationMailHeaderFilterDto.DateTimeReceived,
                    FromMail = automationMailHeaderFilterDto.OriginMail,
                    IsBodyHtml = automationMailHeaderFilterDto.IsHtml,
                    ToMails = automationMailHeaderFilterDto.ToMail?.ToEmailsList(),
                    CCMails = automationMailHeaderFilterDto.CC?.ToEmailsList()
                };

                if (IsDebugEnabled()) _log.LogInformation("[AUDIT][DATA-ATTACH] Objeto completo preparado para envío: {Payload}",
                    JsonConvert.SerializeObject(automationSinisterDataAttachs));

                await AddLogLogicApp(automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject, automationSinisterDataAttachs.ToJson());

                // PETICION
                var responseLoadFileLink = await BaseHttpClientService.PostAsync<AutomationSinisterDataWithAttachmentsDto, bool>(CommonConstants.EndpointLoadFileToDocumentalLink, automationSinisterDataAttachs);

                if (!responseLoadFileLink)
                {
                    await AddLogLogicApp(automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject, "Upload files to server failed!");
                    _log.LogError("[STEP: UPLOAD][FAILED] La API devolvió FALSE al intentar vincular el archivo. Siniestro: {SinisterId} | Path: {Path}",
        automationSinisterDataAttachs.SinisterId,
        automationSinisterDataAttachs.Path);
                }

                if (!referenceModel.OnlyLoad) // Si VS → OnlyLoad=true → entra al else (no hace nada)
                {
                    var successCreateTask = false;

                    // Si VST → IsGenericTask=true → crea tarea genérica
                    if (referenceModel.Resent || referenceModel.IsGenericTask)
                    {
                        successCreateTask = await BaseHttpClientService.GetAsync<bool>($"{CommonConstants.EndpointCreateTaskNewEmailGeneric}/{sinisterData.SinisterId}");
                        if (!successCreateTask)
                        {
                            await AddLogLogicApp(automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject, "Create Task New Email Generic failed!");
                            _log.LogError("[STEP: TASK-GENERIC][FAILED] No se pudo crear la tarea genérica. SinisterId: {SinisterId}", sinisterData.SinisterId);
                        }
                    }
                    else
                    {
                        // Caso normal (sin. vseg:) → crea tarea de compañía
                        successCreateTask = await BaseHttpClientService.GetAsync<bool>($"{CommonConstants.EndpointCreateTaskNewEmailCia}/{sinisterData.SinisterId}");
                        if (!successCreateTask)
                        {
                            await AddLogLogicApp(automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject, "Fallo al crear la tarea de nuevo correo de Compañía");
                            _log.LogError("[STEP: TASK-CIA][FAILED] No se pudo crear la tarea de compañía. SinisterId: {SinisterId}", sinisterData.SinisterId);
                        }
                    }

                    result = successCreateTask;
                }
                // Si OnlyLoad=true (VS), este bloque se salta → NO se crea ninguna tarea
            }
            else
            {
                await AddLogLogicApp(automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject, "Datos del siniestro no encontrados");
                _log.LogWarning("[STEP: SINISTER-DATA][NOT FOUND] No se encontraron datos del siniestro. Mail: {Mail} | Subject: {Subject}",
                    automationMailHeaderFilterDto.OriginMail, automationMailHeaderFilterDto.Subject);
            }

            return result;
        }

        private static bool IsApiLogEnabled()
        {
            var value = Environment.GetEnvironmentVariable("AllowLogLogicApps");
            return bool.TryParse(value, out var enabled) && enabled;
        }

        private static bool IsDebugEnabled()
        {
            var value = Environment.GetEnvironmentVariable("EnableDebugLogs");
            return bool.TryParse(value, out var enabled) && enabled;
        }

        private static async Task AddLogLogicApp(string originMail, string subject, string observations)
        {
            if (!IsApiLogEnabled()) return;

            try
            {
                var addLogicAppDto = new AddLogsLogicAppDto()
                {
                    OriginMail = originMail ?? string.Empty,
                    Subject = subject ?? string.Empty,
                    Tipology = TipologyMailEnum.Sinister,
                    Observations = observations ?? string.Empty,
                };
                await BaseHttpClientService.PostAsync<AddLogsLogicAppDto, bool>(CommonConstants.EndpointAddLogLogicApps, addLogicAppDto);
            }
            catch (System.Exception e)
            {
                _log.LogWarning("[LOG-API] Fallo al registrar log en API: {Error}", e.Message);
            }
        }

        /*Crea un objeto vacío DataReferenceModel.
        Si originMail no está vacío:
        Lo convierte en un objeto MailAddress para separar usuario y dominio.
        Intenta encontrar una referencia de manera específica
        Busca si el mail completo coincide con algún caso específico(GetActionsMailSpecificCases)
        Si encuentra coincidencia, obtiene la referencia usando GetReferenceFromSubjectCaseSpecific
        Si no encuentra referencia específica
        Busca reglas por dominio(GetActionsMailDomain) y verifica si el usuario contiene alguna palabra clave
        Si hay coincidencias, obtiene la referencia con GetDataReferenceFromSubjectCaseNotSpecific
        Si aún no hay referencia
        Revisa casos generales basados solo en el asunto(GetAnyCaseOnlySubject) y marca IsSinAlias = true.
        Devuelve el DataReferenceModel resultante, que puede tener la referencia encontrada o estar vacío si no coincidió nada.*/
        private static DataReferenceModel GetReferenceModelFromSubjectCase(string subject, string originMail)
        {
            var dataReferenceModel = new DataReferenceModel();
            if (!string.IsNullOrEmpty(originMail))
            {
                var mailAddress = new MailAddress(originMail);
                if (IsDebugEnabled()) _log.LogInformation("[DEBUG] Email: {Email} | Host: {Host} | Subject: {Subject}", mailAddress.Address, mailAddress.Host, subject);

                if (string.IsNullOrEmpty(dataReferenceModel.Reference))
                {
                    var casesSpecificList = Helpers.GetActionsMailSpecificCases();
                    var casesSpecificFilterFromMail = casesSpecificList.FirstOrDefault(x => x.Case.ToLower() == mailAddress.Address.ToLower());
                    if (IsDebugEnabled()) _log.LogInformation("[DEBUG] Specific case found: {Found}", casesSpecificFilterFromMail != null);
                    
                    if (casesSpecificFilterFromMail != null)
                    {
                        dataReferenceModel.Reference = GetReferenceFromSubjectCaseSpecific(subject, casesSpecificFilterFromMail);
                    }

                    if (string.IsNullOrEmpty(dataReferenceModel.Reference))
                    {
                        var actionsMailDomainList = Helpers.GetActionsMailDomain();
                        var actionMailDomain = actionsMailDomainList.FirstOrDefault(x => x.Case.ToLower() == mailAddress.Host.ToLower());
                        if (IsDebugEnabled()) _log.LogInformation("[DEBUG] Domain case found: {Found} | Domain: {Domain}", actionMailDomain != null, mailAddress.Host);
                        if (actionMailDomain != null)
                        {
                            var subjectCaseActionContain = actionMailDomain.Data.Where(x => !string.IsNullOrEmpty(x.ContainMail) && mailAddress.User.ToLower().Contains(x.ContainMail)).ToList();
                            if (subjectCaseActionContain != null && subjectCaseActionContain.Any())
                            {
                                dataReferenceModel = GetDataReferenceFromSubjectCaseNotSpecific(subject, subjectCaseActionContain);
                            }

                            if (string.IsNullOrEmpty(dataReferenceModel.Reference))
                            {
                                dataReferenceModel = GetDataReferenceFromSubjectCaseNotSpecific(subject, actionMailDomain.Data);
                            }

                            // Si el caso de dominio tiene un dominio canónico registrado en ERSM distinto
                            // al subdominio del remitente, normalizamos el OriginMail para la búsqueda.
                            if (!string.IsNullOrEmpty(dataReferenceModel.Reference) && !string.IsNullOrEmpty(actionMailDomain.CanonicalDomain))
                            {
                                dataReferenceModel.LookupOriginMail = $"{mailAddress.User}@{actionMailDomain.CanonicalDomain}";
                                if (IsDebugEnabled()) _log.LogInformation("[DOMAIN-NORMALIZE] OriginMail normalizado: {Original} → {Normalized}", originMail, dataReferenceModel.LookupOriginMail);
                            }
                        }

                        // Anycase with unique subject
                        if (string.IsNullOrEmpty(dataReferenceModel.Reference))
                        {
                            var anyCaseOnlySubject = Helpers.GetAnyCaseOnlySubject();
                            dataReferenceModel = GetDataReferenceFromSubjectCaseNotSpecific(subject, anyCaseOnlySubject);
                            dataReferenceModel.IsSinAlias = true;
                        }
                    }

                    // Último recurso: IA (Groq) si ningún patrón ha resuelto la referencia
                    if (string.IsNullOrEmpty(dataReferenceModel.Reference) && IsAiExtractionEnabled())
                    {
                        _log.LogInformation("[AI] No pattern matched. Attempting Groq extraction. Subject: {Subject} | Mail: {Mail}", subject, originMail);
                        var aiReference = GetReferenceFromAiAsync(subject, originMail).GetAwaiter().GetResult();
                        if (!string.IsNullOrEmpty(aiReference))
                        {
                            _log.LogInformation("[AI] Groq extracted reference: {Ref}", aiReference);
                            dataReferenceModel.Reference = aiReference;
                            dataReferenceModel.IsGenericTask = true;
                            dataReferenceModel.IsSinAlias = false;
                        }
                        else
                        {
                            _log.LogInformation("[AI] Groq could not extract a reference.");
                        }
                    }
                }
            }

            return dataReferenceModel;
        }

        private static bool IsAiExtractionEnabled()
        {
            var value = Environment.GetEnvironmentVariable("EnableAiExtraction");
            return bool.TryParse(value, out var enabled) && enabled;
        }

        private static async Task<string> GetReferenceFromAiAsync(string subject, string originMail)
        {
            return await GroqClientService.ExtractSinisterReferenceAsync(subject, originMail);
        }

        private static string GetReferenceFromSubjectCaseSpecific(string subject, DataGenericMailModel<IEnumerable<DataActionMailModel>> casesSpecificFilterFromMail)
        {
            var reference = string.Empty;
            var caseSpecific = casesSpecificFilterFromMail.Data.FirstOrDefault(x => subject.Contains(x.Subject));
            if (caseSpecific != null)
            {
                reference = caseSpecific.FuncSubject.Invoke(subject, caseSpecific.Subject);
            }

            return reference;
        }

        private static DataReferenceModel GetDataReferenceFromSubjectCaseNotSpecific(string subject, IEnumerable<DataDomainMailModel> subjectCaseActionMailList)
        {
            var dataReferenceModel = new DataReferenceModel()
            {
                Reference = string.Empty,
            };
            
            if (IsDebugEnabled())
            {
                foreach (var item in subjectCaseActionMailList)
                    _log.LogInformation("[DEBUG] Pattern: {Pattern} | Regex: {Regex} | Subject: {Subject}",
                        item.PatternRegex, item.Subject ?? "null", item.Subject ?? "null");
            }

            var casePatternRegexMatch = subjectCaseActionMailList.FirstOrDefault(x => !string.IsNullOrEmpty(x.PatternRegex) && Regex.IsMatch(subject, x.PatternRegex, RegexOptions.IgnoreCase));
            if (IsDebugEnabled()) _log.LogInformation("[DEBUG] Regex match found: {Found} | Pattern: {Pattern}", casePatternRegexMatch != null, casePatternRegexMatch?.PatternRegex);

            if (casePatternRegexMatch != null)
            {
                var stringPatternMatch = Regex.Match(subject, casePatternRegexMatch.PatternRegex, RegexOptions.IgnoreCase);
                if (IsDebugEnabled()) _log.LogInformation("[DEBUG] MATCH VALUE: '{Value}' | Length: {Len}", stringPatternMatch.Value, stringPatternMatch.Value.Length);

                if (stringPatternMatch != null && stringPatternMatch.Success)
                {
                    dataReferenceModel.IsGenericTask = casePatternRegexMatch.IsGenericTask;
                    dataReferenceModel.Reference = casePatternRegexMatch.FuncSubject(stringPatternMatch.Value, casePatternRegexMatch.Subject);
                    dataReferenceModel.OnlyLoad = casePatternRegexMatch.OnlyLoad;
                    if (IsDebugEnabled()) _log.LogInformation("[DEBUG] Extracted reference: {Ref}", dataReferenceModel.Reference);
                }
            }
            else
            {
                var subjectCaseElement = subjectCaseActionMailList.FirstOrDefault(x => string.IsNullOrEmpty(x.PatternRegex) && subject.Contains(x.Subject));
                if (subjectCaseElement != null)
                {
                    dataReferenceModel.IsGenericTask = subjectCaseElement.IsGenericTask;
                    dataReferenceModel.Reference = subjectCaseElement.FuncSubject.Invoke(subject, subjectCaseElement.Subject);
                    dataReferenceModel.OnlyLoad = subjectCaseElement.OnlyLoad;
                }
            }

            return dataReferenceModel;
        }

        /// <summary>
        /// Determina si un correo debe tratarse como un caso de excepción (es decir, se debe omitir el procesamiento normal).
        /// </summary>
        /// <param name="body">Contenido HTML o texto del correo.</param>
        /// <param name="subject">Asunto del correo.</param>
        /// <param name="originMail">Dirección de correo del remitente.</param>
        /// <param name="attachments">Colección de adjuntos del correo.</param>
        /// <returns>
        /// Devuelve <c>true</c> cuando el correo cumple alguna regla de excepción; <c>false</c> en caso contrario.
        /// </returns>
        /// <remarks>
        /// Lógica paso a paso:
        /// 1. Si <paramref name="originMail"/> o <paramref name="subject"/> están vacíos, devuelve <c>false</c>.
        /// 2. Comprueba rápidamente si el asunto coincide con un caso de reenvío/auto (Helpers.IsExceptionResentAutoCase).
        ///    - Si es true, el correo es excepción.
        /// 3. Si no, obtiene la lista de reglas de excepción con Helpers.GetExceptionsMailCases() y busca una entrada cuya
        ///    propiedad Case coincida (ignorando mayúsculas) con <paramref name="originMail"/> y cuyo Data.SubjectCase
        ///    contenga el <paramref name="subject"/> (también ignorando mayúsculas).
        /// 4. Si se encuentra una regla:
        ///    - Marca inicialmente como excepción.
        ///    - Si la regla exige comprobar el cuerpo (IsCheckBody), convierte el HTML a texto (Helpers.ParseHTMLToText)
        ///      y llama a Data.CheckBody(parsedBody, BodyCase). El resultado de esa validación sustituye el estado de excepción.
        ///    - Si tras la comprobación del cuerpo sigue siendo excepción y la regla exige comprobar adjuntos (IsCheckAttachments),
        ///      llama a Data.CheckAttachments(attachments, FileTextCase) y usa su resultado final.
        /// 5. Si no se encuentra la regla, se escribe un log informativo y se considera no excepción.
        /// 6. Comparaciones de texto son insensibles a mayúsculas; las comprobaciones del cuerpo/adjuntos solo se ejecutan cuando
        ///    la regla lo requiere y solo si las comprobaciones previas permiten continuar.
        /// </remarks>
        private static bool IsExceptionMailCase(string body, string subject, string originMail, IEnumerable<FileDto> attachments)
        {
            var isExceptionCase = false;

            if (!string.IsNullOrEmpty(originMail) && !string.IsNullOrEmpty(subject))
            {
                isExceptionCase = Helpers.IsExceptionResentAutoCase(subject);
                if (!isExceptionCase)
                {
                    var listExceptions = Helpers.GetExceptionsMailCases();
                    var exceptionCase = listExceptions.FirstOrDefault(x => x.Case.ToLower() == originMail.ToLower() && x.Data.SubjectCase.ToLower().Contains(subject.ToLower()));
                    if (exceptionCase != null)
                    {
                        isExceptionCase = true;
                        if (exceptionCase.Data.IsCheckBody)
                        {
                            isExceptionCase = exceptionCase.Data.CheckBody(Helpers.ParseHTMLToText(body), exceptionCase.Data.BodyCase);
                        }

                        if (isExceptionCase && exceptionCase.Data.IsCheckAttachments)
                        {
                            isExceptionCase = exceptionCase.Data.CheckAttachments(attachments, exceptionCase.Data.FileTextCase);
                        }
                    }
                    else
                    {
                        _log.LogInformation("Email not found in exception cases list");
                    }
                }
            }

            return isExceptionCase;
        }
    }
}
