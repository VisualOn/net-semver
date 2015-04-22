namespace Vtex.SemanticVersioning
{
    /// <summary>
    /// Segments of a semantic version
    /// </summary>
    public enum VersionSegment
    {
        /// <summary>
        /// The pre-release segment of a version (<c>x.y.z-*</c>).
        /// </summary>
        Prerelease,

        /// <summary>
        /// The patch segment of a version (<c>x.y.*</c>).
        /// </summary>
        Patch,

        /// <summary>
        /// The minor segment of a version (<c>x.*.z</c>).
        /// </summary>
        Minor,

        /// <summary>
        /// The major segment of a version (<c>*.y.z</c>).
        /// </summary>
        Major
    }
}