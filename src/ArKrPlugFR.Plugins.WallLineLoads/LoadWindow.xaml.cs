using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.WallLineLoads
{
    /// <summary>
    /// The window of the tool: the scope, the load case and the load values.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives in
    /// the view model, and the result of a run is shown in the shared result window.
    /// </para>
    /// </summary>
    public partial class LoadWindow : Window
    {
        private readonly LoadViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public LoadWindow(LoadViewModel viewModel)
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
