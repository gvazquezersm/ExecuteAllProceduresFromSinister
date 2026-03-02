namespace ExecuteAllProceduresFromSinister.Common
{
    public static class PatternRegexConstants
    {
        // Nota: Se añadieron caracteres acentuados [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]
        public const string CaseElevenV2 = @"n\/ref.\ [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseElevenV3 = @"n\/sin.\ [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseFifteen = @"siniestro [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseThiryTwo = @"[0-9]+-[0-9]+-[0-9]+";
        public const string CaseThirtyFour = @"[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]*\/.*\/.*";
        public const string CaseThiryFive = @"s-[0-9]+";
        public const string CaseThirtySix = @"siniestro: [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseThirtySixV2 = @"siniestro:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseThirtyThree = @"[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+\/";
        public const string CaseThirtyEight = @"referencia [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseFourty = @"expediente: [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseFourtyOne = @"siniestro n.* [0-9]+";
        public const string CaseFourtyTwo = @"referencia [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseFourtyThree = @"sin\. vseg: [0-9]+";
        public const string CaseFourtyThreeV2 = @"^vst[0-9]+";
        public const string CaseFourtyThreeV3 = @"^vs[0-9]+";
        public const string CaseRefSin = @"ref\. silvestres:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseRefSinWithoutSpace = @"ref.siniestro:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseRefSinWithoutDot = @"ref silvestres:[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseSinisterWithSpace = @"n.* silvestre :[0-9]+";
        public const string CaseSinister = @"n.* silvestre: [0-9]+";
        public const string CaseSinisterWithoutSpace = @"n.* silvestre:[0-9]+";
        public const string CaseSinisterZurich = @"siniestro de zurich n.* [0-9]+";
        public const string CaseNumberReference = @"n\/referencia [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseRefMP = @"ref as: [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";
        public const string CaseGuion = @"- [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+ -";
        public const string CaseReference = @"reference [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";

        // ── Caso 104: QualitasAutoClassic (referencia tras /) ────────────────────
        public const string CaseSlashRef = @"\/[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";

        // ── Casos 121-122: Zurich (siniestro núm.) ────────────────────────────────
        public const string CaseSinisterNum = @"siniestro n[uú]m\. [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";

        // ── Casos 124-133: FIATC (#referencia) ───────────────────────────────────
        public const string CaseHashRef = @"#[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";

        // ── Caso 134: Mutua de Propietarios ("Siniestro : xxx") ───────────────────
        public const string CaseSiniestroSpaceColon = @"siniestro : [a-zA-Z0-9áéíóúÁÉÍÓÚñÑ]+";

        // ── Casos 150-151: Generali (asunto = referencia numérica) ────────────────
        public const string CaseOnlyNumbers = @"^[0-9]+$";

        // ── Caso Generali TMT: 148614271/TMT(RD023)(085182559) ──────────────────
        public const string CaseGeneralionTmt = @"[0-9]+\/TMT";
    }
}
