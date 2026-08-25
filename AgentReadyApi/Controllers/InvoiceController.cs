using AgentReadyApi.Data.Services;
using AgentReadyApi.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgentReadyApi.Controllers;
[ApiController]
[Route("api/invoices")]
public class InvoiceController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoiceController(IInvoiceService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lists invoices for the authenticated merchant.
    /// </summary>
    //[Authorize(Policy = "InvoicesRead")]
    [HttpGet("GetInvoices")]
    public async Task<IActionResult> GetInvoicesAsync(
        [FromQuery] string? status,
        [FromQuery] int limit = 20,
        [FromQuery] string? cursor = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetInvoicesAsync(
            status,
            limit,
            cursor,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a draft invoice.
    /// </summary>
    [EnableRateLimiting("agent-policy")]
    //[Authorize(Policy = "InvoicesWrite")]
    [HttpPost("CreateInvoice")]
    public async Task<IActionResult> CreateInvoiceAsync(
        [FromHeader(Name = "Idempotency-Key")]
        string idempotencyKey,
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(
            request,
            idempotencyKey,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetInvoice),
            new { id = result.Id },
            result);
    }

    /// <summary>
    /// Gets an invoice by ID.
    /// </summary>
    [Authorize(Policy = "InvoicesRead")]
    [HttpGet("GetInvoice{id:guid}")]
    public async Task<IActionResult> GetInvoiceAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(invoice);
    }

    /// <summary>
    /// Finalizes a draft invoice.
    /// </summary>
    [Authorize(Policy = "InvoicesWrite")]
    [HttpPost("Finalize{id:guid}")]
    public async Task<IActionResult> FinalizeAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _service.FinalizeAsync(
            id,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Creates a cancellation intent.
    /// </summary>
    [Authorize(Policy = "InvoicesWrite")]
    [HttpPost("CreateCancellationIntent{id:guid}")]
    public async Task<IActionResult> CreateCancellationIntentAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var intent = await _service.CreateCancellationIntentAsync(
            id,
            cancellationToken);

        return Ok(intent);
    }

    /// <summary>
    /// Cancels an invoice using a previously created intent.
    /// </summary>[Authorize(Policy = "InvoicesWrite")]
    [HttpPost("Cancel")]
    [HttpPost("Cancel")]
    public async Task<IActionResult> CancelAsync(
        [FromBody] CancelInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CancelAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}