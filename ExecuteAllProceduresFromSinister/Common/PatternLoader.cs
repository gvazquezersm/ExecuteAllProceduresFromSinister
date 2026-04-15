using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using ExecuteAllProceduresFromSinister.Business.Models;

namespace ExecuteAllProceduresFromSinister.Common
{
    public static class PatternLoader
    {
        private static readonly Dictionary<string, Func<string, string, string>> HelperMap =
            new Dictionary<string, Func<string, string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["GetRefFromSubjectReplaced"]          = Helpers.GetRefFromSubjectReplaced,
                ["GetFirstElementSplitFromSubject"]    = Helpers.GetFirstElementSplitFromSubject,
                ["GetRefSinisterFromSubjectBetweenDash"] = Helpers.GetRefSinisterFromSubjectBetweenDash,
                ["GetRefFromLastDashSubject"]          = Helpers.GetRefFromLastDashSubject,
                ["GetSinisterClaudatorsFromSubject"]   = Helpers.GetSinisterClaudatorsFromSubject,
                ["GetSubjectTrimmed"]                  = Helpers.GetSubjectTrimmed,
                ["GetLastTokenFromMatch"]              = Helpers.GetLastTokenFromMatch,
                ["GetRefFromDeletingElementsAndJoin"]  = Helpers.GetRefFromDeletingElementsAndJoin,
                ["GetRefFromInitStringToCaseString"]   = Helpers.GetRefFromInitStringToCaseString,
            };

        private static PatternsConfig _cache;
        private static readonly object _lock = new object();

        public static void InvalidateCache()
        {
            lock (_lock) { _cache = null; }
        }

        private static PatternsConfig LoadConfig()
        {
            if (_cache != null) return _cache;
            lock (_lock)
            {
                if (_cache != null) return _cache;
                // El runtime de Azure Functions local pone la DLL en bin\Debug\netcoreapp3.1\bin\
                // pero los archivos de contenido (CopyToOutputDirectory) van a bin\Debug\netcoreapp3.1\
                // Por eso buscamos en la carpeta del assembly y subimos un nivel si no lo encuentra.
                var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var path = Path.Combine(assemblyDir, "patterns.json");
                if (!File.Exists(path))
                    path = Path.Combine(Path.GetDirectoryName(assemblyDir), "patterns.json");
                if (!File.Exists(path))
                    throw new FileNotFoundException($"patterns.json not found. Last path tried: {path}");
                var json = File.ReadAllText(path);
                _cache = JsonSerializer.Deserialize<PatternsConfig>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return _cache;
            }
        }

        public static List<DataGenericMailModel<IEnumerable<DataActionMailModel>>> GetSpecificEmailPatterns()
        {
            return LoadConfig().SpecificEmails
                .Select(e => new DataGenericMailModel<IEnumerable<DataActionMailModel>>
                {
                    Case = e.Email,
                    Data = e.Cases.Select(ToActionModel).ToList()
                }).ToList();
        }

        public static List<DataGenericMailModel<IEnumerable<DataDomainMailModel>>> GetDomainPatterns()
        {
            return LoadConfig().Domains
                .Select(d => new DataGenericMailModel<IEnumerable<DataDomainMailModel>>
                {
                    Case = d.Domain,
                    CanonicalDomain = d.CanonicalDomain,
                    Data = d.Cases.Select(ToDomainModel).ToList()
                }).ToList();
        }

        public static IEnumerable<DataDomainMailModel> GetGenericSubjectPatterns()
        {
            return LoadConfig().GenericSubjectCases.Select(ToDomainModel).ToList();
        }

        private static DataActionMailModel ToActionModel(CaseConfig c) =>
            new DataActionMailModel(c.Keyword, ResolveHelper(c.Helper), c.OnlyLoad, c.IsGenericTask);

        private static DataDomainMailModel ToDomainModel(CaseConfig c) =>
            new DataDomainMailModel(c.Keyword, ResolveHelper(c.Helper), c.ContainMail, c.Regex, c.OnlyLoad, c.IsGenericTask, c.StrictStart);

        private static Func<string, string, string> ResolveHelper(string name)
        {
            if (HelperMap.TryGetValue(name ?? string.Empty, out var fn)) return fn;
            return Helpers.GetRefFromSubjectReplaced;
        }
    }
}
