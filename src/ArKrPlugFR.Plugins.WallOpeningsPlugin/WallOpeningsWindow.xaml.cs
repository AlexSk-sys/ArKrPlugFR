using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.WallOpeningsPlugin
{
    /// <summary>
    /// The options window of the tool: the two switches, the size of the selection and
    /// the Apply button.
    /// <para>
    /// The window owns only interaction. Everything that reads or changes the model lives
    /// in the view model, and the result of a run is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class WallOpeningsWindow : Window
    {
        private readonly WallOpeningsViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public WallOpeningsWindow(WallOpeningsViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            string report = _viewModel.Apply();

            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.SelectionLine,
                _viewModel.ReportCaption,
                report);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
