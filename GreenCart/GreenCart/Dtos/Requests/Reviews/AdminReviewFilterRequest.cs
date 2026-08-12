namespace GreenCart.Dtos.Requests.Reviews
{
    public class AdminReviewFilterRequest
    {
        public int? ProductId { get; set; }
        public bool? IsApproved { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
