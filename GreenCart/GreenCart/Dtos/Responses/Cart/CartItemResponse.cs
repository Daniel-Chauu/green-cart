namespace GreenCart.Dtos.Responses.Cart
{
    public class CartItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class CartResponse
    {
        public List<CartItemResponse> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public int TotalItems { get; set; }
    }
}
