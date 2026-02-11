using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Infrastructure.Persistence.DBContext;
namespace SportAcademy.Infrastructure.Persistence.Repositories
{
    public class ChatMessageRepository : BaseRepository<ChatMessage, Guid>, IChatMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatMessageRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
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
