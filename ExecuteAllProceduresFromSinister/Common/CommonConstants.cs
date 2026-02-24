using System;

namespace ExecuteAllProceduresFromSinister.Common
{
    public static class CommonConstants
    {
        //public const string BaseUrl = "https://app.rsmseguros.es:444/app/";

        // FORMA SEGURA: Si no hay variable, explota. No asumas Producción.
        public static string BaseUrl => Environment.GetEnvironmentVariable("BaseUrl")
            ?? throw new Exception("CRÍTICO: No se ha definido BaseUrl. Bloqueando conexión para evitar filtraciones. -- \"https://app.rsmseguros.es:444/app/\" --");

        
        public const string MediaType = "application/json";
        public const string EndpointGetSinisterData = "api/AutomationMail/GetSinisterData";
        public const string EndpointLoadFileToDocumentalLink = "api/AutomationMail/LoadFileToDocumentalLink";
        public const string EndpointCreateTaskNewEmailCia = "api/AutomationMail/CreateTaskNewEmailCia";
        public const string EndpointCreateTaskNewEmailGeneric = "api/AutomationMail/CreateTaskNewEmailGeneric";
        public const string EndpointUpdateCountMail = "api/AutomationMail/UpdateCountMail";
        public const string EndpointAddLogLogicApps = "api/AutomationMail/AddLogsLogicApps";

        public const string BasicAuth = "Authentication";
        public const string BasicValue = "ZXJzbUF1dGhlbnRpY2F0aW9uOmVyc200NDUl";

        public const string TwoPoints = ":";
        public const string Dash = "-";
        public const string OpenClaudator = "[";
        public const string CloseClaudator = "]";
        public const string WhiteSpace = " ";
        public const string Forwarding = "RV:";
        public const string Dot = ".";
        public const string Success = "Success";
        public const string NotProcessed = "Not processed";
        public const string ExceptionCase = "Exception case";
        public const string Pdf = "pdf";
        public const string Docx = "docx";
        public const string Doc = "doc";
        public const string Txt = "txt";
        public const string DotComma = ";";
    }
}
