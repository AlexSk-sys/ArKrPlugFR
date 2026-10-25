using System;
using System.Windows;
using ArKrPlugFR.Ui.Views;
using Autodesk.Revit.DB;

namespace ArKrPlugFR.Plugins.CutHolesInConcrete
{
    /// <summary>
    /// The options window of the tool: the clearance, the hint about the linked model
    /// and the apply button.
    /// <para>
    /// The window owns only interaction. Everything that reads or changes the model
    /// lives in the view model. The window hides itself while the user picks the link
    /// and returns after the pick, and the result of a run is shown in the shared
    /// result window.
    /// </para>
    /// </summary>
    public partial class CutHolesWindow : Window
    {
        private readonly CutHolesViewModel _viewModel;

        /// <summary>Creates the window of the tool.</summary>
        /// <param name="viewModel">The state to show.</param>
        public CutHolesWindow(CutHolesViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
        }

        private void OnApplyClick(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.TryReadClearance(out string clearanceError))
            {
                _viewModel.SetReport(clearanceError);

                return;
            }

            RevitLinkInstance? link;

            Hide();

            try
            {
                link = _viewModel.PickLink();
            }
            finally
            {
                Show();
                Activate();
            }

            if (link == null)
            {
                return;
            }

            string report = _viewModel.Apply(link);

            ToolWindowPresenter.Show(
                _viewModel.WindowTitle,
                _viewModel.LinkLine,
                _viewModel.ReportCaption,
                report);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
