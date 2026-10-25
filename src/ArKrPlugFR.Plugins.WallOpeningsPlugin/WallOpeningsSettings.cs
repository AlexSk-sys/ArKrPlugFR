namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>
    /// The switches of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class WallOpeningsSettings
    {
        /// <summary>Gets or sets a value indicating whether doors are processed.</summary>
        public bool Doors { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether windows are processed.</summary>
        public bool Windows { get; set; } = true;
    }
}
