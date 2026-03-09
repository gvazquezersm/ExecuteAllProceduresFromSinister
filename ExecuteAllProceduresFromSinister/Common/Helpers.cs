using DocumentFormat.OpenXml.Packaging;
using ExecuteAllProceduresFromSinister.Business.Dto;
using ExecuteAllProceduresFromSinister.Business.Models;
using ExecuteAllProceduresFromSinister.Common.Enums;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExecuteAllProceduresFromSinister.Common
{
    public static class Helpers
    {
        public static string GetRefFromSubjectReplaced(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;

            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase))
            {
                var selectedDataRemoved = subjectRequest.ToLower().Replace(subjectCase.ToLower(), string.Empty);
                if (!string.IsNullOrEmpty(selectedDataRemoved))
                {
                    data = selectedDataRemoved.Trim();
                }
            }

            return data;
        }

        public static string GetRefFromLastDashSubject(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;

            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase))
            {
                var selectedDataRemoved = subjectRequest.ToLower().Replace(subjectCase.ToLower(), string.Empty);
                if (!string.IsNullOrEmpty(selectedDataRemoved))
                {
                    var subjectTrim = selectedDataRemoved.Trim();
                    var lastDash = subjectTrim.LastIndexOf(CommonConstants.Dash);
                    data = subjectTrim.Substring(0, lastDash).Trim();
                }
            }

            return data;
        }

        public static bool IsExceptionResentAutoCase(string subject)
        {
            var isExceptionCase = false;
            if (!string.IsNullOrEmpty(subject))
            {
                isExceptionCase = subject.Contains(SubjectCasesConstants.ExceptionResentAutoAccentCase, StringComparison.InvariantCultureIgnoreCase) || subject.Contains(SubjectCasesConstants.ExceptionResentAutoCase, StringComparison.InvariantCultureIgnoreCase);
            }

            return isExceptionCase;
        }

        public static string GetRefSinisterFromSubjectBetweenDash(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;

            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase) && subjectRequest.ToLower().Contains(subjectCase.ToLower()))
            {
                var caseRemoveInSubject = subjectRequest.ToLower().Replace(subjectCase.ToLower(), CommonConstants.WhiteSpace)?.Trim();
                if (!string.IsNullOrEmpty(caseRemoveInSubject))
                {
                    var indexFirst = caseRemoveInSubject.IndexOf(CommonConstants.Dash);
                    var indexEnd = caseRemoveInSubject.IndexOf(CommonConstants.Dash, indexFirst + 1);
                    if (indexFirst > 0 && indexEnd > 0)
                    {
                        var dataSubjectFirstDash = caseRemoveInSubject.Substring(indexFirst + 1, indexEnd - indexFirst - 1);
                        if (!string.IsNullOrEmpty(dataSubjectFirstDash))
                        {
                            data = dataSubjectFirstDash.Trim();
                        }
                    }
                }
            }

            return data;
        }

        public static string GetFirstElementSplitFromSubject(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;

            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase))
            {
                var selectedDataRemoved = subjectRequest.ToLower().Replace(subjectCase.ToLower(), string.Empty);
                if (!string.IsNullOrEmpty(selectedDataRemoved))
                {
                    var subjectTrimSplit = selectedDataRemoved.Trim().Split(CommonConstants.WhiteSpace);
                    if (subjectTrimSplit != null && subjectTrimSplit.Any())
                    {
                        data = subjectTrimSplit.FirstOrDefault()?.Trim();
                    }
                }
            }

            return data;
        }

        public static string GetSinisterClaudatorsFromSubject(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;

            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase))
            {
                var elementSelected = subjectRequest.ToLower().Split(CommonConstants.WhiteSpace).FirstOrDefault(x => x.Contains(subjectCase.ToLower()));
                if (!string.IsNullOrEmpty(elementSelected) && elementSelected.Contains(CommonConstants.TwoPoints))
                {
                    var twoPointsIndex = elementSelected.IndexOf(CommonConstants.TwoPoints) + 1;
                    var closeClaudatorIndex = elementSelected.IndexOf(CommonConstants.CloseClaudator);

                    // Validar que se encontró el corchete de cierre y que hay longitud positiva
                    if (closeClaudatorIndex > 0 && closeClaudatorIndex > twoPointsIndex)
                    {
                        data = elementSelected.Substring(twoPointsIndex, closeClaudatorIndex - twoPointsIndex).Trim();
                    }
                }
            }

            return data;
        }

        public static string GetPreviousDataFromString(string element)
        {
            var reference = string.Empty;

            if (!string.IsNullOrEmpty(element) && element.Contains(CommonConstants.Forwarding))
            {
                var forwardingIndex = element.IndexOf(CommonConstants.Forwarding);
                if (forwardingIndex > 0)
                {
                    reference = element.Substring(0, forwardingIndex);
                }
            }

            return reference;
        }

        public static bool CheckTextBodyFromCase(string body, string bodyCase)
        {
            var existCaseBody = false;
            if (!string.IsNullOrEmpty(body) && !string.IsNullOrEmpty(bodyCase))
            {
                existCaseBody = body.ToLower().Contains(bodyCase.ToLower());
            }

            return existCaseBody;
        }

        public static string ParseHTMLToText(string HTMLCode)
        {
            // Remove new lines since they are not visible in HTML
            HTMLCode = HTMLCode.Replace("\n", " ");

            // Remove tab spaces
            HTMLCode = HTMLCode.Replace("\t", " ");

            // Remove multiple white spaces from HTML
            HTMLCode = Regex.Replace(HTMLCode, "\\s+", " ");

            // Remove HEAD tag
            HTMLCode = Regex.Replace(HTMLCode, "<head.*?</head>", ""
                                , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any JavaScript
            HTMLCode = Regex.Replace(HTMLCode, "<script.*?</script>", ""
              , RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Replace special characters like &, <, >, " etc.
            StringBuilder sbHTML = new StringBuilder(HTMLCode);
            // Note: There are many more special characters, these are just
            // most common. You can add new characters in this arrays if needed
            string[] OldWords = { "&nbsp;", "&amp;", "&quot;", "&lt;", "&gt;", "&reg;", "&copy;", "&bull;", "&trade;", "&#39;" };
            string[] NewWords = { " ", "&", "\"", "<", ">", "®", "©", "•", "™", "\'" };
            for (int i = 0; i < OldWords.Length; i++)
            {
                sbHTML.Replace(OldWords[i], NewWords[i]);
            }

            // Check if there are line breaks (<br>) or paragraph (<p>)
            sbHTML.Replace("<br>", "\n<br>");
            sbHTML.Replace("<br ", "\n<br ");
            sbHTML.Replace("<p ", "\n<p ");

            // Finally, remove all HTML tags and return plain text
            return System.Text.RegularExpressions.Regex.Replace(
              sbHTML.ToString(), "<[^>]*>", "");
        }

        public static bool ContainsTextInFiles(IEnumerable<FileDto> files, string textCase)
        {
            var containsText = false;

            if (files != null && !string.IsNullOrEmpty(textCase))
            {
                foreach (var file in files)
                {
                    if (!containsText)
                    {
                        var textFile = string.Empty;
                        var extension = GetFileTypeFromExtension(file.Name);
                        switch (extension)
                        {
                            case FileTypeEnum.Pdf:
                                textFile = PdfBytesToText(file.ContentBytes);
                                break;
                            case FileTypeEnum.Word:
                                textFile = WordBytesToText(file.ContentBytes);
                                break;
                            case FileTypeEnum.Txt:
                                textFile = Encoding.UTF8.GetString(file.ContentBytes);
                                break;
                        }

                        containsText = !string.IsNullOrEmpty(textFile) && textFile.Contains(textCase);
                    }
                }
            }

            return containsText;
        }

        private static string WordBytesToText(byte[] dataFile)
        {
            var textFile = string.Empty;
            var stream = new MemoryStream(dataFile);
            if (stream != null && stream.Length > 0)
            {
                var document = WordprocessingDocument.Open(stream, true);
                if (document != null)
                {
                    textFile = document.MainDocumentPart.Document.Body.InnerText;
                }

            }
            return textFile;
        }

        private static string PdfBytesToText(byte[] dataFile)
        {
            var textFile = string.Empty;

            if (dataFile != null && dataFile.Any())
            {
                PdfReader pdfReader = new PdfReader(dataFile);
                if (pdfReader != null)
                {
                    var stringBuilder = new StringBuilder();
                    for (int i = 1; i <= pdfReader.NumberOfPages; i++)
                    {
                        try
                        {
                            stringBuilder.Append(PdfTextExtractor.GetTextFromPage(pdfReader, i));
                        }
                        catch
                        {
                            // página sin contenido extraíble (imagen, cifrada, etc.)
                        }
                    }

                    if (stringBuilder.Length > 0)
                    {
                        textFile = stringBuilder.ToString();
                    }
                }
            }

            return textFile;
        }

        private static FileTypeEnum GetFileTypeFromExtension(string fileName)
        {
            var fileTypeEnum = FileTypeEnum.None;

            if (!string.IsNullOrEmpty(fileName))
            {
                var lastDotIndex = fileName.LastIndexOf(CommonConstants.Dot);
                if (lastDotIndex > 0)
                {
                    lastDotIndex++;
                    var extension = fileName[lastDotIndex..];
                    if (!string.IsNullOrEmpty(extension))
                    {
                        switch (extension.ToLower())
                        {
                            case CommonConstants.Pdf:
                                fileTypeEnum = FileTypeEnum.Pdf;
                                break;
                            case CommonConstants.Docx:
                            case CommonConstants.Doc:
                                fileTypeEnum = FileTypeEnum.Word;
                                break;
                            case CommonConstants.Txt:
                                fileTypeEnum = FileTypeEnum.Txt;
                                break;
                        }
                    }
                }
            }

            return fileTypeEnum;
        }

        /// <summary>
        /// Devuelve el valor recibido (matchedValue) sin modificaciones.
        /// Se usa cuando el propio asunto o el fragmento capturado por regex ES la referencia (p.ej. Generali 150-151).
        /// </summary>
        public static string GetSubjectTrimmed(string subjectRequest, string subjectCase)
        {
            return subjectRequest?.Trim() ?? string.Empty;
        }

        private static string GetRefFromDeletingElementsAndJoin(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;
            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase))
            {
                var elementsSplitted = subjectRequest.Split(subjectCase);
                if (elementsSplitted != null)
                {
                    foreach (var elementData in elementsSplitted)
                    {
                        data += elementData;
                    }
                }
            }

            return data;
        }

        private static string GetRefFromInitStringToCaseString(string subjectRequest, string subjectCase)
        {
            var data = string.Empty;

            if (!string.IsNullOrEmpty(subjectRequest) && !string.IsNullOrEmpty(subjectCase))
            {
                var indexSubjectCase = subjectRequest.IndexOf(subjectCase);
                if (indexSubjectCase > 0)
                {
                    data = subjectRequest.Substring(0, subjectRequest.Length - indexSubjectCase - 1).Trim();
                }
            }

            return data;
        }

        /// <summary>
        /// Devuelve el último token (separado por espacio) del fragmento capturado por regex.
        /// Útil para patrones como "siniestro núm. 1234567" donde la referencia es el último elemento.
        /// </summary>
        public static string GetLastTokenFromMatch(string subjectRequest, string subjectCase)
        {
            if (string.IsNullOrEmpty(subjectRequest)) return string.Empty;
            var parts = subjectRequest.Trim().Split(' ');
            return parts.LastOrDefault()?.Trim() ?? string.Empty;
        }

        public static List<DataGenericMailModel<IEnumerable<DataActionMailModel>>> GetActionsMailSpecificCases()
        {
            var list = new List<DataGenericMailModel<IEnumerable<DataActionMailModel>>>()
            {
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "cts.autosnoreste@allianz.es",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseOne, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseSix, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseEight, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                    }
                },
                // Casos 144-146: cts.atenciondirecta@allianz.es — mismos asuntos que autosnoreste
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "cts.atenciondirecta@allianz.es",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseOne, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseSix, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseEight, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "cts.autoscentro@allianz.es",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseTwo, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseNine, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "siniestrosflotas@ersmgrupo.com",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseSeven, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "mediadores@plusultrainfo.com",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseSeventeen, new Func<string,string,string>(GetFirstElementSplitFromSubject)),
                        new DataActionMailModel(SubjectCasesConstants.CaseThirtyNine, new Func<string, string, string>(GetFirstElementSplitFromSubject)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "webmed@axa.es",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseSixteen, new Func<string,string,string>(GetSinisterClaudatorsFromSubject)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "noreply@plusultra.es",
                    Data = new List<DataActionMailModel>()
                    {
                        // Casos 118-120: "Siniestro/Tramitador Siniestro X - xxxxxxxx" (un solo guion → ref tras el guion)
                        new DataActionMailModel(SubjectCasesConstants.CaseSinisterHogar, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseSinisterComunidad, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseSinisterComercios, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseTramitadorSinHogar, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseTramitadorSinComunidad, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseTramitadorSinComercios, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        // Caso 103: "Tramitador Siniestro Flotas - xxxxxxxx - Tomador" (dos guiones → ref entre guiones)
                        new DataActionMailModel(SubjectCasesConstants.CaseThirtyOne, new Func<string,string,string>(GetRefSinisterFromSubjectBetweenDash)),
                    }
                },

                // Casos 154-155: MGS - email específico para garantizar prioridad sobre dominio
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "enviosautomaticosweb@mgs.es",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseMgsApertura, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                        new DataActionMailModel(SubjectCasesConstants.CaseMgsNuevaAccion, new Func<string,string,string>(GetRefFromSubjectReplaced)),
                    }
                },

                // Casos 121-122: siniestros.diversos@zurich.com — "...siniestro núm. xxxxxxxxxx"
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "siniestros.diversos@zurich.com",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseSinisterNum, new Func<string,string,string>(GetLastTokenFromMatch)),
                    }
                },

                // Caso 123: noreply@zurich.com — "Información sobre el siniestro nº xxxxxxxxxx"
                new DataGenericMailModel<IEnumerable<DataActionMailModel>>()
                {
                    Case = "noreply@zurich.com",
                    Data = new List<DataActionMailModel>()
                    {
                        new DataActionMailModel(SubjectCasesConstants.CaseFourtyOne, new Func<string,string,string>(GetLastTokenFromMatch)),
                    }
                },

            };

            return list;
        }

        public static IEnumerable<DataDomainMailModel> GetAnyCaseOnlySubject()
        {
            var list = new List<DataDomainMailModel>()
            {
                // "sin. vseg:" → comportamiento por defecto
                new DataDomainMailModel(SubjectCasesConstants.CaseFourtyThree, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourtyThree),
                // "VST..." → crea tarea genérica
                new DataDomainMailModel(SubjectCasesConstants.CaseFourtyThreeV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourtyThreeV2, isGenericTask: true),
                // "VS..."  → solo carga datos, NO crea tarea
                new DataDomainMailModel(SubjectCasesConstants.CaseFourtyThreeV3, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourtyThreeV3, onlyLoad: true),
            };

            return list;
        }

        public static IEnumerable<string> ToEmailsList(this string emails)
        {
            var emailsList = new List<string>();

            if (!string.IsNullOrEmpty(emails))
            {
                emailsList = emails.Split(CommonConstants.DotComma)?.ToList();
            }

            return emailsList;
        }

        public static IEnumerable<DataGenericMailModel<IEnumerable<DataDomainMailModel>>> GetActionsMailDomain()
        {
            var list = new List<DataGenericMailModel<IEnumerable<DataDomainMailModel>>>()
            {
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "fiatc.es",
                    Data = new List<DataDomainMailModel>()
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyTwo, new Func<string,string,string>(GetRefFromDeletingElementsAndJoin), null, PatternRegexConstants.CaseThiryTwo),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        // Casos 124-129, 132-133: asuntos con formato "Texto#referencia"
                        new DataDomainMailModel(SubjectCasesConstants.CaseHash, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseHashRef),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "libertyseguros.es",
                    Data = new List<DataDomainMailModel>()
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyThree, new Func<string,string,string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThirtyThree),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyFour, new Func<string, string, string>(GetRefFromInitStringToCaseString), ContainMailsConstants.SinisterCase, PatternRegexConstants.CaseThirtyFour),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "allianz.es",
                    Data = new List<DataDomainMailModel>()
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThiryFive, new Func<string, string, string>(GetRefFromSubjectReplaced), ContainMailsConstants.SinisterCase, PatternRegexConstants.CaseThiryFive),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "axa.es",
                    Data = new List<DataDomainMailModel>()
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtySix, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThirtySix),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtySix, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThirtySixV2),
                        new DataDomainMailModel(SubjectCasesConstants.CaseElevenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseElevenV2),
                        new DataDomainMailModel(SubjectCasesConstants.CaseElevenV3, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseElevenV3),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "reale.es",
                    Data = new List<DataDomainMailModel>()
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFourty, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourty),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "helvetia.es",
                    Data =  new List<DataDomainMailModel>()
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThirtyEight),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFourtyTwo, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourtyTwo),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                        // Caso 157: "Pago Referencia xxxxxxxxx" (SINIESTROS.PARTICULARES@HELVETIA.ES)
                        new DataDomainMailModel(SubjectCasesConstants.CasePagoReferencia, new Func<string, string, string>(GetFirstElementSplitFromSubject)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "zurich.com",
                    Data = new List<DataDomainMailModel>
                    {
                        // Caso 123: "Información sobre el siniestro nº xxxxxxxxxx" (noreply@zurich.com)
                        new DataDomainMailModel(SubjectCasesConstants.CaseFourtyOne, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourtyOne),
                        // Casos 121-122: "...siniestro núm. xxxxxxxxxx" (siniestros.diversos@zurich.com)
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterNum, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterNum),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterZurich, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterZurich),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "iurisart.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "gcoservicios.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "catalanaoccidente.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseOccidente, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseGuion),
                        // Casos 109-117: "Siniestro/Tramitador Siniestro X - xxxxxxxx" (prefijo exacto → ref tras guion)
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinHogar, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinComunidad, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinComercios, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinAutoMovil, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinRCProfes, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinRCPrInm, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterHogar, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterComunidad, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterComercios, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "pelayo.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "mapfre.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseReference, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyEight, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "mutuadepropietarios.es",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefMP, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefMP),
                        new DataDomainMailModel(SubjectCasesConstants.CaseReference, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                        // Caso 134: "Siniestro : xxxxxxxx" (espacio antes de dos puntos)
                        new DataDomainMailModel(SubjectCasesConstants.CaseSiniestroSpaceColon, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSiniestroSpaceColon),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "catalanaoccidenteinfo.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefMP, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefMP),
                        new DataDomainMailModel(SubjectCasesConstants.CaseReference, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseReference),
                        new DataDomainMailModel(SubjectCasesConstants.CaseOccidente, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseGuion),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSin, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSin),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseRefSinWithoutDot, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseRefSinWithoutDot),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinister),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinister, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithoutSpace),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSinisterWithSpace),
                    }
                },

                // ═══════════════════════════════════════════════════════════════════════
                //  NUEVOS DOMINIOS - Casos 102-157
                // ═══════════════════════════════════════════════════════════════════════

                // Caso 102: MMT Seguros - "Expediente: xxxxxxxx. Nueva notificación..."
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "mmtseguros.es",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseFourty, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFourty),
                    }
                },

                // Caso 104: QualitasAutoClassic - "Auto Classic: AU04 2026/xxxxxxx"
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "qualitasautoclassic.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyThree, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseSlashRef),
                    }
                },

                // Caso 106: Occident Informa - "Siniestro Diversos xxxxxxxx"
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "occidentinforma.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtyNine, new Func<string, string, string>(GetFirstElementSplitFromSubject)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterHogar, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterComunidad, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseSinisterComercios, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinHogar, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinComunidad, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinComercios, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinAutoMovil, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinRCProfes, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseTramitadorSinRCPrInm, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                    }
                },

                // Casos 107-108: GCO Digitaliza - "COPIA DE CORRESPONDENCIA..." / "N/ REF."
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "gco.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseCopiaCorrespondenciaSiniestro, new Func<string, string, string>(GetFirstElementSplitFromSubject)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseNRefWithSpace, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseElevenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseElevenV2),
                        new DataDomainMailModel(SubjectCasesConstants.CaseElevenV3, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseElevenV3),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                    }
                },

                // Casos 130-131: FIATC email.fiatc.es - sin separador #
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "email.fiatc.es",
                    CanonicalDomain = "fiatc.es",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseHash, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseHashRef),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFiatcDocResolucion, new Func<string, string, string>(GetFirstElementSplitFromSubject)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFiatcNuevaDoc, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFiatcNuevaDoc, isGenericTask: true),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFiatcNuevaDocCat, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFiatcNuevaDocCat, isGenericTask: true),
                        new DataDomainMailModel(SubjectCasesConstants.CaseFifteenV2, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseFifteen),
                    }
                },

                // Casos 148-149: Allianz partners externos - "S-xxxxxxxxx - texto"
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "multiassistance.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThiryFive, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThiryFive),
                    }
                },
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "multiasistencia.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThiryFive, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThiryFive),
                    }
                },

                // Casos 150-151: Asitur/Generali - el asunto completo ES la referencia
                // Formatos: numérico puro, alfanumérico, con guiones/espacios
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "asitur.es",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(string.Empty, new Func<string, string, string>(GetSubjectTrimmed), null, PatternRegexConstants.CaseOnlyNumbers),
                        new DataDomainMailModel(string.Empty, new Func<string, string, string>(GetSubjectTrimmed), null, PatternRegexConstants.CaseAlphanumericRef),
                    }
                },


                // Caso 152: Sinexia - "SINIESTRO:xxxxxxxx"
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "sinexia.org",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtySix, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThirtySix),
                        new DataDomainMailModel(SubjectCasesConstants.CaseThirtySix, new Func<string, string, string>(GetRefFromSubjectReplaced), null, PatternRegexConstants.CaseThirtySixV2),
                    }
                },

                // Caso 158: Generali TMT - "148614271/TMT(RD023)(085182559)"
                // Casos 150-151 v2: Generali - asunto solo numérico (csm.siniestros.bi@generalion.es)
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "generalion.es",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseGeneralionTmt, new Func<string, string, string>(GetSubjectTrimmed), null, PatternRegexConstants.CaseGeneralionTmt),
                        new DataDomainMailModel(string.Empty, new Func<string, string, string>(GetSubjectTrimmed), null, PatternRegexConstants.CaseOnlyNumbers),
                        new DataDomainMailModel(string.Empty, new Func<string, string, string>(GetSubjectTrimmed), null, PatternRegexConstants.CaseAlphanumericRef),
                    }
                },

                // Caso 153: Murimar - "MURIMAR - Referencia: xxxxxxxxxx"
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "murimar.com",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseMurimar, new Func<string, string, string>(GetRefFromSubjectReplaced)),
                    }
                },

                // Casos 154-155: MGS - "MGS Informa: ... siniestro xxxxxxxx"
                new DataGenericMailModel<IEnumerable<DataDomainMailModel>>()
                {
                    Case = "mgs.es",
                    Data = new List<DataDomainMailModel>
                    {
                        new DataDomainMailModel(SubjectCasesConstants.CaseMgsApertura,new Func<string, string, string>(GetRefFromSubjectReplaced)),
                        new DataDomainMailModel(SubjectCasesConstants.CaseMgsNuevaAccion,new Func<string, string, string>(GetRefFromSubjectReplaced)),
                    }
                },
            };

            return list;
        }

        public static IEnumerable<DataGenericMailModel<DataExceptionMailModel>> GetExceptionsMailCases()
        {
            var listExceptions = new List<DataGenericMailModel<DataExceptionMailModel>>
            {
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "mediador@allianz.es",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFiftyOne,
						BodyCase = BodyCasesConstants.CaseFiftyOne,
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = true
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "mediadores@occidentinfo.com",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFifty,
						BodyCase = BodyCasesConstants.CaseFifty,
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = true
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "no-responder@mutuadepropietarios.es",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFourtyNine,
						BodyCase = BodyCasesConstants.CaseFourtyNine,
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = true
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "comunicaciones.digitales@axa.es",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFourtyEight,
						BodyCase = "",
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = false
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "mediador@allianz.es",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFourtySeven,
						BodyCase = BodyCasesConstants.CaseFourtySeven,
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = true
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "CSM.PROD.BI@LIBERTYSEGUROS.ES",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFourtySix,
						BodyCase = "",
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = false
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "no-responder@mutuadepropietarios.es",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFourtyFive,
						BodyCase = BodyCasesConstants.CaseFourtyFive,
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = true
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
				{
					Case = "mediador@allianz.es",
					Data = new DataExceptionMailModel()
					{
						SubjectCase = SubjectCasesConstants.CaseFourtyFour,
                        BodyCase = BodyCasesConstants.CaseFourtyFour,
						CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
						IsCheckBody = true
					}
				},
				new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "cts.autosnoroeste@allianz.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseThree, BodyCase = BodyCasesConstants.CaseThree,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
                        IsCheckBody = true
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "documento@allianz.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseFive, BodyCase = BodyCasesConstants.CaseFive,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
                        IsCheckBody = true
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "cts.autosnoreste@allianz.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyTwo, BodyCase = BodyCasesConstants.CaseTwentyTwo,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
                        IsCheckBody = true
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "mediadores@plusultrainfo.com",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyThree,
                        IsCheckBody = true,
                        BodyCase = BodyCasesConstants.CaseTwentyThree,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "gestionsiniestrosproduccion@caser.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyFour,
                        BodyCase = BodyCasesConstants.CaseTwentyFour,
                        IsCheckBody = true,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "noreply@plusultra.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyOne,
                        BodyCase = BodyCasesConstants.CaseTwentyOne,
                        IsCheckBody = true,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "mediador@allianz.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseFour,
                        FileTextCase = FileTextCasesConstants.CaseFour,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "cicos.gestion@axa.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseEighteen,
                        FileTextCase = FileTextCasesConstants.CaseEigtheen,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "cicos.gestion@axa.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwenty,
                        FileTextCase = FileTextCasesConstants.CaseTwenty,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "no-responder@reale.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseNineteen,
                        FileTextCase = FileTextCasesConstants.CaseNineteen,
                        BodyCase = BodyCasesConstants.CaseNineteen,
                        IsCheckBody = true,
                        IsCheckAttachments = true,
                        CheckBody = new Func<string, string, bool>(CheckTextBodyFromCase),
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "sdm.gestion@axa.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentySeven,
                        FileTextCase = FileTextCasesConstants.CaseTwentySeven,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "siniestros@fiatc.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyEight,
                        FileTextCase = FileTextCasesConstants.CaseTwentyEight,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "materialesautos@mutuatfe.com",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyNine,
                        FileTextCase = FileTextCasesConstants.CaseTwentyNine,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "csm.siniestros.bi@libertyseguros.es",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseThirty,
                        FileTextCase = FileTextCasesConstants.CaseThirty,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "prestacionesautoscmpvalencia@mapfre.com",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentyFive,
                        FileTextCase = FileTextCasesConstants.CaseTwentyFive,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                },
                new DataGenericMailModel<DataExceptionMailModel>()
                {
                    Case = "prestacionesautos@mapfre.com",
                    Data = new DataExceptionMailModel()
                    {
                        SubjectCase = SubjectCasesConstants.CaseTwentySix,
                        FileTextCase = FileTextCasesConstants.CaseTwentySix,
                        IsCheckAttachments = true,
                        CheckAttachments = new Func<IEnumerable<FileDto>, string, bool>(ContainsTextInFiles)
                    }
                }
            };

            return listExceptions;
        }

        public static string ToJson(this AutomationSinisterDataWithAttachmentsDto dto)
        {
            var jsonValue = string.Empty;

            if (dto != null)
            {
                jsonValue = JsonConvert.SerializeObject(new
                {
                    FromMail = dto.FromMail,
                    Path = dto.Path,
                    SinisterId = dto.SinisterId,
                    Subject = dto.Subject,
                    GciAlias = dto.GciAlias,
                    Body = dto.Body,
                    IsBodyHtml = dto.IsBodyHtml,
                    DateTimeReceived = dto.DateTimeReceived,
                    Attachments = dto.Attachments?.Select(s => new { Name = s.Name, Size = s.ContentBytes.Length }),
                    ToMails = dto.ToMails,
                });
            }

            return jsonValue;
        }
    }
}



