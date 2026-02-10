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
    public class ChatConversationRepository : IChatConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatConversationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            ChatConversation conversation,
            CancellationToken cancellationToken)
        {
            await _context.ChatConversations.AddAsync(conversation, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ChatConversation?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _context.ChatConversations
                .Include(x => x.Messages)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
