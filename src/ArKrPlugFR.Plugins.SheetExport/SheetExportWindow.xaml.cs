using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.SheetExport
{
    /// <summary>
    /// The window of the tool: the sheets with their ticks and the folder.
    /// <para>
    /// The window owns only interaction; the export itself lives in the view model and the
    /// engine. The folder picker is the one thing WPF does not offer, so it comes from
    /// Windows Forms, which is part of the framework the add-in already targets.
    /// </para>
    /// </summary>
    public partial class SheetExportWindow : Window
    {
        private readonly SheetExportViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public SheetExportWindow(SheetExportViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnTickAllClick(object sender, RoutedEventArgs e)
        {
            _viewModel.TickAll();
        }

        private void OnUntickAllClick(object sender, RoutedEventArgs e)
        {
            _viewModel.UntickAll();
        }

        private void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.Description = _viewModel.Text.FolderLabel;
                dialog.SelectedPath = _viewModel.Folder;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    _viewModel.Folder = dialog.SelectedPath;
                }
            }
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.Subtitle,
                _viewModel.ReportCaption,
                _viewModel.Apply());
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
