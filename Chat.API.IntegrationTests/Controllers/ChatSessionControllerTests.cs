using Chat.API.IntegrationTests.Core;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Application.Services;
using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Application.Services;
using ChatSystem.Chat.Common.Requests;
using ChatSystem.Chat.Common.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chat.API.IntegrationTests.Controllers
{
    public class ChatSessionControllerTests : TestBase, IClassFixture<ChatApiFactory>
    {
        private readonly ChatApiFactory _chatApiFactory;

        public ChatSessionControllerTests(ChatApiFactory chatApiFactory) : base(chatApiFactory)
        {
            _chatApiFactory = chatApiFactory;
        }

        [Fact]
        public async Task CreateSession_ShouldReturn200Ok()
        {
            #region Arrange
            var client = _chatApiFactory.CreateClient();

            // Sample request payload. Adjust according to your model.
            var request = new ChatSessionRequest
            {
                SessionId = Guid.NewGuid()
            };
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            #endregion

            #region Act
            var response = await client.PostAsync("/api/v1/ChatSession/create", content); // Replace with your session creation endpoint
            #endregion

            #region Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Deserialize and validate the response content if needed
            var sessionResponse = JsonConvert.DeserializeObject<ChatSessionResponse>(responseBody); // Adjust model to match your API response
            Assert.True(sessionResponse.Available);
            Assert.NotNull(sessionResponse.SessionId);

            // Is this session in Db?
            using var scope = _chatApiFactory.Services.CreateScope();
            var chatDbContext = scope.ServiceProvider.GetRequiredService<IChatDbContext>();
            var chatSession = await chatDbContext.ChatSessions.FirstOrDefaultAsync(x => x.Id == sessionResponse.SessionId);
            Assert.NotNull(chatSession);

            #endregion
        }
    }
}
