using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArKrPlugFR.Plugins.SheetBrowser
{
    /// <summary>
    /// The window of the tool: the tree of the browser scheme and the copy of a path.
    /// <para>
    /// The window owns only interaction; everything that reads the project lives in the
    /// view model. Copying is done here because the clipboard belongs to the window, and a
    /// clipboard another application holds is reported instead of throwing.
    /// </para>
    /// </summary>
    public partial class SheetBrowserWindow : Window
    {
        private readonly SheetBrowserViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public SheetBrowserWindow(SheetBrowserViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _viewModel.Selected = e.NewValue as BrowserNode;
        }

        private void OnRowRightClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem row)
            {
                row.IsSelected = true;
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            BrowserNode? node = _viewModel.Selected;

            if (node == null)
            {
                _viewModel.SetReport(_viewModel.Text.NoSelection);

                return;
            }

            try
            {
                Clipboard.SetText(node.Path);
                _viewModel.SetReport(_viewModel.Text.Copied(node.Path));
            }
            catch (Exception exception)
            {
                _viewModel.SetReport(_viewModel.Text.CopyFailed(exception.Message));
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
