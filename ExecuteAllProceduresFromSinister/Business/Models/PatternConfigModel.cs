using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExecuteAllProceduresFromSinister.Business.Models
{
    public class PatternsConfig
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("specificEmails")]
        public List<SpecificEmailConfig> SpecificEmails { get; set; } = new List<SpecificEmailConfig>();

        [JsonPropertyName("domains")]
        public List<DomainConfig> Domains { get; set; } = new List<DomainConfig>();

        [JsonPropertyName("genericSubjectCases")]
        public List<CaseConfig> GenericSubjectCases { get; set; } = new List<CaseConfig>();
    }

    public class SpecificEmailConfig
    {
        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("cases")]
        public List<CaseConfig> Cases { get; set; } = new List<CaseConfig>();
    }

    public class DomainConfig
    {
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("canonicalDomain")]
        public string CanonicalDomain { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("cases")]
        public List<CaseConfig> Cases { get; set; } = new List<CaseConfig>();
    }

    public class CaseConfig
    {
        [JsonPropertyName("keyword")]
        public string Keyword { get; set; }

        [JsonPropertyName("helper")]
        public string Helper { get; set; }

        [JsonPropertyName("regex")]
        public string Regex { get; set; }

        [JsonPropertyName("containMail")]
        public string ContainMail { get; set; }

        [JsonPropertyName("isGenericTask")]
        public bool IsGenericTask { get; set; }

        [JsonPropertyName("onlyLoad")]
        public bool OnlyLoad { get; set; }
    }
}
