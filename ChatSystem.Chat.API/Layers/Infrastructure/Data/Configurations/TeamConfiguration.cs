using ChatSystem.Chat.API.Layers.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ChatSystem.Chat.API.Layers.Infrastructure.Data.Configurations
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.IsMainTeam).HasDefaultValue(true).IsRequired();

            builder.HasOne(x => x.Shift)
                .WithMany()
                .HasForeignKey(x => x.ShiftId);

        }
    }
}
