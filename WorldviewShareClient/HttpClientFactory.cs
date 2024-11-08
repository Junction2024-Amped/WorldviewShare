using System;
using System.Net.Http;

namespace WorldviewShareClient;

public static class HttpClientFactory
{
    private static HttpClient Client { get; set; }

    public static HttpClient GetClient()
    {
        if (Client == null)
            Client = new HttpClient { BaseAddress = new Uri("http://localhost:5140") };

        return Client;
    }
}