namespace Vtex.SemanticVersioning
{
    internal static class ReSrc
    {
        // ## Numeric Identifier
        // A single `0`, or a non-zero digit followed by zero or more digits.
        public const string NumericIdentifier = "0|[1-9]\\d*";
        public const string NumericIdentifierLoose = "[0-9]+";

        // ## Non-numeric Identifier
        // Zero or more digits, followed by a letter or a hyphen, and then zero or
        // more letters, digits, or hyphens.
        public const string NonNumericIdentifier = "\\d*[a-zA-Z-][a-zA-Z0-9-]*";

        // ## Main Version
        // Three dot-separated numeric identifiers.
        public const string MainVersion = "(" + NumericIdentifier + ")\\." +
                                          "(" + NumericIdentifier + ")\\." +
                                          "(" + NumericIdentifier + ")";

        public const string MainVersionLoose = "(" + NumericIdentifierLoose + ")\\." +
                                               "(" + NumericIdentifierLoose + ")\\." +
                                               "(" + NumericIdentifierLoose + ")";

        // ## Pre-release Version Identifier
        // A numeric identifier, or a non-numeric identifier.
        public const string PrereleaseIdentifier = "(?:" + NumericIdentifier +
                                                   "|" + NonNumericIdentifier + ")";

        public const string PrereleaseIdentifierLoose = "(?:" + NumericIdentifierLoose +
                                                        "|" + NonNumericIdentifier + ")";

        // ## Pre-release Version
        // Hyphen, followed by one or more dot-separated pre-release version
        // identifiers.
        public const string Prerelease = "(?:-(" + PrereleaseIdentifier +
                                         "(?:\\." + PrereleaseIdentifier + ")*))";

        public const string PrereleaseLoose = "(?:-?(" + PrereleaseIdentifierLoose +
                                              "(?:\\." + PrereleaseIdentifierLoose + ")*))";

        // ## Build Metadata Identifier
        // Any combination of digits, letters, or hyphens.
        public const string BuildIdentifier = "[0-9A-Za-z-]+";

        // ## Build Metadata
        // Plus sign, followed by one or more period-separated build metadata
        // identifiers.
        public const string Build = "(?:\\+(" + BuildIdentifier +
                                    "(?:\\." + BuildIdentifier + ")*))";

        // ## Full Version String
        // A main version, followed optionally by a pre-release version and
        // build metadata.

        // Note that the only major, minor, patch and pre-release section of
        // the version string are capturing groups. The build metadata is not a
        // capturing group, because it should not ever be used in version comparison.
        private const string FullPlain = "v?" + MainVersion +
                                         Prerelease + "?" +
                                         Build + "?";

        public const string Full = "^" + FullPlain + "$";

        // like full, but allows v1.2.3 and =1.2.3, which people do sometimes.
        // also, 1.0.0alpha1 (prerelease without the hyphen) which is pretty
        // common in the npm registry.
        private const string LoosePlain = "[v=\\s]*" + MainVersionLoose +
                                          PrereleaseLoose + "?" +
                                          Build + "?";

        public const string Loose = "^" + LoosePlain + "$";

        public const string GtLt = "((?:<|>)?=?)";

        // Something line "2.*" or "1.2.x".
        // Note that "x.x" is a valid xRange identifier, meaning "any version"
        // Only the first item is strictly required.
        private const string XRangeIdentifierLoose = NumericIdentifierLoose + "|x|X|\\*";

        private const string XRangeIdentifier = NumericIdentifier + "|x|X|\\*";

        private const string XRangePlain = "[v=\\s]*(" + XRangeIdentifier + ")" +
                                           "(?:\\.(" + XRangeIdentifier + ")" +
                                           "(?:\\.(" + XRangeIdentifier + ")" +
                                           "(?:(" + Prerelease + ")" +
                                           ")?)?)?";

        private const string XRangePlainLoose = "[v=\\s]*(" + XRangeIdentifierLoose + ")" +
                                           "(?:\\.(" + XRangeIdentifierLoose + ")" +
                                           "(?:\\.(" + XRangeIdentifierLoose + ")" +
                                           "(?:(" + PrereleaseLoose + ")" +
                                           ")?)?)?";

        // >=2.x, for example, means >=2.0.0-0
        // <1.2 would be the same as "<1.0.0-0", though.
        public const string XRange = "^" + GtLt + "\\s*" + XRangePlain + "$";

        public const string XRangeLoose = "^" + GtLt + "\\s*" + XRangePlainLoose + "$";

        // Tilde ranges.
        // Meaning is "reasonably at or greater than"
        private const string LoneTilde = "(?:~>?)";

        public const string TildeTrim = "(\\s*)" + LoneTilde + "\\s+";

        public const string Tilde = "^" + LoneTilde + XRangePlain + "$";
        
        public const string TildeLoose = "^" + LoneTilde + XRangePlainLoose + "$";

        // Caret ranges.
        // Meaning is "at least and backwards compatible with"
        private const string LoneCaret = "(?:\\^)";

        public const string CaretTrim = "(\\s*)" + LoneCaret + "\\s+";

        public const string Caret = "^" + LoneCaret + XRangePlain + "$";
        
        public const string CaretLoose = "^" + LoneCaret + XRangePlainLoose + "$";

        // A simple gt/lt/eq thing, or just "" to indicate "any version"
        public const string ComparatorLoose = "^" + GtLt + "\\s*(" + LoosePlain + ")$|^$";

        public const string Comparator = "^" + GtLt + "\\s*(" + FullPlain + ")$|^$";

        // An expression to strip any whitespace between the gtlt and the thing
        // it modifies, so that `> 1.2.3` => `>1.2.3`
        public const string ComparatorTrim = "(\\s*)" + GtLt +
                                             "\\s*(" + LoosePlain + "|" + XRangePlain + ")";

        // Something like `1.2.3 - 1.2.4`
        // Note that these all use the loose form, because they'll be
        // checked against either the strict or loose comparator form later.
        public const string HyphenRange = "^\\s*(" + XRangePlain + ")" +
                                          "\\s+-\\s+" +
                                          "(" + XRangePlain + ")" +
                                          "\\s*$";

        public const string HyphenRangeLoose = "^\\s*(" + XRangePlainLoose + ")" +
                                               "\\s+-\\s+" +
                                               "(" + XRangePlainLoose + ")" +
                                               "\\s*$";

        // Star ranges basically just allow anything at all.
        public const string Star = "(<|>)?=?\\s*\\*";
    }
}