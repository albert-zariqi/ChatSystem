using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ChatSystem.Chat.API.Layers.Domain.Entities;

namespace ChatSystem.Chat.API.Layers.Application.Infrastructure.Common.Infrastructure
{
    public interface IChatDbContext
    {
        DbSet<Team> Teams { get; set; }
        DbSet<Seniority> Seniorities { get; set; }
        DbSet<Shift> Shifts { get; set; }
        DbSet<Agent> Agents { get; set; }
        DbSet<ChatSession> ChatSessions { get; set; }
        DbSet<ChatMessage> ChatMessages { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        int SaveChanges();
        ChangeTracker ChangeTracker { get; }
        DatabaseFacade Database { get; }

    }
}
