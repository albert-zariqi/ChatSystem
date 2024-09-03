using ChatSystem.Chat.Client.Abstractions;
using ChatSystem.Presentation.BackgroundServices;
using ChatSystem.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using Quartz;
using System.Diagnostics;

namespace ChatSystem.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IChatClient _chatClient;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ChatSessionManager _chatSessionManager;

        public HomeController(
            ILogger<HomeController> logger,
            IChatClient chatClient,
            ISchedulerFactory schedulerFactory,
            ChatSessionManager chatSessionManager
            )
        {
            _logger = logger;
            _chatClient = chatClient;
            _schedulerFactory = schedulerFactory;
            _chatSessionManager = chatSessionManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> StartNewChat()
        {
            var response = await _chatClient.ChatSession.CreateNewChatSession(throwOnException: false);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.ErrorMessage = "Unable to start a new chat. Please try again later.";
                return View("Index");
            }

            if (!response.Data.Available)
            {
                ViewBag.InfoMessage = "No agents are available at the moment. Please try again later.";
                return View("Index");
            }

            // Schedule the Quartz job for this session
            var scheduler = await _schedulerFactory.GetScheduler();

            var jobDetail = JobBuilder.Create<ChatPollingJob>()
                .WithIdentity(response.Data.SessionId.ToString()!, "PollingGroup")
                .UsingJobData("SessionId", response.Data.SessionId.ToString())
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("ChatPollingTrigger", "PollingGroup")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(1).RepeatForever()) // Poll every 10 seconds
                .Build();

            await scheduler.ScheduleJob(jobDetail, trigger);

            _chatSessionManager.AddSession(response.Data.SessionId.Value);

            return RedirectToAction("Index", "Chat", new { SessionId = response.Data.SessionId });
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
