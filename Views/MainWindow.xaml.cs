using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.IO;

namespace ProjectTimeEstimator.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        LoadIcon();
    }

    private void LoadIcon()
    {
        try
        {
            var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "timelineiq.png");
            if (File.Exists(iconPath))
            {
                this.Icon = new BitmapImage(new Uri(iconPath, UriKind.Absolute));
            }
        }
        catch
        {
            // Icon yüklenemezse sessizce devam et
        }
    }

    // Title Bar Drag
    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    // Minimize
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    // Maximize/Restore
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
        }
        else
        {
            this.WindowState = WindowState.Maximized;
        }
    }

    // Close
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}