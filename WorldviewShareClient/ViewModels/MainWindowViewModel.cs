using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WorldviewShareClient.Models;
using WorldviewShareClient.Models.Users;

namespace WorldviewShareClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly EnvironmentSettings _environmentSettings;
    private bool _isCreateUserEnabled;
    private string _userInputUserName;
    private string _userName;

    public MainWindowViewModel()
    {
        CreateUserCommand = new RelayCommand(CreateUser);

        _environmentSettings = EnvironmentHelper.GetEnvironment();

        if (_environmentSettings.Name == string.Empty)
        {
            UserName = "Create user";
            isCreateUserEnabled = true;
        }
        else
        {
            UserName = _environmentSettings.Name;
        }
    }

    public string UserName
    {
        get => _userName;
        set
        {
            if (value == _userName) return;
            _userName = value;
            isCreateUserEnabled = false;
            OnPropertyChanged();
        }
    }

    public ICommand CreateUserCommand { get; }

    public bool isCreateUserEnabled
    {
        get => _isCreateUserEnabled;
        set
        {
            if (value == _isCreateUserEnabled) return;
            _isCreateUserEnabled = value;
            OnPropertyChanged();
        }
    }

    public string UserInputUserName
    {
        get => _userInputUserName;
        set
        {
            if (value == _userInputUserName) return;
            _userInputUserName = value;
            OnPropertyChanged();
        }
    }

    private async void CreateUser()
    {
        _environmentSettings.Name = UserInputUserName;
        UserName = _environmentSettings.Name;
        EnvironmentHelper.SaveEnvironment(_environmentSettings);

        var dto = new CreateUserDto
        {
            Username = _environmentSettings.Name,
            Id = _environmentSettings.Id
        };

        var test = await HttpClientFactory.GetClient()
            .PostAsync("api/users",
                new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

        Console.WriteLine(test.Content.ReadAsStringAsync().Result);
    }
}