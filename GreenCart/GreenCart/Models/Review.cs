namespace GreenCart.Entities
{
    public class Review : BaseEntity
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
        public bool IsApproved { get; set; } = false;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
