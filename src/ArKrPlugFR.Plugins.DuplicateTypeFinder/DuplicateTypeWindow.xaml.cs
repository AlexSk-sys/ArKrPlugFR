using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArKrPlugFR.Plugins.DuplicateTypeFinder
{
    /// <summary>
    /// The duplicate window: the two lists, the close matches and the delete.
    /// <para>
    /// The window owns only interaction: which row was clicked, which range a
    /// Shift-click covers, and what to copy to the clipboard. Everything that
    /// changes the model lives in the view model.
    /// </para>
    /// </summary>
    public partial class DuplicateTypeWindow : Window
    {
        private readonly DuplicateFinderViewModel _viewModel;
        private DataGrid? _anchorGrid;
        private int _anchorIndex = -1;

        /// <summary>Creates the window for a finished scan.</summary>
        /// <param name="viewModel">The state to show.</param>
        public DuplicateTypeWindow(DuplicateFinderViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

            InitializeComponent();

            DataContext = _viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DuplicateFinderText text = _viewModel.Text;

            TypeFamilyColumn.Header = text.ColumnFamily;
            TypeCategoryColumn.Header = text.ColumnCategory;
            TypeNameColumn.Header = text.ColumnType;
            TypeKeptColumn.Header = text.ColumnKeptType;
            TypeCountColumn.Header = text.ColumnInstances;
            TypeStatusColumn.Header = text.ColumnStatus;

            FamilyFamilyColumn.Header = text.ColumnFamily;
            FamilyCategoryColumn.Header = text.ColumnCategory;
            FamilyKeptColumn.Header = text.ColumnKeptFamily;
            FamilyMatchedColumn.Header = text.ColumnMatchedTypes;
            FamilyCountColumn.Header = text.ColumnInstances;
            FamilyStatusColumn.Header = text.ColumnStatus;

            NearFamilyColumn.Header = text.ColumnFamily;
            NearFirstColumn.Header = text.ColumnTypeA;
            NearSecondColumn.Header = text.ColumnTypeB;
            NearDifferenceColumn.Header = text.ColumnDifference;
        }

        private void OnSelectAllTypesClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAll();
        }

        private void OnClearTypesClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearSelection();
        }

        private void OnSelectAllFamiliesClick(object sender, RoutedEventArgs e)
        {
            _viewModel.SelectAll();
        }

        private void OnClearFamiliesClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ClearSelection();
        }

        private void OnGridPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not DataGrid grid || e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            // The tick box is an indicator only; every click on the row is handled here.
            if (FindAncestor<DataGridRow>(source)?.Item is not CheckableRow row || !row.IsRemovable)
            {
                return;
            }

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift
                && ReferenceEquals(_anchorGrid, grid)
                && _anchorIndex >= 0)
            {
                int index = grid.ItemContainerGenerator.IndexFromContainer(FindAncestor<DataGridRow>(source)!);

                if (index < 0)
                {
                    return;
                }

                int from = Math.Min(_anchorIndex, index);
                int to = Math.Max(_anchorIndex, index);

                for (int position = from; position <= to; position++)
                {
                    if (grid.Items[position] is CheckableRow item && item.IsRemovable)
                    {
                        item.IsSelected = true;
                    }
                }

                return;
            }

            row.IsSelected = !row.IsSelected;
            _anchorGrid = grid;
            _anchorIndex = grid.ItemContainerGenerator.IndexFromContainer(FindAncestor<DataGridRow>(source)!);
        }

        private void OnGridMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source)
            {
                return;
            }

            if (FindAncestor<DataGridRow>(source)?.Item is not CheckableRow row)
            {
                return;
            }

            if (_viewModel.Zoom(row))
            {
                e.Handled = true;
            }
        }

        private void OnCopyClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_viewModel.BuildCopyText());
            }
            catch (ExternalException)
            {
                // The clipboard is held by another process; the list stays available
                // in the window and can be copied again.
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            int count = _viewModel.SelectedCount;

            if (count == 0)
            {
                MessageBox.Show(
                    this,
                    _viewModel.Text.StatusUsed,
                    _viewModel.Text.ConfirmCaption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            MessageBoxResult answer = MessageBox.Show(
                this,
                _viewModel.Text.ConfirmText(count),
                _viewModel.Text.ConfirmCaption,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            _viewModel.DeleteSelected();
            _anchorGrid = null;
            _anchorIndex = -1;
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static T? FindAncestor<T>(DependencyObject? current)
            where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T typed)
                {
                    return typed;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }
    }
}
