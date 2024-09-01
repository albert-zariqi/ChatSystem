using ChatSystem.Chat.Client.Abstractions.Requests;
using ChatSystem.Coordinator.ApiClient.Abstractions;
using ChatSystem.Coordinator.ApiClient.Clients;
using ChatSystem.Coordinator.ApiClient.Clients.Requests;
using ChatSystem.Coordinator.ApiClient.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http.Headers;

namespace ChatSystem.Coordinator.ApiClient.Extensions
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

            
        }
    }
}
