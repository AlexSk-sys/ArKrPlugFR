using System.Globalization;
using System.Text;
using ArKrPlugFR.Core.Sections;

namespace ArKrPlugFR.Plugins.SectionCollector
{
    /// <summary>
    /// The strings of the tool resolved for one language.
    /// </summary>
    public sealed class SectionText
    {
        private readonly string _culture;

        /// <summary>Creates the text for a culture; an unknown culture falls back to English.</summary>
        /// <param name="culture">Two-letter culture code.</param>
        public SectionText(string? culture)
        {
            _culture = culture ?? "en";
        }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => SectionStrings.WindowTitle.Resolve(_culture);

        /// <summary>Gets the line under the title; it stays English in every language.</summary>
        public string Subtitle => SectionStrings.Subtitle.Resolve(_culture);

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => SectionStrings.ReportCaption.Resolve(_culture);

        /// <summary>Gets the message shown when the project has no 3D view.</summary>
        public string NoView3D => SectionStrings.NoView3D.Resolve(_culture);

        /// <summary>Gets the message shown when nothing was collected.</summary>
        public string NoStructures => SectionStrings.NoStructures.Resolve(_culture);

        /// <summary>Gets the name of the columns category.</summary>
        public string CategoryColumns => SectionStrings.CategoryColumns.Resolve(_culture);

        /// <summary>Gets the name of the walls category.</summary>
        public string CategoryWalls => SectionStrings.CategoryWalls.Resolve(_culture);

        /// <summary>Gets the name of the floors category.</summary>
        public string CategoryFloors => SectionStrings.CategoryFloors.Resolve(_culture);

        /// <summary>Gets the name of the beams category.</summary>
        public string CategoryBeams => SectionStrings.CategoryBeams.Resolve(_culture);

        /// <summary>Gets the stand-in for an element without a level.</summary>
        public string NoLevel => SectionStrings.NoLevel.Resolve(_culture);

        /// <summary>Gets the stand-in for a type without a section.</summary>
        public string NoSection => SectionStrings.NoSection.Resolve(_culture);

        /// <summary>Gets the caption of the select button.</summary>
        public string SelectAll => SectionStrings.SelectAll.Resolve(_culture);

        /// <summary>Gets the caption of the copy button.</summary>
        public string Copy => SectionStrings.Copy.Resolve(_culture);

        /// <summary>Gets the message shown after a copy.</summary>
        public string Copied => SectionStrings.Copied.Resolve(_culture);

        /// <summary>Gets the caption of the close button.</summary>
        public string Close => SectionStrings.Close.Resolve(_culture);

        /// <summary>Gets the message shown when selection failed.</summary>
        /// <param name="reason">What the platform reported.</param>
        public string SelectFailed(string reason) => SectionStrings.SelectFailed.Resolve(_culture, reason);

        /// <summary>Gets the summary line of the report.</summary>
        /// <param name="elements">Elements that were collected.</param>
        /// <param name="types">Types they carry.</param>
        public string Summary(int elements, int types) => SectionStrings.Summary.Resolve(_culture, elements, types);

        /// <summary>Builds the report of a collection.</summary>
        /// <param name="groups">The report groups.</param>
        /// <param name="elements">Elements that were collected.</param>
        /// <param name="types">Types they carry.</param>
        /// <returns>The report text.</returns>
        public string BuildReport(System.Collections.Generic.IReadOnlyList<SectionLevelGroup> groups, int elements, int types)
        {
            var builder = new StringBuilder();

            foreach (SectionLevelGroup level in groups)
            {
                builder.Append("=== ");
                builder.AppendLine(level.Level);

                foreach (SectionCategoryGroup category in level.Categories)
                {
                    builder.AppendLine(category.Category + ":");

                    foreach (SectionItem item in category.Items)
                    {
                        builder.Append("  ");
                        builder.Append(item.TypeName);
                        builder.Append(" — ");
                        builder.Append(item.Section ?? NoSection);
                        builder.Append(": ");
                        builder.AppendLine(item.Count.ToString(CultureInfo.InvariantCulture));
                    }

                    builder.AppendLine();
                }
            }

            builder.Append(Summary(elements, types));

            return builder.ToString();
        }
    }
}
