namespace AgentReadyApi.Models.Dtos;

public class CancelInvoiceRequest
{
    /// <summary>
    /// The unique identifier of the cancellation intent previously created for the invoice.
    /// </summary>
    public Guid IntentId { get; init; }

    /// <summary>
    /// The unique identifier of the invoice to cancel.
    /// </summary>
    public Guid InvoiceId { get; init; }

    /// <summary>
    /// The date and time when the cancellation intent expires.
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}