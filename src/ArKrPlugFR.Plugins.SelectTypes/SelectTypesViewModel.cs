using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ArKrPlugFR.Core.Diagnostics;
using ArKrPlugFR.Core.Types;
using ArKrPlugFR.Revit;
using ArKrPlugFR.Ui.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>The scopes the type tree offers.</summary>
    internal enum TypeScope
    {
        /// <summary>Every family of the project.</summary>
        All,

        /// <summary>The types that have instances in the project.</summary>
        Placed,

        /// <summary>The types that have instances visible in the active view.</summary>
        Visible
    }

    /// <summary>
    /// The state of the tool window: the scope, the search, the tree and the selection.
    /// <para>
    /// The types of every scope are read once when the window opens. The tree is rebuilt
    /// from the facts whenever the scope or the query changes, and the ticks survive a
    /// rebuild, so a search never loses what was already ticked. The tool only reads the
    /// project.
    /// </para>
    /// </summary>
    public sealed class SelectTypesViewModel : ObservableObject
    {
        /// <summary>How many report lines are listed one by one; the rest are only counted.</summary>
        private const int ReportLimit = 10;

        private readonly UIDocument _uiDocument;
        private readonly Document _document;
        private readonly Log _log;
        private readonly IReadOnlyList<TypeFact> _all;
        private readonly IReadOnlyList<TypeFact> _placed;
        private readonly IReadOnlyList<TypeFact> _visible;
        private readonly List<TypeScope> _scopes = new List<TypeScope>();
        private readonly View? _activeView;
        private readonly HashSet<long> _ticked = new HashSet<long>();
        private int _scopeIndex;
        private string _searchText = string.Empty;
        private string _report = string.Empty;

        /// <summary>Creates the view model of the tool.</summary>
        /// <param name="uiDocument">The document the user is working in.</param>
        /// <param name="document">Project the types come from.</param>
        /// <param name="text">Text resolved for the current language.</param>
        /// <param name="log">Logger of the add-in.</param>
        public SelectTypesViewModel(UIDocument uiDocument, Document document, SelectText text, Log log)
        {
            _uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
            _document = document ?? throw new ArgumentNullException(nameof(document));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            _log = log ?? throw new ArgumentNullException(nameof(log));

            _all = TypeCatalogReader.ReadAll(document);
            _placed = TypeCatalogReader.ReadPlaced(document);

            _activeView = document.ActiveView;
            bool viewScope = _activeView != null && !(_activeView is ViewSheet);
            _visible = viewScope
                ? TypeCatalogReader.ReadVisible(document, _activeView!)
                : Array.Empty<TypeFact>();

            _scopes.Add(TypeScope.All);
            ScopeOptions.Add(Text.ScopeAll + " (" + _all.Count + ")");

            _scopes.Add(TypeScope.Placed);
            ScopeOptions.Add(Text.ScopePlaced + " (" + _placed.Count + ")");

            if (viewScope)
            {
                _scopes.Add(TypeScope.Visible);
                ScopeOptions.Add(Text.ScopeVisible + " (" + _visible.Count + ")");
            }

            Rebuild();
        }

        /// <summary>Gets the text resolved for the current language.</summary>
        public SelectText Text { get; }

        /// <summary>Gets the categories of the tree.</summary>
        public ObservableCollection<CategoryNode> Categories { get; } = new ObservableCollection<CategoryNode>();

        /// <summary>Gets the scopes the window offers.</summary>
        public ObservableCollection<string> ScopeOptions { get; } = new ObservableCollection<string>();

        /// <summary>Gets the window title.</summary>
        public string WindowTitle => Text.WindowTitle;

        /// <summary>Gets the line under the title.</summary>
        public string Subtitle => Text.Subtitle;

        /// <summary>Gets the caption of the scope list.</summary>
        public string ScopeCaption => Text.ScopeCaption;

        /// <summary>Gets the hint about the view scope not being available on a sheet.</summary>
        public string ScopeHint => _scopes.Count < 3 ? Text.ScopeUnavailableOnSheets : string.Empty;

        /// <summary>Gets a value indicating whether the scope hint has anything to say.</summary>
        public bool HasScopeHint => ScopeHint.Length > 0;

        /// <summary>Gets the caption of the search field.</summary>
        public string SearchCaption => Text.SearchCaption;

        /// <summary>Gets the hint under the search field.</summary>
        public string SearchHint => Text.SearchHint;

        /// <summary>Gets the caption of the type tree.</summary>
        public string TreeCaption => Text.TreeCaption;

        /// <summary>Gets the caption of the button that ticks every type.</summary>
        public string TickAllText => Text.TickAll;

        /// <summary>Gets the caption of the button that clears the ticks.</summary>
        public string UntickAllText => Text.UntickAll;

        /// <summary>Gets the caption of the run button.</summary>
        public string ApplyText => Text.Apply;

        /// <summary>Gets the hint under the buttons.</summary>
        public string Hint => Text.Hint;

        /// <summary>Gets the caption of the report area.</summary>
        public string ReportCaption => Text.ReportCaption;

        /// <summary>Gets or sets the chosen scope, as an index into the offered scopes.</summary>
        public int ScopeIndex
        {
            get => _scopeIndex;

            set
            {
                if (SetProperty(ref _scopeIndex, value))
                {
                    Rebuild();
                }
            }
        }

        /// <summary>Gets or sets the search query.</summary>
        public string SearchText
        {
            get => _searchText;

            set
            {
                if (SetProperty(ref _searchText, value ?? string.Empty))
                {
                    Rebuild();
                }
            }
        }

        /// <summary>Gets the line that counts the types the tree shows.</summary>
        public string TypesLine => Text.TypesLine(ShownTypes());

        /// <summary>Gets the result of the last run, or an empty string.</summary>
        public string Report => _report;

        /// <summary>Gets a value indicating whether there is anything to show in the report area.</summary>
        public bool HasReport => !string.IsNullOrWhiteSpace(_report);

        /// <summary>Ticks or unticks every type of the tree.</summary>
        /// <param name="selected">Whether everything is ticked.</param>
        public void TickAll(bool selected)
        {
            foreach (CategoryNode category in Categories)
            {
                category.IsChecked = selected;
            }

            RaisePropertyChanged(nameof(TypesLine));
        }

        /// <summary>Selects the instances of the ticked types and returns the report.</summary>
        /// <returns>The message to show the user.</returns>
        public string Apply()
        {
            var ticked = new List<long>();
            var builder = new StringBuilder();

            foreach (CategoryNode category in Categories)
            {
                CollectTicked(category, ticked);
            }

            if (ticked.Count == 0)
            {
                string empty = CurrentFacts().Count == 0 ? Text.NoTypesInScope : Text.NoTypes;

                SetReport(empty);

                return empty;
            }

            TypeSelectionOutcome outcome = TypeSelectionEngine.Run(_uiDocument, _document, ScopeView(), ticked, _log);

            builder.AppendLine(Text.Result(outcome.TypesTicked, outcome.Selected, outcome.WithoutInstances));

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

            string message = builder.ToString().TrimEnd();
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

        private void Rebuild()
        {
            HarvestTicks();
            Categories.Clear();

            IReadOnlyList<CategoryGroup> groups = TypeCatalog.Build(CurrentFacts(), _searchText, Text.NoCategory);

            foreach (CategoryGroup category in groups)
            {
                var categoryNode = new CategoryNode(category.Name, category.TypeCount);

                foreach (FamilyGroup family in category.Families)
                {
                    var familyNode = new FamilyNode(family.Name, family.TypeCount);

                    foreach (TypeFact fact in family.Types)
                    {
                        var typeNode = new TypeNode(fact);

                        if (_ticked.Contains(fact.Id))
                        {
                            typeNode.IsChecked = true;
                        }

                        familyNode.Children.Add(typeNode);
                    }

                    familyNode.RefreshFromChildren();
                    categoryNode.Children.Add(familyNode);
                }

                categoryNode.RefreshFromChildren();
                Categories.Add(categoryNode);
            }

            RaisePropertyChanged(nameof(TypesLine));
        }

        private void HarvestTicks()
        {
            var ticked = new List<long>();

            foreach (CategoryNode category in Categories)
            {
                CollectTicked(category, ticked);
            }

            _ticked.Clear();

            foreach (long id in ticked)
            {
                _ticked.Add(id);
            }
        }

        private static void CollectTicked(FilterNode node, List<long> ticked)
        {
            if (node is TypeNode type && type.IsChecked == true)
            {
                ticked.Add(type.TypeId);
            }

            foreach (FilterNode child in node.Children)
            {
                CollectTicked(child, ticked);
            }
        }

        private IReadOnlyList<TypeFact> CurrentFacts()
        {
            switch (_scopes[_scopeIndex])
            {
                case TypeScope.Placed:
                    return _placed;

                case TypeScope.Visible:
                    return _visible;

                default:
                    return _all;
            }
        }

        private View? ScopeView()
        {
            return _scopes[_scopeIndex] == TypeScope.Visible ? _activeView : null;
        }

        private int ShownTypes()
        {
            int total = 0;

            foreach (CategoryNode category in Categories)
            {
                foreach (FilterNode family in category.Children)
                {
                    total += family.Children.Count;
                }
            }

            return total;
        }
    }
}
