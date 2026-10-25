namespace ArKrPlugFR.Core.Concrete
{
    /// <summary>
    /// An axis of the project's own frame.
    /// <para>
    /// An opening in a column is based on a plane perpendicular to one of the three
    /// axes; the axis names the direction the opening goes through, not the plane.
    /// </para>
    /// </summary>
    public enum CutAxis
    {
        /// <summary>The X axis.</summary>
        X = 0,

        /// <summary>The Y axis.</summary>
        Y = 1,

        /// <summary>The Z axis.</summary>
        Z = 2
    }
}
