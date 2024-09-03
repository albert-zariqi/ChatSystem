using ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure;
using ChatSystem.Chat.API.Layers.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ChatSystem.Chat.API.Layers.Infrastructure.Data
{
    public class ChatDbContext(DbContextOptions<ChatDbContext> options) : DbContext(options), IChatDbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Seniority> Seniorities { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(builder);
        }
    }
}
