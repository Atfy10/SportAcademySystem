using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        
        public string TokenHash { get; set; } = string.Empty;
        
        public Guid UserId { get; set; }
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public bool IsRevoked { get; set; }
        
        public DateTime? RevokedAt { get; set; }
        
        public int? ReplacedByTokenId { get; set; }
        
        public virtual AppUser User { get; set; } = null!;
        
        public virtual RefreshToken? ReplacedByToken { get; set; }
    }
}
