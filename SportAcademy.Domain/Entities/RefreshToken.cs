using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace SportAcademy.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(64)]
        public string TokenHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; } = string.Empty;
        
        public DateTime ExpiresAt { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public bool IsRevoked { get; set; }
        
        public DateTime? RevokedAt { get; set; }
        
        public int? ReplacedByTokenId { get; set; }
        
        [ForeignKey(nameof(UserId))]
        public virtual AppUser User { get; set; } = null!;
        
        [ForeignKey(nameof(ReplacedByTokenId))]
        public virtual RefreshToken? ReplacedByToken { get; set; }
    }
}
