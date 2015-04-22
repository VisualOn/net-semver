using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Vtex.SemanticVersioning
{
    /// <summary>
    /// A range of semantic versions.
    /// </summary>
    public class Range
    {
        private readonly bool _loose;
        private readonly Comparator[][] _set;
        private readonly string _source;
        
        private Range(Comparator[][] comparatorSets,  bool loose, string source)
        {
            _set = comparatorSets;
            _loose = loose;
            _source = source;
            _loose = loose;

            _source = string.Join("||", _set.Select(comps => string.Join(" ", comps.Select(c => c.ToString()))));
        }

        /// <summary>
        /// Converts the specified string representation of a version range to its
        /// <see cref="T:Vtex.SemanticVersioning.Range"/> equivalent.
        /// </summary>
        /// <param name="source">The string representation of the range</param>
        /// <param name="loose">Whether to use loose mode or not</param>
        /// <returns>The parsed <see cref="T:Vtex.SemanticVersioning.Range"/></returns>
        /// <exception cref="T:System.FormatException"><paramref name="source"/> is not a valid SemVer Range</exception>
        public static Range Parse(string source, bool loose = false)
        {
            Range range;
            if (TryParse(source, out range, loose))
                return range;

            throw new FormatException("Invalid SemVer Range: " + source);
        }

        /// <summary>
        /// Tries to convert the specified string representation of a version range to its
        /// <see cref="T:Vtex.SemanticVersioning.Range"/> equivalent. A return value indicates whether the conversion
        /// succeeded or failed.
        /// </summary>
        /// <param name="source">The string representation of the range</param>
        /// <param name="range">
        /// When this method returns, contains a <see cref="T:Vtex.SemanticVersioning.Range"/> if the conversion succeeded.
        /// </param>
        /// <param name="loose">Whether to use loose mode or not</param>
        /// <returns>true if <paramref name="source"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string source, out Range range, bool loose = false)
        {
            var sets = Regex.Split(source, @"\s*\|\|\s*")
                .Select(comparatorSet => ParseSet(comparatorSet, loose))
                .Where(comparators => comparators.Length > 0)
                .ToArray();

            if (sets.Any())
            {
                range = new Range(sets, loose, source);
                return true;
            }

            range = null;
            return false;
        }

        /// <summary>
        /// Returns whether the given version satisfies thing range.
        /// </summary>
        /// <param name="version">The <see cref="T:Vtex.SemanticVersioning.Version"/> to be matched</param>
        /// <returns>true if the <paramref name="version"/> matches this range; false otherwise.</returns>
        public bool Matches(Version version)
        {
            if(ReferenceEquals(version, null))
                throw new ArgumentNullException("version");

            return _set.Any(comparators => AllMatch(comparators, version));
        }

        private static bool AllMatch(IEnumerable<Comparator> comparators, Version version)
        {
            return comparators.All(comp => comp.Matches(version));
        }

        public override string ToString()
        {
            return _source;
        }

        private static Comparator[] ParseSet(string range, bool loose)
        {
            range = range.Trim();

            // `1.2.3 - 1.2.4` => `>=1.2.3 <= 1.2.4`
            var hyphenRangeRegex = loose ? Re.HyphenRangeLoose : Re.HyphenRange; // get hyphen replace Regex
            range = hyphenRangeRegex.Replace(range, HyphenReplace);

            // `< 1.2.3 < 1.2.5` => `>1.2.3 <1.2.5`
            range = Re.ComparatorTrim.Replace(range, "$1$2$3");

            // `~ 1.2.3` => `~1.2.3`
            range = Re.TildeTrim.Replace(range, "$1~");

            // `^ 1.2.3` => `^1.2.3`
            range = Re.CaretTrim.Replace(range, "$1^");

            // normalize spaces
            range = string.Join(" ", Regex.Split(range, "\\s+"));

            // At this point, the range is completely trimmed and
            // ready to be split into comparators.

            var compRegex = loose ? Re.ComparatorLoose : Re.Comparator;
            var comps = Regex.Split(string.Join(" ", range.Split(' ').Select(comp => TrimComparator(comp, loose))),
                "\\s+").Select(comp => new { Source = comp, Match = compRegex.Match(comp) });
            if (loose)
            {
                // in loose mode, throw out any that are not valid comparators
                comps = comps.Where(comp => comp.Match.Success).ToArray();
            }

            return comps.Select(comp => Comparator.Parse(comp.Match, comp.Source, loose)).ToArray();
        }

        private static string TrimComparator(string comp, bool loose)
        {
            comp = ReplaceCarets(comp, loose);
            comp = ReplaceTildes(comp, loose);
            comp = ReplaceXRanges(comp, loose);
            comp = ReplaceStars(comp, loose);
            return comp;
        }

        // ^ --> * (any, kinda silly)
        // ^2, ^2.x, ^2.x.x --> >=2.0.0 <3.0.0
        // ^2.0, ^2.0.x --> >=2.0.0 <3.0.0
        // ^1.2, ^1.2.x --> >=1.2.0 <2.0.0
        // ^1.2.3 --> >=1.2.3 <2.0.0
        // ^1.2.0 --> >=1.2.0 <2.0.0
        private static string ReplaceCarets(string comp, bool loose)
        {
            return string.Join(" ", Regex.Split(comp, "\\s+").Select(c => ReplaceCaret(c, loose)));
        }

        private static string ReplaceCaret(string comp, bool loose)
        {
            var regex = loose ? Re.CaretLoose : Re.Caret;
            return regex.Replace(comp, match =>
            {
                var major = match.Groups[1].Value;
                var minor = match.Groups[2].Value;
                var patch = match.Groups[3].Value;
                var prerelease = match.Groups[4].Value;

                string ret;
                if (IsX(major))
                    ret = "";
                else if (IsX(minor))
                    ret = ">=" + major + ".0.0-0 <" + Inc(major) + ".0.0-0";
                else if (IsX(patch))
                {
                    if (major == "0")
                        ret = ">=" + major + "." + minor + ".0-0 <" + major + "." + Inc(minor) + ".0-0";
                    else
                        ret = ">=" + major + "." + minor + ".0-0 <" + Inc(major) + ".0.0-0";
                }
                else if (!string.IsNullOrWhiteSpace(prerelease))
                {
                    if (prerelease[0] != '-')
                        prerelease = "-" + prerelease;
                    if (major == "0")
                    {
                        if (minor == "0")
                            ret = "=" + major + "." + minor + "." + patch + prerelease;
                        else
                            ret = ">=" + major + "." + minor + "." + patch + prerelease +
                                  " <" + major + "." + Inc(minor) + ".0-0";
                    }
                    else
                    {
                        ret = ">=" + major + "." + minor + "." + patch + prerelease +
                              " <" + Inc(major) + ".0.0-0";
                    }
                }
                else
                {
                    if (major == "")
                    {
                        if (minor == "0")
                            ret = "=" + major + "." + minor + "." + patch;
                        else
                            ret = ">=" + major + ">" + minor + "." + patch + "-0" +
                                  " <" + major + "." + Inc(minor) + ".0-0";
                    }
                    else
                    {
                        ret = ">=" + major + "." + minor + "." + patch + "-0" +
                              " <" + Inc(major) + ".0.0-0";
                    }
                }

                return ret;
            });
        }

        // ~, ~> --> * (any, kinda silly)
        // ~2, ~2.x, ~2.x.x, ~>2, ~>2.x ~>2.x.x --> >=2.0.0 <3.0.0
        // ~2.0, ~2.0.x, ~>2.0, ~>2.0.x --> >=2.0.0 <2.1.0
        // ~1.2, ~1.2.x, ~>1.2, ~>1.2.x --> >=1.2.0 <1.3.0
        // ~1.2.3, ~>1.2.3 --> >=1.2.3 <1.3.0
        // ~1.2.0, ~>1.2.0 --> >=1.2.0 <1.3.0
        private static string ReplaceTildes(string comp, bool loose)
        {
            return string.Join(" ", Regex.Split(comp, "\\s+").Select(c => ReplaceTilde(c, loose)));
        }

        private static string ReplaceTilde(string comp, bool loose)
        {
            var regex = loose ? Re.TildeLoose : Re.Tilde;
            return regex.Replace(comp, match =>
            {
                var major = match.Groups[1].Value;
                var minor = match.Groups[2].Value;
                var patch = match.Groups[3].Value;
                var prerelease = match.Groups[4].Value;

                string ret;
                if (IsX(major))
                    ret = "";
                else if (IsX(minor))
                    ret = ">=" + major + ".0.0-0 <" + Inc(major) + ".0.0-0";
                else if (IsX(patch))
                    // ~1.2 == >=1.2.0- <1.3.0-
                    ret = ">=" + major + "." + minor + ".0-0 <" + major + "." + Inc(minor) + ".0-0";
                else if (!string.IsNullOrWhiteSpace(prerelease))
                {
                    if (prerelease[0] != '-')
                        prerelease = "-" + prerelease;
                    ret = ">=" + major + "." + minor + "." + patch + prerelease +
                          " <" + major + "." + Inc(minor) + ".0-0";
                }
                else
                {
                    // ~1.2.3 == >=1.2.3-0 <1.3.0-0
                    ret = ">=" + major + "." + minor + "." + patch + "-0" +
                          " <" + major + "." + Inc(minor) + ".0-0";
                }

                return ret;
            });
        }

        private static string ReplaceXRanges(string comp, bool loose)
        {
            return string.Join(" ", Regex.Split(comp, "\\s+").Select(c => ReplaceXRange(c, loose)));
        }

        private static string ReplaceXRange(string comp, bool loose)
        {
            var regex = loose ? Re.XRangeLoose : Re.XRange;
            return regex.Replace(comp, match =>
            {
                var gtlt = match.Groups[1].Value;
                var major = match.Groups[2].Value;
                var minor = match.Groups[3].Value;
                var patch = match.Groups[4].Value;
                var prerelease = match.Groups[5].Value;

                var xMajor = IsX(major);
                var xMinor = xMajor || IsX(minor);
                var xPatch = xMinor || IsX(patch);
                var anyX = xPatch;

                if (gtlt == "=" && anyX)
                    gtlt = "";

                var result = match.Groups[0].Value;

                if (!string.IsNullOrWhiteSpace(gtlt) && anyX)
                {
                    // replace X with 0, and then append the -0 min-prerelease
                    if (xMajor)
                        major = "0";
                    if (xMinor)
                        minor = "0";
                    if (xPatch)
                        patch = "0";

                    if (gtlt == ">")
                    {
                        // >1 => >=2.0.0-0
                        // >1.2 => >=1.3.0-0
                        // >1.2.3 => >= 1.2.4-0
                        gtlt = ">=";
                        if (!Truthy(major))
                        {
                            // no change
                        }
                        else if (xMinor)
                        {
                            major = Inc(major) + "";
                            minor = "0";
                            patch = "0";
                        }
                        else if (xPatch)
                        {
                            minor = Inc(minor) + "";
                            patch = "0";
                        }
                    }

                    result = gtlt + major + "." + minor + "." + patch + "-0";
                }

                else if (xMajor)
                    // allow any
                    result = "*";

                else if (xMinor)
                    // append '-0' onto the version, otherwise
                    // '1.x.x' matches '2.0.0-beta', since the tag
                    // *lowers* the version value
                    result = ">=" + major + ".0.0-0 <" + Inc(major) + ".0.0-0";

                else if (xPatch)
                    result = ">=" + major + "." + minor + ".0-0 <" + major + "." + Inc(minor) + ".0-0";

                return result;
            });
        }

        // Because * is AND-ed with everything else in the comparator,
        // and '' means "any version", just remove the *s entirely.
        private static string ReplaceStars(string comp, bool loose)
        {
            return Re.Star.Replace(comp.Trim(), "");
        }

        private static string HyphenReplace(Match match)
        {
            var values = match.Groups.Cast<Group>().Select(group => group.Value).ToArray();
            return HyphenReplace(values[0],
                values[1], values[2], values[3], values[4], values[5], values[6],
                values[7], values[8], values[9], values[10], values[11], values[12]);
        }

        private static string HyphenReplace(string match,
            string from, string fromMajor, string fromMinor, string fromPatch, string fromPrerelease, string fromBuild,
            string to, string toMajor, string toMinor, string toPatch, string toPrerelease, string toBuild)
        {
            if (IsX(fromMajor))
                from = "";
            else if (IsX(fromMinor))
                from = ">=" + fromMajor + ".0.0-0";
            else if (IsX(fromPatch))
                from = ">=" + fromMajor + "." + fromMinor + ".0-0";
            else
                from = ">=" + from;

            if (IsX(toMajor))
                to = "";
            else if (IsX(toMinor))
                to = "<" + Inc(toMajor) + ".0.0-0";
            else if (IsX(toPatch))
                to = "<" + toMajor + "." + Inc(toMinor) + ".0-0";
            else if (!string.IsNullOrWhiteSpace(toPrerelease))
                to = "<=" + toMajor + "." + toMinor + "." + toPatch + "-" + toPrerelease;
            else
                to = "<=" + to;

            return (from + " " + to).Trim();
        }

        private static int Inc(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? 1 : int.Parse(id) + 1;
        }

        private static bool Truthy(string id)
        {
            return id != null && !string.IsNullOrWhiteSpace(id) && id.Any(c => c != '0');
        }

        private static bool IsX(string id)
        {
            return string.IsNullOrWhiteSpace(id) || id.ToLowerInvariant() == "x" || id == "*";
        }
    }
}
