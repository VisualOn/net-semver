﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SemanticVersioning
{
    public class SemVer : ICloneable, IEquatable<SemVer>
    {
        private readonly bool _loose;
        private readonly string _raw;
        private string _version;

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public object[] Prerelease { get; set; }
        public string[] Build { get; set; }

        protected SemVer()
        {
        }

        public SemVer(SemVer version)
        {
            _loose = version._loose;
            _raw = version._raw;
            _version = version._version;
            Major = version.Major;
            Minor = version.Minor;
            Patch = version.Patch;
            Prerelease = (object[]) version.Prerelease.Clone();
            Build = (string[]) version.Build.Clone();
        }

        public SemVer(string version, bool loose = false)
        {
            _loose = loose;
            var match = (loose ? Re.Loose : Re.Full).Match(version.Trim());

            if (!match.Success)
                throw new FormatException("Invalid Version: " + version);

            _raw = version;

            Major = int.Parse(match.Groups[1].Value);
            Minor = int.Parse(match.Groups[2].Value);
            Patch = int.Parse(match.Groups[3].Value);

            if (!match.Groups[4].Success)
                Prerelease = new object[] {};
            else
                Prerelease = match.Groups[4].Value.Split('.').Select(id =>
                    Re.Integer.IsMatch(id) ? int.Parse(id) : (object) id).ToArray();

            Build = match.Groups[5].Success
                ? match.Groups[5].Value.Split('.').ToArray()
                : new string[] {};

            Format();
        }

        public static SemVer Parse(string version, bool loose = false)
        {
            var regex = loose ? Re.Loose : Re.Full;
            return regex.IsMatch(version) ? new SemVer(version, loose) : null;
        }

        public static string Valid(string version, bool loose = false)
        {
            var semver = Parse(version, loose);
            return !ReferenceEquals(semver, null) ? semver._version : null;
        }

        public static string Clean(string version, bool loose = false)
        {
            var semver = Parse(version, loose);
            return !ReferenceEquals(semver, null) ? semver._version : null;
        }

        public string Format()
        {
            _version = Major + "." + Minor + "." + Patch;
            if (Prerelease.Length > 0)
                _version += "-" + string.Join(".", Prerelease);
            return _version;
        }

        public string Inspect()
        {
            return string.Format("SemVer \"{0}\">", this);
        }

        public override string ToString()
        {
            return _version;
        }

        public object Clone()
        {
            return new SemVer(this);
        }

        public int Compare(SemVer other)
        {
            var main = CompareMain(other);
            return main != 0 ? main : ComparePre(other);
        }

        public int Compare(string other, bool loose = false)
        {
            if (string.IsNullOrWhiteSpace(other))
                throw new ArgumentNullException("other");

            SemVer otherVersion;
            try
            {
                otherVersion = new SemVer(other, loose);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException(exception.Message, "other", exception);
            }
            return Compare(otherVersion);
        }

        protected int CompareMain(SemVer other)
        {
            var major = CompareIdentifiers(Major, other.Major);
            if (major != 0) return major;
            
            var minor = CompareIdentifiers(Minor, other.Minor);
            if (minor != 0) return minor;

            return CompareIdentifiers(Patch, other.Patch);
        }

        protected int ComparePre(SemVer other)
        {
            // NOT having a prerelease is > having one
            if (this.Prerelease.Length > 0 && other.Prerelease.Length == 0)
                return -1;

            if (this.Prerelease.Length == 0 && other.Prerelease.Length > 0)
                return 1;

            if (this.Prerelease.Length == 0 && other.Prerelease.Length == 0)
                return 0;

            for (var i = 0; i < Math.Max(this.Prerelease.Length, other.Prerelease.Length); i++)
            {
                if (other.Prerelease.Length == i)
                    return 1;

                if (this.Prerelease.Length == i)
                    return -1;

                var compare = CompareIdentifiers(this.Prerelease[i], other.Prerelease[i]);
                if (compare != 0)
                    return compare;
            }
            return 0;
        }

        protected static int CompareIdentifiers(object a, object b)
        {
            var anum = a as int?;
            var bnum = b as int?;
            var astr = a.ToString();
            var bstr = b.ToString();

            if (anum != null && bnum != null)
                return anum == bnum ? 0 : (anum < bnum ? -1 : 1);

            if (anum == null && bnum == null)
            {
                var stringCompare = string.CompareOrdinal(astr, bstr);
                return stringCompare > 0 ? 1 : stringCompare < 0 ? -1 : 0;
            }

            if (anum != null)
                return -1;

            return 1;
        }

        public SemVer Increment(IncrementType type)
        {
            switch (type)
            {
                case IncrementType.Major:
                    this.Major++;
                    this.Minor = -1;
                    goto case IncrementType.Minor;
                case IncrementType.Minor:
                    this.Minor++;
                    this.Patch = -1;
                    goto case IncrementType.Patch;
                case IncrementType.Patch:
                    this.Patch++;
                    this.Prerelease = new object[0];
                    break;
                case IncrementType.Prerelease:
                    if (this.Prerelease.Length == 0)
                        this.Prerelease = new object[] {0};
                    else
                    {
                        var incrementedSomething = false;
                        for (var i = this.Prerelease.Length - 1; i >= 0; i--)
                        {
                            if (this.Prerelease[i] is int)
                            {
                                this.Prerelease[i] = (int) this.Prerelease[i] + 1;
                                incrementedSomething = true;
                                break;
                            }
                        }
                        if (!incrementedSomething)
                            this.Prerelease = new List<object>(this.Prerelease) {0}.ToArray();
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid increment: " + type, "type");
            }
            this.Format();
            return this;
        }

        public bool Equals(SemVer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Compare(other) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SemVer)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _loose.GetHashCode();
                hashCode = (hashCode * 397) ^ Major;
                hashCode = (hashCode * 397) ^ Minor;
                hashCode = (hashCode * 397) ^ Patch;
                hashCode = (hashCode * 397) ^ Prerelease.GetHashCode();
                hashCode = (hashCode * 397) ^ Build.GetHashCode();
                return hashCode;
            }
        }

        #region SemVer Operator Overloads

        public static bool operator ==(SemVer v1, SemVer v2)
        {
            if (ReferenceEquals(v1, null))
                return ReferenceEquals(v2, null);
            return v1.Compare(v2) == 0;
        }

        public static bool operator !=(SemVer v1, SemVer v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(SemVer v1, SemVer v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.Compare(v2) < 0;
        }

        public static bool operator >(SemVer v1, SemVer v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.Compare(v2) > 0;
        }

        public static bool operator <=(SemVer v1, SemVer v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(SemVer v1, SemVer v2)
        {
            return !(v1 < v2);
        }

        #endregion

        #region SemVer to String Operator Overloads

        public static bool operator ==(SemVer v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                return false;
            return v1.Compare(v2) == 0;
        }

        public static bool operator !=(SemVer v1, string v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(SemVer v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (string.IsNullOrWhiteSpace(v2))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.Compare(v2) < 0;
        }

        public static bool operator >(SemVer v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (string.IsNullOrWhiteSpace(v2))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.Compare(v2) > 0;
        }

        public static bool operator <=(SemVer v1, string v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(SemVer v1, string v2)
        {
            return !(v1 < v2);
        }

        #endregion

        #region String to SemVer Operator Overloads

        public static bool operator ==(string v1, SemVer v2)
        {
            if (ReferenceEquals(v2, null))
                return false;
            return v2.Compare(v1) == 0;
        }

        public static bool operator !=(string v1, SemVer v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(string v1, SemVer v2)
        {
            if (string.IsNullOrWhiteSpace(v1))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            return v2.Compare(v1) > 0;
        }

        public static bool operator >(string v1, SemVer v2)
        {
            if (string.IsNullOrWhiteSpace(v1))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            return v2.Compare(v1) < 0;
        }

        public static bool operator <=(string v1, SemVer v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(string v1, SemVer v2)
        {
            return !(v1 < v2);
        }

        #endregion
    }
}
