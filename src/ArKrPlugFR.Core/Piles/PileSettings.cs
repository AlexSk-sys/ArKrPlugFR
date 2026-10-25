namespace ArKrPlugFR.Core.Piles
{
    /// <summary>
    /// The remembered state of the tool: the start number, the direction of the numbering
    /// and the names of the four parameters the actions write.
    /// </summary>
    public sealed class PileSettings
    {
        /// <summary>Gets or sets the number the first pile receives.</summary>
        public int StartNumber { get; set; } = 1;

        /// <summary>Gets or sets a value indicating whether the numbering runs from the largest Y down.</summary>
        public bool TopDown { get; set; } = true;

        /// <summary>Gets or sets the name of the parameter that receives the number.</summary>
        public string NumberParameter { get; set; } = "O_Позиция";

        /// <summary>Gets or sets the name of the parameter that receives the cut height.</summary>
        public string CutHeightParameter { get; set; } = "Высота срубаемой части";

        /// <summary>Gets or sets the name of the parameter that receives the elevation of the raft bottom.</summary>
        public string SlabBottomParameter { get; set; } = "Отметка низа плиты";

        /// <summary>Gets or sets the name of the parameter that receives the elevation of the pile bottom.</summary>
        public string PileBottomParameter { get; set; } = "Отметка низа сваи";
    }
}
