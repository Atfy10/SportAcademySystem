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
    public class ChatConversationRepository : BaseRepository<ChatConversation, Guid>, IChatConversationRepository
    {
        private readonly ApplicationDbContext _context;

        public ChatConversationRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
