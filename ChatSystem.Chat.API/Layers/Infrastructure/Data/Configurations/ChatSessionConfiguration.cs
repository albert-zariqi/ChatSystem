using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ChatSystem.Chat.API.Layers.Domain.Entities;

namespace ChatSystem.Chat.API.Layers.Infrastructure.Data.Configurations
{
    public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
    {
        public void Configure(EntityTypeBuilder<ChatSession> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Status).IsRequired();

            builder.HasOne(x => x.Agent)
                .WithMany(x => x.ChatSessions)
                .HasForeignKey(x => x.AgentId);
        }
    }
}
