using ChatSystem.Chat.Client.Configurations;
using ChatSystem.Presentation.Hubs;
using ChatSystem.Presentation;
using ChatSystem.Chat.Client.Extensions;
using Quartz;
using ChatSystem.Presentation.BackgroundServices;
using ChatSystem.Presentation.Services;
using ChatSystem.Presentation.Configurations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var chatApiClientConfiguration = new ChatApiClientConfiguration();
builder.Configuration.Bind("ChatApiClientConfiguration", chatApiClientConfiguration);
builder.Services.AddChatApiClient(chatApiClientConfiguration);
builder.Services.AddSingleton<ChatSessionManager>();
builder.Services.AddScoped<IRedisQueue, RedisQueue>();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(nameof(ConnectionStrings)));

builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory(); // Use DI for Quartz jobs

    // Register the job and mark it as durable^
    q.AddJob<ChatPollingJob>(opts => opts
        .WithIdentity("ChatPollingJob")
        .StoreDurably()); // <-- This is important
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<ChatHub>("/chatHub");

app.Run();