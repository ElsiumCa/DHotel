namespace FrontDesk.API.Entities
{
    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}