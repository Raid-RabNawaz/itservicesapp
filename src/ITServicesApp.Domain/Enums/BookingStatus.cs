namespace ITServicesApp.Domain.Enums
{
    public enum BookingStatus
    {
        PendingCustomerConfirmation = 0,
        PendingTechnicianConfirmation = 1,
        Confirmed = 2,
        OnTheWay = 3,
        InProgress = 4,
        Completed = 5,
        Cancelled = 6
    }
}
