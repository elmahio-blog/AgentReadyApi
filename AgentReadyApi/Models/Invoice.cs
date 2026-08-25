namespace AgentReadyApi.Models;

public class Invoice
{
    public Guid Id { get; set; }

    public string CustomerId { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "USD";

    public InvoiceStatus Status { get; set; }

    public string ClientReference { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    
}