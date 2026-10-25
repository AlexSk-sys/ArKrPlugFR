namespace ArKrPlugFR.Core.Parameters
{
    /// <summary>
    /// What receives the value of a room: the structures around it and, when asked for,
    /// the room itself.
    /// </summary>
    public sealed class RoomSpreadOptions
    {
        /// <summary>Gets or sets a value indicating whether the boundary walls receive the value.</summary>
        public bool Walls { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether the floor under the room receives the value.</summary>
        public bool Floors { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether the ceiling above the room receives the value.</summary>
        public bool Ceilings { get; set; } = true;

        /// <summary>Gets or sets a value indicating whether the room itself receives the value.</summary>
        public bool Rooms { get; set; }

        /// <summary>Gets a value indicating whether any receiver at all was chosen.</summary>
        public bool AnySelected => Walls || Floors || Ceilings || Rooms;
    }
}
