using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ChatMessageRepository : IChatMessageRepository
    {
        private readonly SportAcademyDbContext _context;

        public ChatMessageRepository(SportAcademyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ChatMessage message,
            CancellationToken cancellationToken)
        {
            await _context.ChatMessages.AddAsync(message, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<ChatMessage>> GetByConversationIdAsync(
            Guid conversationId,
            CancellationToken cancellationToken)
        {
            return await _context.ChatMessages
                .Where(x => x.ChatConversationId == conversationId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }

}
