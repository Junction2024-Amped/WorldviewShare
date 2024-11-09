using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.SignalR.Client;
using WorldviewShareClient.Models;
using WorldviewShareShared.DTO.Request.Messages;
using WorldviewShareShared.DTO.Request.TopicSessions;
using WorldviewShareShared.DTO.Request.Users;
using WorldviewShareShared.DTO.Response.Messages;
using WorldviewShareShared.DTO.Response.TopicSessions;
using WorldviewShareShared.DTO.Response.Users;

namespace WorldviewShareClient.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly EnvironmentSettings _environmentSettings;

    private readonly HubConnection connection;
    private readonly List<UserResponseDto> knownUsers = new();

    private readonly Random rnd = new();
    private string _currentTopic;
    private Guid _currentTopicId;
    private bool _isCreateUserEnabled;
    private string _messageField;
    private List<MessageViewModel> _messages = new();
    private string _userInputUserName;
    private string _userName;

    public MainWindowViewModel()
    {
        CreateUserCommand = new RelayCommand(CreateUser);
        SendMessageCommand = new RelayCommand(SendMessage);

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

        connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5140/messages")
            .WithAutomaticReconnect()
            .Build();

        connection.StartAsync().Wait();
        connection.SendAsync("Register", new UserReferenceRequestDto(EnvironmentHelper.GetEnvironment().Id));

        Task.Factory.StartNew(async () => await ChangeTopic());

        var client = HttpClientFactory.GetClient();

        connection.On<string>("RejectJoinSession", Console.WriteLine);
        connection.On("AcceptJoinSession", () => Console.WriteLine("Accepted join session"));

        connection.On<string>("RejectRegistration", Console.WriteLine);
        connection.On("AcceptRegistration", () => Console.WriteLine("Accepted registration"));

        connection.On<MessageResponseDto>("ReceiveMessage", messageResponseDto =>
        {
            var authorName = string.Empty;

            if (knownUsers.Any(x => x.Id == messageResponseDto.AuthorId))
            {
                authorName = knownUsers.First(x => x.Id == messageResponseDto.AuthorId).Username;
            }
            else
            {
                var rawUserDate = client.GetStringAsync($"api/users/{messageResponseDto.AuthorId}").Result;
                var user = JsonSerializer.Deserialize<UserResponseDto>(rawUserDate,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                knownUsers.Add(user!);
                authorName = user!.Username;
            }

            Messages.Add(new MessageViewModel
            {
                Message = messageResponseDto.Content,
                Name = authorName
            });
        });
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
    public ICommand SendMessageCommand { get; }

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

    public string MessageField
    {
        get => _messageField;
        set
        {
            if (value == _messageField) return;
            _messageField = value;
            OnPropertyChanged();
        }
    }

    public async Task ChangeTopic()
    {
        // TODO: Leave session if one exists
        var client = HttpClientFactory.GetClient();

        var rawTopicData = await client.GetStringAsync("api/topics");
        var topics = JsonSerializer.Deserialize<List<TopicSessionResponseDto>>(rawTopicData,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (topics!.Count == 0) return;

        var i = rnd.Next(topics!.Count);

        CurrentTopic = topics[i].Topic;
        _currentTopicId = topics[i].Id;

        var rawMessageData = await client.GetStringAsync($"api/topics/{topics[i].Id}/messages");
        var messages = JsonSerializer.Deserialize<List<MessageResponseDto>>(rawMessageData,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        foreach (var message in messages!)
        {
            var authorName = string.Empty;

            if (knownUsers.Any(x => x.Id == message.AuthorId))
            {
                authorName = knownUsers.First(x => x.Id == message.AuthorId).Username;
            }
            else
            {
                var rawUserDate = await client.GetStringAsync($"api/users/{message.AuthorId}");
                var user = JsonSerializer.Deserialize<UserResponseDto>(rawUserDate,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                knownUsers.Add(user!);
                authorName = user!.Username;
            }

            Messages.Add(new MessageViewModel
            {
                Message = message.Content,
                Name = authorName
            });
        }

        // Join session
        await connection.SendAsync("JoinSession", new TopicSessionReferenceRequestDto(topics[i].Id));
    }

    private async void CreateUser()
    {
        _environmentSettings.Name = UserInputUserName;

        var dto = new UserRequestDto(_environmentSettings.Name);

        try
        {
            var user = await HttpClientFactory.GetClient()
                .PostAsync("api/users",
                    new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json"));

            if (!user.IsSuccessStatusCode) return;

            var responseContent =
                JsonSerializer.Deserialize<UserResponseDto>(user.Content.ReadAsStringAsync().Result,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (responseContent != null) _environmentSettings.Id = responseContent.Id;
            EnvironmentHelper.SaveEnvironment(_environmentSettings);
            UserName = _environmentSettings.Name;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async void SendMessage()
    {
        await connection.SendAsync("SendMessage",
            new MessageRequestDto(MessageField, _currentTopicId, EnvironmentHelper.GetEnvironment().Id));

        MessageField = string.Empty;
    }
}