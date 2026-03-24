using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
namespace ExecuteAllProceduresFromSinister.Core.HttpClient
{
    public static class GroqClientService
    {
        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

        private static bool IsDebugEnabled()
        {
            var value = Environment.GetEnvironmentVariable("EnableDebugLogs");
            return bool.TryParse(value, out var enabled) && enabled;
        }

        /// <summary>
        /// Llama a la API configurada (Groq o Ollama) para extraer el identificador de siniestro del asunto del email.
        /// Devuelve null si no encuentra identificador o si la llamada falla.
        /// Variables de entorno requeridas: GroqApiUrl, GroqModel, GroqApiKey.
        /// </summary>
        public static async Task<string> ExtractSinisterReferenceAsync(string subject, string originMail)
        {
            try
            {
                var apiUrl = Environment.GetEnvironmentVariable("GroqApiUrl");
                var model = Environment.GetEnvironmentVariable("GroqModel");
                var apiKey = Environment.GetEnvironmentVariable("GroqApiKey");

                var debugEnabled = IsDebugEnabled();
                if (debugEnabled) Console.WriteLine($"[AI-CONFIG] ApiUrl: {apiUrl} | Model: {model} | HasKey: {!string.IsNullOrEmpty(apiKey)}");

                if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(model)) return null;

                var prompt = $@"Eres un extractor de referencias de siniestros de seguros.
Del siguiente asunto de email, extrae ÚNICAMENTE el identificador del siniestro.
El identificador suele ser un número, código alfanumérico, o referencia con formato como: 12345678, S-12345678, AU04 2026/12345, #12345678, VST12345, etc.
Si no encuentras ningún identificador claro, responde exactamente con: NOT_FOUND
Responde SOLO con el identificador, sin texto adicional, sin explicaciones.

Remitente: {originMail}
Asunto: {subject}";

                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0,
                    max_tokens = 50,
                    think = false
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                if (!string.IsNullOrEmpty(apiKey))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (debugEnabled) Console.WriteLine($"[AI-HTTP] StatusCode: {(int)response.StatusCode} {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    if (debugEnabled) Console.WriteLine($"[AI-HTTP-ERROR] Body: {errorBody}");
                    return null;
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                if (debugEnabled)
                {
                    System.Diagnostics.Debug.WriteLine($"[AI-RAW] Groq response: {responseJson}");
                    Console.WriteLine($"[AI-RAW] Status: {response.StatusCode} | Body: {responseJson}");
                }
                var groqResponse = JsonConvert.DeserializeObject<GroqResponse>(responseJson);
                var message = groqResponse?.Choices?[0]?.Message;
                var extracted = message?.Content?.Trim();

                // Modelos thinking (ej. gpt-oss:120b-cloud en Ollama) devuelven la respuesta
                // en el campo "reasoning" cuando "content" queda vacío.
                if (string.IsNullOrEmpty(extracted))
                {
                    extracted = message?.Reasoning?.Trim();
                    if (!string.IsNullOrEmpty(extracted) && debugEnabled)
                        Console.WriteLine($"[AI-THINKING] content vacío, usando campo reasoning como fallback");
                }

                if (string.IsNullOrEmpty(extracted) || extracted.Equals("NOT_FOUND", StringComparison.OrdinalIgnoreCase))
                    return null;

                return extracted;
            }
            catch (Exception ex)
            {
                if (IsDebugEnabled()) Console.WriteLine($"[AI-ERROR] Exception: {ex.GetType().Name} | {ex.Message}");
                return null;
            }
        }

        // ── DTOs internos para deserializar la respuesta ──────────────────────────

        private class GroqResponse
        {
            [JsonProperty("choices")]
            public GroqChoice[] Choices { get; set; }
        }

        private class GroqChoice
        {
            [JsonProperty("message")]
            public GroqMessage Message { get; set; }
        }

        private class GroqMessage
        {
            [JsonProperty("content")]
            public string Content { get; set; }

            /// <summary>
            /// Campo usado por modelos de tipo "thinking" (ej. gpt-oss:120b-cloud en Ollama)
            /// cuando devuelven la respuesta en "reasoning" en lugar de "content".
            /// </summary>
            [JsonProperty("reasoning")]
            public string Reasoning { get; set; }
        }
    }
}
