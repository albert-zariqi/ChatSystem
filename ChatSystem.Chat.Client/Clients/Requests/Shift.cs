using ChatSystem.Chat.Client.Abstractions.Requests;
using ChatSystem.Utils.Errors.CoreErrors;
using ChatSystem.Utils.Errors;
using ChatSystem.Utils.Exceptions;
using ChatSystem.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatSystem.Chat.Common.Response;

namespace ChatSystem.Chat.Client.Clients.Requests
{
    public class Shift : IShift
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Shift(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<ResponseResult<ShiftCapacityResponse>> GetShiftCapacity(Guid shiftId, bool throwOnException = true)
        {
            var client = _httpClientFactory.CreateClient("ChatClient");

            var response = await client.GetAsync($"api/v1/Shift/capacity/{shiftId}");

            var content = await response.Content.ReadAsStringAsync();

            var responseResult = new ResponseResult<ShiftCapacityResponse>(response.IsSuccessStatusCode, response.StatusCode, content);
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
