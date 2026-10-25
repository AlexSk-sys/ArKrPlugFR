namespace ArKrPlugFR.Plugins.AlignToGrids
{
    /// <summary>
    /// The options of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class AlignSettings
    {
        /// <summary>Gets or sets how close a grid has to be, in millimetres.</summary>
        public int CaptureMillimeters { get; set; } = 5;
    }
}
