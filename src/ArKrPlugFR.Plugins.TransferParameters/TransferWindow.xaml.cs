using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;

namespace ArKrPlugFR.Plugins.TransferParameters
{
    /// <summary>
    /// The window of the tool: three transfers, one per tab, with a shared report.
    /// <para>
    /// The window owns only interaction; everything that reads or changes the model lives
    /// in the view model. The window hides itself while the user picks in the model and
    /// returns after the pick, and the result of a run is shown in the shared result
    /// window.
    /// </para>
    /// </summary>
    public partial class TransferWindow : Window
    {
        private readonly TransferViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public TransferWindow(TransferViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnPickSourceClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickSourceElement);
        }

        private void OnPickStructuresClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickStructures);
        }

        private void OnPickRoomsClick(object sender, RoutedEventArgs e)
        {
            Pick(_viewModel.PickRooms);
        }

        private void OnSelectAllSheetsClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAllSheets(true);
        }

        private void OnClearSheetsClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAllSheets(false);
        }

        private void OnApplySheetsClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplySheets());
        }

        private void OnApplyStructuresClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyStructures());
        }

        private void OnApplyRoomsClick(object sender, RoutedEventArgs e)
        {
            ShowReport(_viewModel.ApplyRooms());
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Pick(Action pick)
        {
            Hide();

            try
            {
                pick();
            }
            finally
            {
                Show();
                Activate();
            }
        }

        private void ShowReport(string report)
        {
            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.Subtitle,
                _viewModel.ReportCaption,
                report);
        }
    }
}
