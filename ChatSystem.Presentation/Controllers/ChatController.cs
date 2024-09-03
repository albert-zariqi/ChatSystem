using Microsoft.AspNetCore.Mvc;
using Quartz;

namespace ChatSystem.Presentation.Controllers
{
    public class ChatController : Controller
    {
        private readonly ChatSessionManager _chatSessionManager;
        private readonly ISchedulerFactory _schedulerFactory;

        public ChatController(
            ChatSessionManager chatSessionManager,
            ISchedulerFactory schedulerFactory
            )
        {
            _chatSessionManager = chatSessionManager;
            _schedulerFactory = schedulerFactory;
        }

        public async Task<IActionResult> Index(Guid SessionId)
        {
            if (_chatSessionManager.SessionExists(SessionId))
            {
                ViewBag.InfoMessage = "Session already underway";
                return RedirectToAction("Index", "Home");
            }
            return View();
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

            return Ok(new { message = "Chat session ended successfully." });
        }
    }
}
