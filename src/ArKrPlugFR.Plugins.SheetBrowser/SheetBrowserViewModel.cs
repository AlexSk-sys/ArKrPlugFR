using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArKrPlugFR.Core.Browser;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.SheetBrowser
{
    /// <summary>
    /// The state of the tool window: the tree of the active browser scheme and the counts
    /// the window reports.
    /// <para>
    /// The scheme is read once when the window opens. Every sheet the browser shows is
    /// asked for its folders; a sheet the scheme hides is counted, and a sheet whose
    /// folders cannot be read is counted and shown under its own group, so a broken scheme
    /// never hides a sheet. The tool only reads the project.
    /// </para>
    /// </summary>
    public sealed class SheetBrowserViewModel : ObservableObject
    {
        private readonly Document _document;
        private readonly Log _log;
        private int _sheetCount;
        private int _groupCount;
        private int _excluded;
        private int _unreadable;
        private bool _noScheme;
        private BrowserNode? _selected;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="document">Project the sheets come from.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public SheetBrowserViewModel(Document document, SheetBrowserText text, Log log)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            Read();
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SheetBrowserText Text { get; }

        /// <summary>Gets the rows at the root of the tree.</summary>
        public ObservableCollection<BrowserNode> Nodes { get; } = new ObservableCollection<BrowserNode>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the button that copies the selected path.</summary>
        public string CopyPathText => Text.CopyPath;

        /// <summary>Gets the hint under the tree.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets the line that counts the sheets and groups.</summary>
        public string Summary => _sheetCount == 0
            ? Text.NoSheets
            : Text.Summary(_sheetCount, _groupCount, _excluded, _unreadable);

        /// <summary>Gets the line about the project having no organization scheme.</summary>
        public string SchemeNote => _noScheme ? Text.NoScheme : string.Empty;

        /// <summary>Gets a value indicating whether the scheme note has anything to say.</summary>
        public bool HasSchemeNote => _noScheme;

        /// <summary>Gets or sets the selected row.</summary>
        public BrowserNode? Selected
        {
            get => _selected;

            set => SetProperty(ref _selected, value);
        }

        /// <summary>Gets the result of the last copy, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Shows the given text in the report area of the window.</summary>
        /// <param name="report">Text to show.</param>
        public void SetReport(string report)
        {
            _report = report ?? string.Empty;
            RaisePropertyChanged(nameof(Report));
            RaisePropertyChanged(nameof(HasReport));
        }

        private void Read()
        {
            BrowserOrganization? organization = BrowserSupport.CurrentForSheets(_document);
            _noScheme = organization == null;

            var sheets = new List<ViewSheet>();

            foreach (Element element in new FilteredElementCollector(_document).OfClass(typeof(ViewSheet)))
            {
                if (element is ViewSheet sheet)
                {
                    sheets.Add(sheet);
                }
            }

            _sheetCount = sheets.Count;

            bool descending = false;

            if (organization != null)
            {
                try
                {
                    descending = BrowserSupport.IsDescending(organization);
                }
                catch (Exception exception)
                {
                    _log.Warn(
                        "SheetBrowser: the sorting order of the scheme could not be read: " + exception.Message,
                        nameof(SheetBrowserViewModel));
                }
            }

            var facts = new List<SheetFact>();

            foreach (ViewSheet sheet in sheets)
            {
                string number = sheet.SheetNumber ?? string.Empty;
                string name = sheet.Name ?? string.Empty;

                if (organization == null)
                {
                    facts.Add(new SheetFact(Array.Empty<string>(), number, name, number));

                    continue;
                }

                try
                {
                    if (!BrowserSupport.IsShown(organization, sheet.Id))
                    {
                        _excluded++;

                        continue;
                    }

                    IReadOnlyList<string> folders = BrowserSupport.ReadFolders(organization, sheet.Id);
                    string sortKey = BrowserSupport.ReadSortingValue(organization, sheet);

                    facts.Add(new SheetFact(folders, number, name, sortKey));
                }
                catch (Exception exception)
                {
                    _unreadable++;
                    _log.Warn(
                        "SheetBrowser: the folders of " + ElementDescription.Describe(sheet) +
                        " could not be read: " + exception.Message,
                        nameof(SheetBrowserViewModel));

                    facts.Add(new SheetFact(new[] { Text.GroupingError }, number, name, number));
                }
            }

            SheetTree tree = SheetTreeBuilder.Build(facts, descending);
            _groupCount = tree.GroupCount;

            foreach (SheetFact sheet in tree.Sheets)
            {
                Nodes.Add(new SheetNode(sheet.Label, SheetPath.Join(sheet.Folders, sheet.Label)));
            }

            AddGroups(tree.Groups, string.Empty, Nodes);
        }

        private static void AddGroups(
            IReadOnlyList<SheetGroup> groups,
            string parentPath,
            ObservableCollection<BrowserNode> target)
        {
            foreach (SheetGroup group in groups)
            {
                string path = parentPath.Length == 0
                    ? group.Name
                    : parentPath + SheetPath.Separator + group.Name;

                var node = new GroupNode(group.Name + " (" + group.SheetCount + ")", path);

                AddGroups(group.Groups, path, node.Children);

                foreach (SheetFact sheet in group.Sheets)
                {
                    node.Children.Add(new SheetNode(sheet.Label, SheetPath.Join(sheet.Folders, sheet.Label)));
                }

                target.Add(node);
            }
        }
    }
}
