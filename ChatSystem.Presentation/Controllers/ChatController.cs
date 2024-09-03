using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Presentation.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace ChatSystem.Presentation.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatSessionManager _chatSessionManager;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IChatClient _chatClient;

        public ChatController(
            ChatSessionManager chatSessionManager,
            ISchedulerFactory schedulerFactory,
            IChatClient chatClient
            )
        {
            _chatSessionManager = chatSessionManager;
            _schedulerFactory = schedulerFactory;
            _chatClient = chatClient;
        }

        public async Task<IActionResult> Index(Guid? SessionId)
        {
            if (SessionId.HasValue && _chatSessionManager.SessionExists(SessionId.Value))
            {
                var messages = await _chatClient.ChatSession.GetMessages(SessionId.Value);

                return View(new ChatViewModel { Messages = messages.Data });
            }
            return View(new ChatViewModel { });
        }

        [HttpPost]
        public async Task<IActionResult> EndSession(Guid sessionId)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobKey = new JobKey(sessionId.ToString(), "PollingGroup");

            if (await scheduler.CheckExists(jobKey))
            {
                await scheduler.DeleteJob(jobKey);
            }

            _chatSessionManager.RemoveSession(sessionId);
            await _chatClient.ChatSession.EndSession(sessionId);

            return Ok(new { message = "Chat session ended successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] MessageBody request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            await _chatClient.ChatSession.SendChatMessage(sessionId, new Chat.Common.Requests.ChatMessageRequest
            {
                Message = request.Message,
                FromAgent = false,
                Sender = "UserName here"
            });

            // Optionally, you can return some data back to the client
            return Ok(new { Success = true, Message = "Message sent successfully." });
        }

    }

    public class MessageBody
    {
        public string Message { get; set; }
    }
}
