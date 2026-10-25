using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.SelectTypes
{
    /// <summary>
    /// The window of the tool: the scope, the search, the type tree and the run button.
    /// <para>
    /// The window owns only interaction; everything that reads the model lives in the view
    /// model. The result of a run is shown in the window and in the shared result window.
    /// </para>
    /// </summary>
    public partial class SelectTypesWindow : Window
    {
        private readonly SelectTypesViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public SelectTypesWindow(SelectTypesViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnTickAllClick(object sender, RoutedEventArgs e)
        {
            _viewModel.TickAll(true);
        }

        private void OnUntickAllClick(object sender, RoutedEventArgs e)
        {
            _viewModel.TickAll(false);
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
