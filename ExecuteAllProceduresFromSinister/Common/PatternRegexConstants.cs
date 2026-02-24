namespace ExecuteAllProceduresFromSinister.Common
{
    public static class PatternRegexConstants
    {
        public const string CaseElevenV2 = @"n\/ref.\ [a-z0-9]*";
        public const string CaseElevenV3 = @"n\/sin.\ [a-z0-9]*";
        public const string CaseFifteen = @"siniestro [a-z0-9]*";
        public const string CaseThiryTwo = @"[0-9]*-[0-9]*-[0-9]*";
        public const string CaseThirtyFour = @"[a-z0-9]*\/.*\/.*";
        public const string CaseThiryFive = @"s-[a-z0-9]*";
        public const string CaseThirtySix = @"siniestro: [a-z0-9]*";
        public const string CaseThirtySixV2 = @"siniestro:[a-z0-9]*";
        public const string CaseThirtyThree = @"[a-z0-9]*\/";
        public const string CaseThirtyEight = @"referencia [a-z0-9]*";
        public const string CaseFourty = @"expediente: [a-z0-9]*";
        public const string CaseFourtyOne = @"siniestro nº [a-z0-9]*";
        public const string CaseFourtyTwo = @"referencia [a-z0-9]*";
        public const string CaseFourtyThree = @"sin\. vseg: [0-9]*";
        public const string CaseFourtyThreeV2 = @"^vst[0-9]*";
        public const string CaseFourtyThreeV3 = @"^vs[0-9]*";
        public const string CaseRefSin = @"ref\. siniestro:[a-z0-9]*";
        public const string CaseRefSinWithoutSpace = @"ref.siniestro:[a-z0-9]*";
        public const string CaseRefSinWithoutDot = @"ref siniestro:[a-z0-9]*";
        public const string CaseSinisterWithSpace = @"nº siniestro :[a-z0-9]*";
        public const string CaseSinister = @"nº siniestro: [a-z0-9]*";
        public const string CaseSinisterWithoutSpace = @"nº siniestro:[a-z0-9]*";
        public const string CaseSinisterZurich = @"siniestro de zurich n\.º [a-z0-9]*";
        public const string CaseNumberReference = @"n\/referencia [a-z0-9]*";
        public const string CaseRefMP = @"ref as: [a-z0-9]*";
        public const string CaseGuion = @"- [a-z0-9]* -";
        public const string CaseReference = @"reference [a-z0-9]*";

        // ── Caso 104: QualitasAutoClassic (referencia tras /) ────────────────────
        public const string CaseSlashRef = @"\/[a-z0-9]+";

        // ── Casos 121-122: Zurich (siniestro núm.) ────────────────────────────────
        public const string CaseSinisterNum = @"siniestro n[uú]m\. [a-z0-9]+";

        // ── Casos 124-133: FIATC (#referencia) ───────────────────────────────────
        public const string CaseHashRef = @"#[a-z0-9]+";

        // ── Caso 134: Mutua de Propietarios ("Siniestro : xxx") ───────────────────
        public const string CaseSiniestroSpaceColon = @"siniestro : [a-z0-9]+";

        // ── Casos 150-151: Generali (asunto = referencia numérica) ────────────────
        public const string CaseOnlyNumbers = @"^[0-9]+$";

        // ── Caso Generali TMT: 148614271/TMT(RD023)(085182559) ──────────────────
        public const string CaseGeneralionTmt = @"[0-9]+\/TMT";
    }
}
