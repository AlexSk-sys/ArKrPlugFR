namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>
    /// The options of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class CutHolesSettings
    {
        /// <summary>Gets or sets the clearance added on every side of a crossing, in millimetres.</summary>
        public int ClearanceMillimeters { get; set; } = 50;
    }
}
