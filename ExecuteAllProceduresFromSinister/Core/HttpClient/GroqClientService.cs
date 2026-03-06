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
        private const string GroqApiUrl = "https://api.groq.com/openai/v1/chat/completions";
        private const string Model = "llama-3.1-8b-instant";

        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

        /// <summary>
        /// Llama a Groq para extraer el identificador de siniestro del asunto del email.
        /// Devuelve null si no encuentra identificador o si la llamada falla.
        /// </summary>
        public static async Task<string> ExtractSinisterReferenceAsync(string subject, string originMail)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("GroqApiKey");
                if (string.IsNullOrEmpty(apiKey)) return null;

                var prompt = $@"Eres un extractor de referencias de siniestros de seguros.
Del siguiente asunto de email, extrae ÚNICAMENTE el identificador del siniestro.
El identificador suele ser un número, código alfanumérico, o referencia con formato como: 12345678, S-12345678, AU04 2026/12345, #12345678, VST12345, etc.
Si no encuentras ningún identificador claro, responde exactamente con: NOT_FOUND
Responde SOLO con el identificador, sin texto adicional, sin explicaciones.

Remitente: {originMail}
Asunto: {subject}";

                var requestBody = new
                {
                    model = Model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0,
                    max_tokens = 50
                };

                var json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var request = new HttpRequestMessage(HttpMethod.Post, GroqApiUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                request.Content = content;

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return null;

                var responseJson = await response.Content.ReadAsStringAsync();
                var groqResponse = JsonConvert.DeserializeObject<GroqResponse>(responseJson);
                var extracted = groqResponse?.Choices?[0]?.Message?.Content?.Trim();

                if (string.IsNullOrEmpty(extracted) || extracted.Equals("NOT_FOUND", StringComparison.OrdinalIgnoreCase))
                    return null;

                return extracted;
            }
            catch
            {
                return null;
            }
        }

        // ── DTOs internos para deserializar la respuesta de Groq ─────────────────

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
        }
    }
}
