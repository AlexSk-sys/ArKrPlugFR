using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.ConcreteExport
{
    /// <summary>
    /// The window of the tool: the scope, the file and the export button.
    /// <para>
    /// The window owns only interaction; everything that reads the project lives in the
    /// view model. The file dialog is a window concern and stays here, and the result of a
    /// run is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class ConcreteExportWindow : Window
    {
        private readonly ConcreteExportViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public ConcreteExportWindow(ConcreteExportViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnChooseFileClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ChooseFile();
        }

        private void OnExportClick(object sender, RoutedEventArgs e)
        {
            string report = _viewModel.Apply();

            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.Subtitle,
                _viewModel.ReportCaption,
                report);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
