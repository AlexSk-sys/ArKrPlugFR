using System;
using System.Collections.Generic;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Settings;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.CountByMark
{
    /// <summary>
    /// The state of the options window: the kind of structures to count and the run it
    /// starts.
    /// <para>
    /// The kind is read from the user's settings folder when the window opens and written
    /// back whenever it changes, so the next run starts where this one left off. The
    /// parameter names live in the same settings file.
    /// </para>
    /// </summary>
    public sealed class CountViewModel : ObservableObject
    {
        /// <summary>Name of the settings file of this tool.</summary>
        internal const string SettingsName = "count-by-mark";

        /// <summary>How many failure lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        /// <summary>Settings names of the five choices, in the order of the kind list.</summary>
        private static readonly string[] KindKeys = { "columns", "walls", "floors", "beams", "all" };

        private readonly Document _document;
        private readonly Log _log;
        private readonly CountSettings _settings;
        private int _kindIndex;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project that is counted.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public CountViewModel(Document document, CountText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _settings = ToolSettings.Load(AppPaths.SettingsDirectory, SettingsName, new CountSettings());
            _kindIndex = IndexOfKind(_settings.Kind);
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public CountText Text { get; }

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the kind list.</summary>
        public string KindLabel => Text.KindLabel;

        /// <summary>Gets the hint about the parameter names.</summary>
        public string OptionsHint => Text.OptionsHint;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the five choices of the kind list.</summary>
        public string[] Kinds => Text.Kinds;

        /// <summary>Gets or sets the chosen kind.</summary>
        public int KindIndex
        {
            get => _kindIndex;

            set
            {
                if (SetProperty(ref _kindIndex, value) && value >= 0 && value < KindKeys.Length)
                {
                    _settings.Kind = KindKeys[value];

                    ToolSettings.Save(AppPaths.SettingsDirectory, SettingsName, _settings);
                }
            }
        }

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Counts the structures and returns the report of the run.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            CountOutcome outcome = CountEngine.Run(_document, Categories(), _settings, Text, _log);

            string message = BuildReport(outcome);
            SetReport(message);

            return message;
        }

        /// <summary>Shows the given text in the report area of the window.</summary>
        /// <param name="report">Text to show.</param>
        public void SetReport(string report)
        {
            _report = report ?? string.Empty;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        private IReadOnlyCollection<BuiltInCategory> Categories()
        {
            switch (KindKeys[_kindIndex])
            {
                case "columns":
                    return new[] { BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_Columns };

                case "walls":
                    return new[] { BuiltInCategory.OST_Walls };

                case "floors":
                    return new[] { BuiltInCategory.OST_Floors };

                case "beams":
                    return new[] { BuiltInCategory.OST_StructuralFraming };

                default:
                    return new[]
                    {
                        BuiltInCategory.OST_StructuralColumns,
                        BuiltInCategory.OST_Columns,
                        BuiltInCategory.OST_Walls,
                        BuiltInCategory.OST_Floors,
                        BuiltInCategory.OST_StructuralFraming
                    };
            }
        }

        private static int IndexOfKind(string? kind)
        {
            for (int index = 0; index < KindKeys.Length; index++)
            {
                if (string.Equals(KindKeys[index], kind, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return KindKeys.Length - 1;
        }

        private string BuildReport(CountOutcome outcome)
        {
            var builder = new StringBuilder();

            if (outcome.Stopped != null)
            {
                builder.AppendLine(outcome.Stopped);
            }

            if (outcome.Bars > 0)
            {
                builder.AppendLine(Text.Result(
                    outcome.Structures,
                    outcome.Marks,
                    outcome.Bars,
                    outcome.Updated,
                    outcome.NoHostMark,
                    outcome.NoCount,
                    outcome.NoParameter,
                    outcome.Failed));
            }

            int shown = 0;

            foreach (string failure in outcome.Failures)
            {
                if (shown >= ReportLimit)
                {
                    break;
                }

                builder.Append("  ");
                builder.AppendLine(failure);
                shown++;
            }

            if (outcome.Failures.Count > shown)
            {
                builder.AppendLine(Text.MoreLine(outcome.Failures.Count - shown));
            }

            return builder.ToString().TrimEnd();
        }
    }
}
