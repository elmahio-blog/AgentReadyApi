using System.ComponentModel.DataAnnotations;

namespace AgentReadyApi.Models;

public class IdempotencyRecord
{
    [Key]
    public Guid Id { get; set; }

    public string Key { get; set; } = null!;

    public string RequestHash { get; set; } = null!;

    public int StatusCode { get; set; }

    public string ResponseBody { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}