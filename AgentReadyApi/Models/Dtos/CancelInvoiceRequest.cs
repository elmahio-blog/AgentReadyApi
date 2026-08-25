namespace AgentReadyApi.Models.Dtos;

public class CancelInvoiceRequest
{
    public Guid IntentId { get; init; }

    public Guid InvoiceId { get; init; }

    public DateTime ExpiresAt { get; init; }
}