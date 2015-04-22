using System;
using System.Text.RegularExpressions;

namespace Vtex.SemanticVersioning
{
    /// <summary>
    /// Can compare versions based on an operator.
    /// </summary>
    public class Comparator
    {
        private static readonly Version Any = new Version(0, 0, 0);
        private readonly Version _semver;

        /// <summary>
        /// The operator to be used for the comparison.
        /// </summary>
        public string Operator { get; private set; }

        /// <summary>
        /// The version on which to base the comparison.
        /// </summary>
        public string Version { get; private set; }

        private Comparator(string op, Version version)
        {
            this.Operator = op;
            this.Version = ReferenceEquals(version, Any) ? string.Empty : version.ToString();
            _semver = version;
        }

        /// <summary>
        /// Converts the specified string representation of a comparator to its
        /// <see cref="T:Vtex.SemanticVersioning.Comparator"/> equivalent.
        /// </summary>
        /// <param name="source">The string representation of the comparator</param>
        /// <param name="loose">Whether to use loose mode or not</param>
        /// <returns>The parsed <see cref="SemanticVersioning.Comparator"/></returns>
        /// <exception cref="T:System.FormatException"><paramref name="source"/> is not a valid comparator</exception>
        public static Comparator Parse(string source, bool loose = false)
        {
            Comparator comparator;
            if (TryParse(source, out comparator, loose))
                return comparator;

            throw new FormatException("Invalid comparator: " + source);
        }

        /// <summary>
        /// Tries to convert the specified string representation of a comparator to its
        /// <see cref="T:Vtex.SemanticVersioning.Comparator"/> equivalent. A return value indicates whether the conversion
        /// succeeded or failed.
        /// </summary>
        /// <param name="source">The string representation of the range</param>
        /// <param name="comparator">
        /// When this method returns, contains a <see cref="T:Vtex.SemanticVersioning.Comparator"/> if the conversion succeeded.
        /// </param>
        /// <param name="loose">Whether to use loose mode or not</param>
        /// <returns>true if <paramref name="source"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string source, out Comparator comparator, bool loose = false)
        {
            var regex = loose ? Re.ComparatorLoose : Re.Comparator;
            var match = regex.Match(source);

            if (!match.Success)
            {
                comparator = null;
                return false;
            }
            comparator = Parse(match, source, loose);
            return true;
        }

        internal static Comparator Parse(Match match, string source, bool loose = false)
        {
            if(!match.Success)
                throw new FormatException("Invalid comparator: " + source);

            var op = match.Groups[1].Value;
            Version version;

            // if it literally is just '>' or '' then allow anything
            if (!match.Groups[2].Success)
                version = Any;
            else
            {
                version = SemanticVersioning.Version.Parse(match.Groups[2].Value, loose);

                // <1.2.3-rc DOES allow 1.2.3-beta (has prerelease)
                // >=1.2.3 DOES NOT allow 1.2.3-beta
                // <=1.2.3 DOES allow 1.2.3-beta
                // However, <1.2.3 does NOT allow 1.2.3-bet_semver,
                // even though `1.2.3-beta < 1.2.3`
                // The assumption is that the 1.2.3 version has something you
                // *don't* want, so we push the prerelease down to the minimum.
                if (op == "<" && version.Prerelease.Length == 0)
                {
                    version.Prerelease = new VersionIdentifier[] {0};
                }
            }

            return new Comparator(op, version);
        }

        /// <summary>
        /// Returns whether the given version satisfies this comparator.
        /// </summary>
        /// <param name="version">The <see cref="T:Vtex.SemanticVersioning.Version"/> to be matched</param>
        /// <returns>true if the <paramref name="version"/> matches this comparator; false otherwise.</returns>
        public bool Matches(Version version)
        {
            return ReferenceEquals(_semver, Any) || CompareTo(version);
        }

        private bool CompareTo(Version other)
        {
            switch (this.Operator)
            {
                case "===":
                    return ReferenceEquals(other, _semver);
                case "!==":
                    return !ReferenceEquals(other, _semver);
                case "!=":
                    return other != _semver;
                case ">":
                    return other > _semver;
                case ">=":
                    return other >= _semver;
                case "<":
                    return other < _semver;
                case "<=":
                    return other <= _semver;
                case "":
                case "=":
                case "==":
                    return other == _semver;
                default:
                    throw new FormatException("Invalid operator: " + this.Operator);
            }
        }
        
        public override string ToString()
        {
            return Version;
        }
    }
}
