using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportAcademy.Domain.Entities;


namespace SportAcademy.Infrastructure.Persistence.Configurations
{
    public class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
    {
        public void Configure(EntityTypeBuilder<ChatConversation> builder)
        {
            builder.ToTable("ChatConversations");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasMany(x => x.Messages)
                .WithOne(x => x.ChatConversation)
                .HasForeignKey(x => x.ChatConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
