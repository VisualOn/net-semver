using System.Text.RegularExpressions;

namespace Vtex.SemanticVersioning
{
    internal static class Re
    {
        public static Regex Full = new Regex(ReSrc.Full, RegexOptions.Compiled);
        public static Regex Loose = new Regex(ReSrc.Loose, RegexOptions.Compiled);
        public static Regex Comparator = new Regex(ReSrc.Comparator, RegexOptions.Compiled);
        public static Regex ComparatorLoose = new Regex(ReSrc.ComparatorLoose, RegexOptions.Compiled);
        public static Regex HyphenRangeLoose = new Regex(ReSrc.HyphenRangeLoose, RegexOptions.Compiled);
        public static Regex HyphenRange = new Regex(ReSrc.HyphenRange, RegexOptions.Compiled);
        public static Regex ComparatorTrim = new Regex(ReSrc.ComparatorTrim, RegexOptions.Compiled);
        public static Regex XRange = new Regex(ReSrc.XRange, RegexOptions.Compiled);
        public static Regex XRangeLoose = new Regex(ReSrc.XRangeLoose, RegexOptions.Compiled);
        public static Regex TildeTrim = new Regex(ReSrc.TildeTrim, RegexOptions.Compiled);
        public static Regex Tilde = new Regex(ReSrc.Tilde, RegexOptions.Compiled);
        public static Regex TildeLoose = new Regex(ReSrc.TildeLoose, RegexOptions.Compiled);
        public static Regex CaretTrim = new Regex(ReSrc.CaretTrim, RegexOptions.Compiled);
        public static Regex Caret = new Regex(ReSrc.Caret, RegexOptions.Compiled);
        public static Regex CaretLoose = new Regex(ReSrc.CaretLoose, RegexOptions.Compiled);
        public static Regex Star = new Regex(ReSrc.Star, RegexOptions.Compiled);
    }
}
