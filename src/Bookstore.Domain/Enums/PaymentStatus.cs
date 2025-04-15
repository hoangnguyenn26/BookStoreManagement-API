namespace Bookstore.Domain.Enums
{
    public enum PaymentStatus : byte
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3
    }
}
