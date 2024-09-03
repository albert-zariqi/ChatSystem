using ChatSystem.Chat.API.Layers.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Infrastructure.Data.Configurations
{
    public class AgentConfiguration : IEntityTypeConfiguration<Agent>
    {
        public void Configure(EntityTypeBuilder<Agent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.FirstName).HasMaxLength(100);
            builder.Property(x => x.LastName).HasMaxLength(100);
            builder.Property(x => x.Username).HasMaxLength(500);

            builder.HasIndex(x => x.Username)
                .IsUnique();

            builder.HasOne(x => x.Seniority)
                .WithMany()
                .HasForeignKey(x => x.SeniorityId);

            builder.HasOne(x => x.Team)
                .WithMany(x => x.Agents)
                .HasForeignKey(x => x.TeamId);
        }
    }
}
