using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WorldviewShareClient.Views;

namespace WorldviewShareClient.ViewModels;

public class MessageViewModel : ViewModelBase
{
    private string _message;
    private string _name;
    private bool _showSource;
    private Uri? _source;

    public MessageViewModel()
    {
        HandleLinkCommand = new RelayCommand(async () =>
        {
            var launcher = MainWindow.Instance.Launcher;
            if (_source != null)
            {
                await launcher.LaunchUriAsync(_source);
            }
            else
            {
                throw new InvalidOperationException("Source is null");
            }
        });
    }

    public string Message
    {
        get => _message;
        set
        {
            if (value == _message) return;
            _message = value;
            OnPropertyChanged();
        }
    }

    public ICommand HandleLinkCommand { get; }

    public bool ShowSource
    {
        get => _showSource;
        set
        {
            if (value == _showSource) return;
            _showSource = value;
            OnPropertyChanged();
        }
    }

    public Uri? Source
    {
        get => _source;
        set
        {
            if (Equals(value, _source)) return;
            _source = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            _name = value;
            OnPropertyChanged();
        }
    }
}