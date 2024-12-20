﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
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
    private bool _canChangeTopic;
    private bool _canSendMessage;
    private int _currentContributions;
    private string _currentTopic;
    private Guid _currentTopicId = Guid.Empty;
    private Guid _currentUserId;
    private bool _isCreateUserEnabled;
    private string _messageField;
    private ObservableCollection<MessageViewModel> _messages = new();
    private string _source;
    private string _userInputUserName;
    private string _userName;

    public MainWindowViewModel()
    {
        CreateUserCommand = new RelayCommand(CreateUser);
        SendMessageCommand = new RelayCommand(SendMessage);
        ChangeTopicCommand = new RelayCommand(async () => await ChangeTopic());

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

        connection.On<string>("RejectJoinSession", Console.WriteLine);
        connection.On<UserResponseDto>("AcceptJoinSession", userResponse =>
        {
            Console.WriteLine("Accepted join session");
            CurrentActiveUser = userResponse.Id;
        });

        connection.On<string>("RejectLeaveSession", Console.WriteLine);
        connection.On("AcceptLeaveSession", () => Console.WriteLine("Accept leave session"));

        connection.On<UserResponseDto>("ChangeActiveUser", userResponse => { CurrentActiveUser = userResponse.Id; });

        connection.On<string>("RejectRegistration", Console.WriteLine);
        connection.On("AcceptRegistration", () => Console.WriteLine("Accepted registration"));

        connection.On<string>("RejectMessage", Console.WriteLine);

        connection.StartAsync().Wait();
        connection.SendAsync("Register", new UserReferenceRequestDto(EnvironmentHelper.GetEnvironment().Id));

        Task.Factory.StartNew(async () => await ChangeTopic());

        var client = HttpClientFactory.GetClient();

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
                Name = authorName,
                Source = messageResponseDto.Source,
                ShowSource = messageResponseDto.Source != null
            });
        });

        Task.Delay(200).Wait();
    }

    public int CurrentContributions
    {
        get => _currentContributions;
        set
        {
            if (Equals(value, _currentContributions)) return;
            _currentContributions = value;
            OnPropertyChanged();
        }
    }

    public Guid CurrentActiveUser
    {
        get => _currentUserId;
        set
        {
            _currentUserId = value;
            if (value == EnvironmentHelper.GetEnvironment().Id && _currentContributions < 5)
                CanSendMessage = true;
            else
                CanSendMessage = false;
        }
    }

    public ObservableCollection<MessageViewModel> Messages
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
    public ICommand ChangeTopicCommand { get; }

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

    public bool CanChangeTopic
    {
        get => _canChangeTopic;
        set
        {
            if (value == _canChangeTopic) return;
            _canChangeTopic = value;
            OnPropertyChanged();
        }
    }

    public bool CanSendMessage
    {
        get => _canSendMessage;
        set
        {
            if (value == _canSendMessage) return;
            _canSendMessage = value;
            OnPropertyChanged();
        }
    }

    public string SourceUri
    {
        get => _source;
        set
        {
            if (value == _source) return;
            _source = value;
            OnPropertyChanged();
        }
    }

    public async Task ChangeTopic()
    {
        var client = HttpClientFactory.GetClient();

        if (_currentTopicId != Guid.Empty)
        {
            Messages.Clear();
            CanChangeTopic = false;
            CurrentContributions = 0;
            await connection.SendAsync("LeaveSession");
        }

        var rawTopicData = await client.GetStringAsync("api/topics");
        var topics = JsonSerializer.Deserialize<List<TopicSessionResponseDto>>(rawTopicData,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (topics!.Count == 0) return;

        var i = 0;

        while (true)
        {
            i = rnd.Next(topics!.Count);

            if (topics![i].Id != _currentTopicId || topics.Count == 1)
            {
                CurrentTopic = topics[i].Topic;
                _currentTopicId = topics[i].Id;
                break;
            }
        }

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
                Name = authorName,
                Source = message.Source,
                ShowSource = message.Source != null
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
        if (MessageField == string.Empty) return;

        if (SourceUri != null && SourceUri != string.Empty &&
            Regex.IsMatch(SourceUri, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"))
            await connection.SendAsync("SendMessage",
                new MessageRequestDto(MessageField, new Uri(SourceUri), _currentTopicId,
                    EnvironmentHelper.GetEnvironment().Id));
        else
            await connection.SendAsync("SendMessage",
                new MessageRequestDto(MessageField, null, _currentTopicId, EnvironmentHelper.GetEnvironment().Id));

        MessageField = string.Empty;
        SourceUri = string.Empty;
        CanChangeTopic = true;
        CurrentContributions++;
        CanSendMessage = false;
    }
}