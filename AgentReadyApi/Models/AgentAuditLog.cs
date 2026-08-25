namespace AgentReadyApi.Models;

public class AgentAuditLog
{
    
    public Guid Id { get; set; }

    public string? AgentId { get; set; }

    public string? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string HttpMethod { get; set; } = null!;

    public string Path { get; set; } = null!;

    public int StatusCode { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime CreatedAt { get; set; }
}