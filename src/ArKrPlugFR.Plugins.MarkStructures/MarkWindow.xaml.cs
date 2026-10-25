using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.MarkStructures
{
    /// <summary>
    /// The window of the tool: the mark codes, the suffixes and the run button.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives
    /// in the view model, and the result of a run is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class MarkWindow : Window
    {
        private readonly MarkViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public MarkWindow(MarkViewModel viewModel)
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
