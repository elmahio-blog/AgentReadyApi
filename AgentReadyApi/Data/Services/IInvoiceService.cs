using AgentReadyApi.Models.Dtos;

namespace AgentReadyApi.Data.Services;
public interface IInvoiceService
{
    Task<InvoiceListResponse> GetInvoicesAsync(
        string? status,
        int limit,
        string? cursor,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> FinalizeAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<CancelInvoiceRequest> CreateCancellationIntentAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<InvoiceResponse> CancelAsync(
        CancelInvoiceRequest input,
        CancellationToken cancellationToken);
}