using AgentReadyApi.Exceptions;
using AgentReadyApi.Models;
using AgentReadyApi.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AgentReadyApi.Data.Services;
public sealed class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;

    public InvoiceService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<InvoiceListResponse> GetInvoicesAsync(
        string? status,
        int limit,
        string? cursor,
        CancellationToken cancellationToken)
    {
        if (limit <= 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "INVALID_LIMIT",
                "Limit must be greater than zero.");
        }

        if (limit > 100)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "LIMIT_TOO_LARGE",
                "Limit cannot be greater than 100.");
        }

        var query = _db.Invoices
            .AsNoTracking()
            .AsQueryable();

        // Optional status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<InvoiceStatus>(
                    status,
                    ignoreCase: true,
                    out var invoiceStatus))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "INVALID_STATUS",
                    $"Invalid invoice status '{status}'.");
            }

            query = query.Where(x => x.Status == invoiceStatus);
        }

        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            if (!Guid.TryParse(cursor, out var cursorId))
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "INVALID_CURSOR",
                    "The supplied cursor is invalid.");
            }

            query = query.Where(x => x.Id > cursorId);
        }

        var invoices = await query
            .OrderBy(x => x.Id)
            .Take(limit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = invoices.Count > limit;

        if (hasMore)
        {
            invoices = invoices.Take(limit).ToList();
        }

        var items = invoices
            .Select(MapToResponse)
            .ToList();

        string? nextCursor = null;

        if (hasMore && invoices.Count > 0)
        {
            nextCursor = invoices[^1].Id.ToString();
        }

        return new InvoiceListResponse
        {
            Items = items,
            NextCursor = nextCursor
        };
    }

    public async Task<InvoiceResponse> CreateAsync(
        CreateInvoiceRequest request,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "IDEMPOTENCY_KEY_REQUIRED",
                "Idempotency-Key header is required.");
        }

        var existing = await _db.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ClientReference == request.ClientReference,
                cancellationToken);

        if (existing is not null)
        {
            return MapToResponse(existing);
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Amount = request.Amount,
            Currency = request.Currency.ToUpperInvariant(),
            ClientReference = request.ClientReference,
            Status = InvoiceStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        _db.Invoices.Add(invoice);

        await _db.SaveChangesAsync(cancellationToken);

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (invoice is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "INVOICE_NOT_FOUND",
                "The requested invoice was not found.");
        }

        return MapToResponse(invoice);
    }

    public async Task<InvoiceResponse> FinalizeAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (invoice is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "INVOICE_NOT_FOUND",
                "The requested invoice was not found.");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "INVALID_INVOICE_STATE",
                "Only draft invoices can be finalized.");
        }

        invoice.Status = InvoiceStatus.Finalized;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return MapToResponse(invoice);
    }

    public async Task<CancelInvoiceRequest>
        CreateCancellationIntentAsync(
            Guid id,
            CancellationToken cancellationToken)
    {
        var invoice = await _db.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (invoice is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "INVOICE_NOT_FOUND",
                "The requested invoice was not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "INVOICE_ALREADY_PAID",
                "A paid invoice cannot be cancelled.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "INVOICE_ALREADY_CANCELLED",
                "The invoice is already cancelled.");
        }

        return new CancelInvoiceRequest
        {
            IntentId = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
    }

    public async Task<InvoiceResponse> CancelAsync(
        CancelInvoiceRequest input,
        CancellationToken cancellationToken)
    {
        if (input.IntentId == Guid.Empty)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "INVALID_INTENT",
                "A valid cancellation intent is required.");
        }

        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(
                x => x.Id == input.InvoiceId,
                cancellationToken);

        if (invoice is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "INVOICE_NOT_FOUND",
                "The requested invoice was not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "INVOICE_ALREADY_PAID",
                "A paid invoice cannot be cancelled.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "INVOICE_ALREADY_CANCELLED",
                "The invoice is already cancelled.");
        }

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return MapToResponse(invoice);
    }

    private static InvoiceResponse MapToResponse(
        Invoice invoice)
    {
        return new InvoiceResponse
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            Amount = invoice.Amount,
            Currency = invoice.Currency,
            Status = invoice.Status.ToString().ToLowerInvariant(),
            ClientReference = invoice.ClientReference,
            CreatedAt = invoice.CreatedAt
        };
    }
}