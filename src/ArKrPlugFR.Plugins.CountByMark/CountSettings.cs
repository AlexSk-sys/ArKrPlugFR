namespace ArKrPlugFR.Plugins.CountByMark
{
    /// <summary>
    /// The options of the tool, remembered between sessions as one JSON file in the
    /// user's settings folder.
    /// </summary>
    public sealed class CountSettings
    {
        /// <summary>Gets or sets the chosen kind of structures; one of the five settings names.</summary>
        public string Kind { get; set; } = "all";

        /// <summary>Gets or sets the parameter that holds a structure's mark.</summary>
        public string StructureMarkParameter { get; set; } = "Марка";

        /// <summary>Gets or sets the parameter that holds a bar's structure mark.</summary>
        public string HostMarkParameter { get; set; } = "Метка основы";

        /// <summary>Gets or sets the parameter the number is written into.</summary>
        public string CountParameter { get; set; } = "Кол_Конструкций";
    }
}
