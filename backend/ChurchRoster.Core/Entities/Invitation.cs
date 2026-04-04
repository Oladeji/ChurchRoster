namespace ChurchRoster.Core.Entities
{
    public class Invitation
    {
        public int InvitationId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public int? UserId { get; set; } // Set when invitation is accepted
        public int CreatedBy { get; set; }
        public User? CreatedByUser { get; set; }
        public User? User { get; set; }
    }
}
