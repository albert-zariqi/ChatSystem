using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Chat.Client.Abstractions.Requests;
using ChatSystem.Chat.Client.Clients;
using ChatSystem.Chat.Client.Clients.Requests;
using ChatSystem.Chat.Client.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace ChatSystem.Chat.Client.Extensions
{
    public static class ChatApiClientExtensions
    {
        public static void AddChatApiClient(this IServiceCollection services, ChatApiClientConfiguration configuration)
        {
            services.AddHttpClient("ChatClient", client =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var httpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();

                client.BaseAddress = new Uri(configuration.BaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            });

            services.AddScoped<IChatClient, ChatClient>();
            services.AddScoped<IChatSession, ChatSession>();
            services.AddScoped<IShift, Shift>();
        }
    }
}
