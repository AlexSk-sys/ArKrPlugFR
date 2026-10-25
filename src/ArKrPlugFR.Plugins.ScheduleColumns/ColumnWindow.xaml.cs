using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.ScheduleColumns
{
    /// <summary>
    /// The window of the tool: the chosen schedule and the run.
    /// <para>
    /// The window owns only interaction; the reading and the hiding live in the view model and
    /// the engine, and the result of a run is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class ColumnWindow : Window
    {
        private readonly ColumnViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public ColumnWindow(ColumnViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
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
