using System.ComponentModel.DataAnnotations;

namespace AgentReadyApi.Models.Dtos;

public class CreateInvoiceRequest
{
    /// <summary>
    /// Customer that owns the invoice.
    /// </summary>
    [Required]
    public string CustomerId { get; init; } = null!;

    /// <summary>
    /// Invoice amount. Must be greater than zero.
    /// </summary>
    [Range(0.01, 999999999)]
    public decimal Amount { get; init; }

    /// <summary>
    /// Three-letter ISO currency code.
    /// </summary>
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Unique client-side reference used for reconciliation.
    /// </summary>
    [Required]
    public string ClientReference { get; init; } = null!;
    
}