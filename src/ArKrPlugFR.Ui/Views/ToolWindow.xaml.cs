using System;
using System.Windows;

namespace ArKrPlugFR.Ui.Views
{
    /// <summary>
    /// Shared shell for plugin windows: a title, a subtitle, a body supplied by
    /// the plugin, an optional report area and a footer.
    /// <para>
    /// The window only carries presentation. It knows nothing about Revit and
    /// nothing about specific tools, so it can be reused as is across plugins.
    /// </para>
    /// </summary>
    public partial class ToolWindow : Window
    {
        /// <summary>Identifies the <see cref="Subtitle"/> property.</summary>
        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(ToolWindow),
            new PropertyMetadata(string.Empty));

        /// <summary>Identifies the <see cref="Body"/> property.</summary>
        public static readonly DependencyProperty BodyProperty = DependencyProperty.Register(
            nameof(Body),
            typeof(object),
            typeof(ToolWindow),
            new PropertyMetadata(null));

        /// <summary>Identifies the <see cref="Report"/> property.</summary>
        public static readonly DependencyProperty ReportProperty = DependencyProperty.Register(
            nameof(Report),
            typeof(string),
            typeof(ToolWindow),
            new PropertyMetadata(string.Empty, OnReportChanged));

        /// <summary>Identifies the <see cref="ReportCaption"/> property.</summary>
        public static readonly DependencyProperty ReportCaptionProperty = DependencyProperty.Register(
            nameof(ReportCaption),
            typeof(string),
            typeof(ToolWindow),
            new PropertyMetadata("Report", OnReportCaptionChanged));

        /// <summary>Initializes a new instance of the <see cref="ToolWindow"/> class.</summary>
        public ToolWindow()
        {
            InitializeComponent();
        }

        /// <summary>Gets or sets the secondary line under the title, for example the document name.</summary>
        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        /// <summary>Gets or sets the tool-specific content of the window.</summary>
        public object? Body
        {
            get => GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        /// <summary>Gets or sets the report text; an empty value hides the report area.</summary>
        public string Report
        {
            get => (string)GetValue(ReportProperty);
            set => SetValue(ReportProperty, value);
        }

        /// <summary>Gets or sets the caption shown above the report text.</summary>
        public string ReportCaption
        {
            get => (string)GetValue(ReportCaptionProperty);
            set => SetValue(ReportCaptionProperty, value);
        }

        private static void OnReportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToolWindow window)
            {
                window.RefreshReportVisibility();
            }
        }

        private static void OnReportCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToolWindow window)
            {
                window.RefreshReportVisibility();
            }
        }

        private void RefreshReportVisibility()
        {
            bool hasReport = !string.IsNullOrWhiteSpace(Report);

            ReportPanel.Visibility = hasReport ? Visibility.Visible : Visibility.Collapsed;
            CopyReportButton.Visibility = hasReport ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnCopyReportClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Report))
            {
                return;
            }

            try
            {
                Clipboard.SetText(Report);
            }
            catch (Exception)
            {
                // The clipboard can be locked by another process; the report stays
                // selectable in the window, so this is not worth interrupting the user.
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
