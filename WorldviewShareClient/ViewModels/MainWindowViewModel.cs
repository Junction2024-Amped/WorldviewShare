using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using WorldviewShareClient.Models;
using WorldviewShareShared.DTO.Users.Create;
using WorldviewShareShared.DTO.Users.Get;

namespace WorldviewShareClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly EnvironmentSettings _environmentSettings;
    private string _currentTopic;
    private bool _isCreateUserEnabled;
    private List<MessageViewModel> _messages = new();
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

    public List<MessageViewModel> Messages
    {
        get => _messages;
        set
        {
            if (Equals(value, _messages)) return;
            _messages = value;
            OnPropertyChanged();
        }
    }

    public string CurrentTopic
    {
        get => _currentTopic;
        set
        {
            if (value == _currentTopic) return;
            _currentTopic = value;
            OnPropertyChanged();
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

        var dto = new CreateUserDto
        {
            Username = _environmentSettings.Name,
            Id = _environmentSettings.Id
        };

        try
        {
            var test = await HttpClientFactory.GetClient()
                .PostAsync("api/users",
                    new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

            if (test.IsSuccessStatusCode)
            {
                var responseContent = JsonSerializer.Deserialize<GetUserDto>(test.Content.ReadAsStringAsync().Result);
                if (responseContent != null) _environmentSettings.Id = responseContent.Id;
                EnvironmentHelper.SaveEnvironment(_environmentSettings);
                UserName = _environmentSettings.Name;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}