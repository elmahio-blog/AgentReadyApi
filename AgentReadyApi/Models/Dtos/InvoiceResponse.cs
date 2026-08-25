namespace AgentReadyApi.Models.Dtos;

public class InvoiceResponse
{
    public Guid Id {get; set;}
    public string CustomerId {get; set;}
    public decimal Amount {get; set;}

    public string Currency {get; set;}
    public string Status {get; set;}

    public DateTime CreatedAt {get; set;}
    public string ClientReference { get; set; } = null!;
};