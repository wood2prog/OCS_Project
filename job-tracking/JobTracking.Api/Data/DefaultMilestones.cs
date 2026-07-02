namespace JobTracking.Api.Data;

public static class DefaultMilestones
{
    public static readonly (int Order, string Label)[] All =
    [
        (1, "Designed"),
        (2, "Sent for approval"),
        (3, "Approved to build"),
        (4, "Production started"),
        (5, "Components machined and assembled"),
        (6, "Components finished"),
        (7, "Final assembly done"),
        (8, "Loaded"),
        (9, "Delivered"),
        (10, "Billed"),
        (11, "Paid"),
        (12, "Closed"),
    ];
}
