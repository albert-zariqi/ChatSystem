using ChatSystem.Utils;
using ChatSystem.Utils.Errors.CoreErrors;
using ChatSystem.Utils.Errors;
using ChatSystem.Utils.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ChatSystem.Chat.Client.Abstractions.Requests;
using ChatSystem.Chat.Common.Requests;
using ChatSystem.Chat.Common.Response;

namespace ChatSystem.Chat.Client.Clients.Requests
{
    public class ChatSession : IChatSession
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatSession(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ResponseResult<ChatSessionResponse>> CreateNewChatSession(bool throwOnException = true)
        {
            var client = _httpClientFactory.CreateClient("ChatClient");

            var response = await client.PostAsync($"api/v1/ChatSession/create", null);

            var content = await response.Content.ReadAsStringAsync();

            var responseResult = new ResponseResult<ChatSessionResponse>(response.IsSuccessStatusCode, response.StatusCode, content);
            if (!responseResult.IsSuccessStatusCode)
            {
                if (responseResult.ProblemDetails != null && throwOnException)
                {
                    throw new AppException(new CustomError(responseResult.StatusCode, responseResult.ProblemDetails.Title, responseResult.ProblemDetails.Detail));
                }

                if (throwOnException)
                {
                    throw new AppException(GenericErrors.ThirdPartyFailure);
                }
            }
            return responseResult;
        }

        public async Task<ResponseResult<List<MessageResponse>>> GetMessages(Guid sessionId, bool throwOnException = true)
        {
            var client = _httpClientFactory.CreateClient("ChatClient");

            var response = await client.GetAsync($"api/v1/ChatSession/{sessionId}/messages");

            var content = await response.Content.ReadAsStringAsync();

            var responseResult = new ResponseResult<List<MessageResponse>>(response.IsSuccessStatusCode, response.StatusCode, content);
            if (!responseResult.IsSuccessStatusCode)
            {
                if (responseResult.ProblemDetails != null && throwOnException)
                {
                    throw new AppException(new CustomError(responseResult.StatusCode, responseResult.ProblemDetails.Title, responseResult.ProblemDetails.Detail));
                }

                if (throwOnException)
                {
                    throw new AppException(GenericErrors.ThirdPartyFailure);
                }
            }
            return responseResult;
        }

        public async Task<ResponseResult> AssignAgent(Guid sessionId, string username, bool throwOnException = true)
        {
            var client = _httpClientFactory.CreateClient("ChatClient");

            var response = await client.PostAsync($"api/v1/ChatSession/{sessionId}/assign/{username}", null);

            var content = await response.Content.ReadAsStringAsync();

            var responseResult = new ResponseResult(response.IsSuccessStatusCode, response.StatusCode, content);
            if (!responseResult.IsSuccessStatusCode)
            {
                if (responseResult.ProblemDetails != null && throwOnException)
                {
                    throw new AppException(new CustomError(responseResult.StatusCode, responseResult.ProblemDetails.Title, responseResult.ProblemDetails.Detail));
                }

                if (throwOnException)
                {
                    throw new AppException(GenericErrors.ThirdPartyFailure);
                }
            }
            return responseResult;
        }

        public async Task<ResponseResult> SendChatMessage(Guid sessionId, ChatMessageRequest request, bool throwOnException = true)
        {
            var client = _httpClientFactory.CreateClient("ChatClient");

            string jsonString = JsonSerializer.Serialize(request);
            var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"api/v1/ChatSession/{sessionId}/newmessage", stringContent);

            var content = await response.Content.ReadAsStringAsync();

            var responseResult = new ResponseResult(response.IsSuccessStatusCode, response.StatusCode, content);
            if (!responseResult.IsSuccessStatusCode)
            {
                if (responseResult.ProblemDetails != null && throwOnException)
                {
                    throw new AppException(new CustomError(responseResult.StatusCode, responseResult.ProblemDetails.Title, responseResult.ProblemDetails.Detail));
                }

                if (throwOnException)
                {
                    throw new AppException(GenericErrors.ThirdPartyFailure);
                }
            }
            return responseResult;
        }
    }
}
