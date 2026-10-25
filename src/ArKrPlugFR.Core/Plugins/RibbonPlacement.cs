using System;

namespace ArKrPlugFR.Core.Plugins
{
    /// <summary>Placement and size of a tool's buttons on the ribbon.</summary>
    public sealed class RibbonPlacement
    {
        /// <summary>Creates a placement.</summary>
        /// <param name="tab">Name of the ribbon tab.</param>
        /// <param name="panel">Name of the panel inside the tab.</param>
        /// <param name="buttonSize">Button size: <c>large</c>, <c>medium</c> or <c>small</c>.</param>
        /// <param name="order">Sort order of the button inside the panel.</param>
        public RibbonPlacement(string tab, string panel, string buttonSize = "large", int order = 0)
        {
            if (string.IsNullOrWhiteSpace(tab))
            {
                throw new ArgumentException("The ribbon tab must not be empty.", nameof(tab));
            }

            if (string.IsNullOrWhiteSpace(panel))
            {
                throw new ArgumentException("The ribbon panel must not be empty.", nameof(panel));
            }

            Tab = tab;
            Panel = panel;
            ButtonSize = NormalizeSize(buttonSize);
            Order = order;
        }

        /// <summary>Gets the ribbon tab.</summary>
        public string Tab { get; }

        /// <summary>Gets the panel inside the tab.</summary>
        public string Panel { get; }

        /// <summary>Gets the button size, one of <c>large</c>, <c>medium</c> or <c>small</c>.</summary>
        public string ButtonSize { get; }

        /// <summary>Gets the sort order of the button inside the panel.</summary>
        public int Order { get; }

        private static string NormalizeSize(string size)
        {
            if (string.IsNullOrWhiteSpace(size))
            {
                return "large";
            }

            switch (size.Trim().ToLowerInvariant())
            {
                case "large":
                    return "large";
                case "medium":
                    return "medium";
                case "small":
                    return "small";
                default:
                    throw new ArgumentException(
                        "The button size must be 'large', 'medium' or 'small', but was '" + size + "'.",
                        nameof(size));
            }
        }
    }
}
