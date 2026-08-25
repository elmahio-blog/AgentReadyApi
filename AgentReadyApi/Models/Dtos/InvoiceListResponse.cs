namespace AgentReadyApi.Models.Dtos;
public sealed class InvoiceListResponse
{
    public IReadOnlyList<InvoiceResponse> Items { get; init; }
        = [];

    public string? NextCursor { get; init; }
}