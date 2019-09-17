using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Semverify.SemverModel
{
    internal class Semver : IComparable<Semver> 
    {
        private static readonly Regex SemanticVersionRegex = new Regex(@"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");

        public int? Major { get; set; }
        public int? Minor { get; set; }
        public int? Patch { get; set; }
        public string Prerelease { get; set; }
        public string BuildMetadata { get; set; }

        public bool IsPrerelease { get => !string.IsNullOrWhiteSpace(Prerelease); }

        public override string ToString()
        {
            if (!Major.HasValue)
            {
                return string.Empty;
            }

            var version = $"{Major}.{Minor ?? 0}.{Patch ?? 0}";

            if (!string.IsNullOrWhiteSpace(Prerelease))
            {
                version += $"-{Prerelease}";
            }
            if (!string.IsNullOrWhiteSpace(BuildMetadata))
            {
                version += $"+{BuildMetadata}";
            }
            return version;
        }

        public string ToShortVerion()
        {
            if (!Major.HasValue)
            {
                return string.Empty;
            }
            return $"{Major}.{Minor ?? 0}.{Patch ?? 0}";
        }

        public static bool TryParse(Version version, out Semver semver)
        {
            if (version == null)
            {
                semver = null;
                return false;
            }

            semver = new Semver
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Build,
                Prerelease = version.Revision == 0 ? string.Empty : version.Revision.ToString()
            };
            return true;
        }

        public static bool TryParse(string version, out Semver semver)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                semver = null;
                return false;
            }

            var match = SemanticVersionRegex.Match(version);
            if (match.Success)
            {
                semver = new Semver
                {
                    Major = Convert.ToInt16(match.Groups["major"].Value),
                    Minor = Convert.ToInt16(match.Groups["minor"].Value),
                    Patch = Convert.ToInt16(match.Groups["patch"].Value),
                    Prerelease = match.Groups["prerelease"]?.Value,
                    BuildMetadata = match.Groups["buildmetadata"]?.Value,
                };

                return true;
            }

            semver = null;
            return false;
        }

        public int CompareTo([AllowNull] Semver other)
        {
            if (other == null)
            {
                return 1;
            }

            if (Major.HasValue || other.Major.HasValue)
            {
                if (Major != other.Major)
                {
                    if (other.Major == null || Major > other.Major)
                        return 1;
                    else
                        return -1;
                }
            }

            if (Minor.HasValue || other.Minor.HasValue)
            {
                if (Minor != other.Minor)
                {
                    if (other.Major == null || Minor > other.Minor)
                        return 1;
                    else
                        return -1;
                }
            }

            if (Patch.HasValue || other.Patch.HasValue)
            {
                if (Patch != other.Patch)
                {
                    if (Patch > other.Patch)
                        return 1;
                    else
                        return -1;
                }
            }

            if (Prerelease != null && other.Prerelease != null)
            {
                return Prerelease.CompareTo(other.Prerelease);
            }

            return 0;
        }
    }
}
