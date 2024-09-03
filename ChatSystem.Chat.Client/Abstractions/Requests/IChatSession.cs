using ChatSystem.Chat.Common.Requests;
using ChatSystem.Chat.Common.Response;
using ChatSystem.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem.Chat.Client.Abstractions.Requests
{
    public interface IChatSession
    {
        Task<ResponseResult<ChatSessionResponse>> CreateNewChatSession(bool throwOnException = true);
        Task<ResponseResult> SendChatMessage(Guid sessionId, ChatMessageRequest request, bool throwOnException = true);
        Task<ResponseResult<List<MessageResponse>>> GetMessages(Guid sessionId, bool throwOnException = true);
    }
}
