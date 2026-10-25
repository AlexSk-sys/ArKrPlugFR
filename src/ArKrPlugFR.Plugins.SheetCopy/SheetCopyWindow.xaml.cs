using System;
using System.Windows;
using ArKrPlugFR.Core.Diagnostics;

namespace ArKrPlugFR.Plugins.SheetCopy
{
    /// <summary>
    /// The sheet-copy window: the sheets of this project with their tick boxes and
    /// copy counts, the open projects that can receive them, and the single Copy
    /// button.
    /// <para>
    /// The window owns only interaction — which buttons were clicked and how the
    /// result is shown. Everything that reads or changes the model lives in the
    /// view model.
    /// </para>
    /// </summary>
    public partial class SheetCopyWindow : Window
    {
        private readonly SheetCopyViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public SheetCopyWindow(SheetCopyViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnSelectAllClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAll();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearSelection();
        }

        private void OnSelectAllTargetsClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAllTargets();
        }

        private void OnClearTargetsClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearTargets();
        }

        private void OnIncreaseClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SheetRowViewModel row)
            {
                row.Increase();
            }
        }

        private void OnDecreaseClick(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is SheetRowViewModel row)
            {
                row.Decrease();
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            // Nothing ticked on either side means no transaction at all: the run is
            // not even started, and the message says which half is missing.
            if (!_viewModel.HasSelection || _viewModel.SelectedTargetCount == 0)
            {
                string missing = _viewModel.HasSelection
                    ? _viewModel.Text.NoTargetSelected
                    : _viewModel.Text.NothingSelected;

                MessageBox.Show(
                    this,
                    missing + Environment.NewLine + Environment.NewLine +
                        _viewModel.Text.LogFolder(AppPaths.LogDirectory),
                    _viewModel.Text.WindowTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            SheetCopyRunResult result;

            try
            {
                result = _viewModel.CopySelected();
            }
            catch (Exception exception)
            {
                _viewModel.SetReport(_viewModel.Text.GeneralFailure(exception.Message));

                MessageBox.Show(
                    this,
                    _viewModel.Text.GeneralFailure(exception.Message),
                    _viewModel.Text.ReportCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return;
            }

            string report = _viewModel.BuildReport(result);
            _viewModel.SetReport(report);

            MessageBox.Show(
                this,
                report,
                _viewModel.Text.ReportCaption,
                MessageBoxButton.OK,
                result.HasFailures || result.Created == 0
                    ? MessageBoxImage.Warning
                    : MessageBoxImage.Information);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
