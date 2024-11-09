using Avalonia.Controls;

namespace WorldviewShareClient.Views;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; } = null!;
    public MainWindow()
    {
        Instance = this;
        InitializeComponent();
    }
}