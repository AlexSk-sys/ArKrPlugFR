using System;
using System.Collections.Generic;
using System.Linq;

namespace ArKrPlugFR.Core.SheetTransfer
{
    /// <summary>
    /// What the run will do: one planned copy per ticked sheet, each with a number
    /// that is free in the target project and a name that marks it as a copy.
    /// <para>
    /// The plan is decided before the transaction starts, so the run never has to
    /// ask the user anything halfway through.
    /// </para>
    /// </summary>
    public sealed class TransferPlan
    {
        private TransferPlan(IReadOnlyList<SheetTransferItem> items)
        {
            Items = items;
        }

        /// <summary>Gets the planned copies, ordered by sheet number with a natural comparison.</summary>
        public IReadOnlyList<SheetTransferItem> Items { get; }

        /// <summary>Gets the number of planned copies.</summary>
        public int PlannedCount => Items.Count;

        /// <summary>
        /// Builds a plan from the ticked sheets and the numbers the target project
        /// already uses.
        /// </summary>
        /// <param name="selected">Sheets the user ticked.</param>
        /// <param name="takenNumbers">Sheet numbers that already exist in the target project.</param>
        /// <param name="nameMarker">Marker appended to the name of every copy.</param>
        /// <param name="copyCounts">
        /// How many copies each sheet gets, by sheet id; a sheet missing from the map
        /// gets one copy. Counts below one read as one.
        /// </param>
        /// <returns>The planned copies; sheets that are not transferable are left out.</returns>
        public static TransferPlan Build(
            IEnumerable<SheetCandidate> selected,
            IEnumerable<string> takenNumbers,
            string nameMarker,
            IReadOnlyDictionary<long, int>? copyCounts = null)
        {
            if (selected is null)
            {
                throw new ArgumentNullException(nameof(selected));
            }

            if (takenNumbers is null)
            {
                throw new ArgumentNullException(nameof(takenNumbers));
            }

            if (nameMarker is null)
            {
                throw new ArgumentNullException(nameof(nameMarker));
            }

            var occupied = new HashSet<string>(takenNumbers, StringComparer.OrdinalIgnoreCase);
            var usedSheets = new HashSet<long>();
            var items = new List<SheetTransferItem>();

            // The copies are numbered in the natural order of the source numbers, so
            // the generated numbers read the way the sheet list reads: A-01 is planned
            // before A-10 even when the sheets arrive the other way round.
            var ordered = selected
                .Where(sheet => SheetTransferRules.IsTransferable(sheet))
                .OrderBy(
                    sheet => sheet,
                    Comparer<SheetCandidate>.Create(
                        (left, right) => SheetTransferRules.CompareSheetNumbers(left.Number, right.Number)));

            foreach (SheetCandidate sheet in ordered)
            {
                if (!usedSheets.Add(sheet.Id))
                {
                    continue;
                }

                int copies = 1;

                if (copyCounts != null && copyCounts.TryGetValue(sheet.Id, out int wanted) && wanted > 1)
                {
                    copies = wanted;
                }

                string marked = SheetTransferRules.MarkSheetName(sheet.Name, nameMarker);

                for (int copyNumber = 1; copyNumber <= copies; copyNumber++)
                {
                    // Every copy takes the next free number on its own: the source
                    // number when it is free, otherwise the lowest free suffix. Several
                    // copies of one sheet also carry their copy number in the name, so
                    // they can be told apart in the sheet list.
                    string number = SheetTransferRules.MakeUniqueNumber(sheet.Number, occupied);
                    occupied.Add(number);

                    items.Add(new SheetTransferItem(
                        sheet.Id,
                        sheet.Number,
                        sheet.Name,
                        number,
                        copies == 1 ? marked : Sheets.SheetCopyNames.MarkedSheetName(marked, copyNumber),
                        CountTransferableViews(sheet)));
                }
            }

            return new TransferPlan(items);
        }

        private static int CountTransferableViews(SheetCandidate sheet)
        {
            int count = 0;

            foreach (ViewCandidate view in sheet.PlacedViews)
            {
                if (SheetTransferRules.IsTransferableView(view))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
