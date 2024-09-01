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

namespace ChatSystem.Coordinator.ApiClient.Clients.Requests
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
                if (responseResult.ErrorDetails != null && throwOnException)
                {
                    throw new AppException(new CustomError(responseResult.StatusCode, responseResult.ErrorDetails.ErrorMessage, responseResult.ErrorDetails.ErrorExceptionMessage));
                }

                if (throwOnException)
                {
                    throw new AppException(GenericErrors.ThirdPartyFailure);
                }
            }
            return responseResult;
        }

        public async Task<ResponseResult<ChatPollResponse>> PollSession(Guid session, bool throwOnException = true)
        {
            var client = _httpClientFactory.CreateClient("ChatClient");

            var response = await client.GetAsync($"api/v1/ChatSession/poll");

            var content = await response.Content.ReadAsStringAsync();

            var responseResult = new ResponseResult<ChatPollResponse>(response.IsSuccessStatusCode, response.StatusCode, content);
            if (!responseResult.IsSuccessStatusCode)
            {
                if (responseResult.ErrorDetails != null && throwOnException)
                {
                    throw new AppException(new CustomError(responseResult.StatusCode, responseResult.ErrorDetails.ErrorMessage, responseResult.ErrorDetails.ErrorExceptionMessage));
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
