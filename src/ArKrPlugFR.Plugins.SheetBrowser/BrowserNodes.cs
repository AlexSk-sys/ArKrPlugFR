using System;
using System.Collections.ObjectModel;
using ArKrPlugFR.Ui.ViewModels;

namespace ArKrPlugFR.Plugins.SheetBrowser
{
    /// <summary>
    /// A row of the browser tree: a group or a sheet, with the path the row copies.
    /// </summary>
    public abstract class BrowserNode : ObservableObject
    {
        private bool _isExpanded = true;

        /// <summary>Creates a row.</summary>
        /// <param name="title">Text shown in the tree.</param>
        /// <param name="path">Full path the row copies.</param>
        protected BrowserNode(string title, string path)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>Gets the text shown in the tree.</summary>
        public string Title { get; }

        /// <summary>Gets the full path of the row.</summary>
        public string Path { get; }

        /// <summary>Gets the rows under this one.</summary>
        public ObservableCollection<BrowserNode> Children { get; } = new ObservableCollection<BrowserNode>();

        /// <summary>Gets or sets a value indicating whether the row is open.</summary>
        public bool IsExpanded
        {
            get => _isExpanded;

            set => SetProperty(ref _isExpanded, value);
        }
    }

    /// <summary>A folder of the browser scheme.</summary>
    public sealed class GroupNode : BrowserNode
    {
        /// <summary>Creates a group row.</summary>
        /// <param name="title">Text shown in the tree.</param>
        /// <param name="path">Full path of the group.</param>
        public GroupNode(string title, string path)
            : base(title, path)
        {
        }
    }

    /// <summary>A sheet of the browser tree.</summary>
    public sealed class SheetNode : BrowserNode
    {
        /// <summary>Creates a sheet row.</summary>
        /// <param name="title">Label of the sheet: its number and name.</param>
        /// <param name="path">Full path of the sheet.</param>
        public SheetNode(string title, string path)
            : base(title, path)
        {
        }
    }
}
