using FrontDesk.API.Entities;

namespace FrontDesk.API.Entities;
public enum ReservationStatus
{
    Active = 1,
    CheckedOut = 2,
    Cancelled = 3
}
public class Reservation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid GuestId { get; set; }
    public Guest? Guest { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Active;
}