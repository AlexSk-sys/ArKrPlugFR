namespace ArKrPlugFR.Plugins.ReassignRebar
{
    /// <summary>
    /// The options of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class ReassignSettings
    {
        /// <summary>Gets or sets the parameter that holds a structure's mark.</summary>
        public string MarkParameter { get; set; } = "Марка";

        /// <summary>Gets or sets the parameter written into a bar that arrived from IFC.</summary>
        public string IfcMarkParameter { get; set; } = "ADSK_Метка основы";
    }
}
