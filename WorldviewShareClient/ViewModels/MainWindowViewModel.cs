namespace WorldviewShareClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
#pragma warning disable CA1822 // Mark members as static
    public string Text { get; set; } = EnvironmentHelper.GetEnvironment().Id.ToString();
#pragma warning restore CA1822 // Mark members as static
}