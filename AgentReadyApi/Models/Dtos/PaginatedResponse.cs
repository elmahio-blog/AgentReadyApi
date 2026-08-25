namespace AgentReadyApi.Models.Dtos;
public record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    string? NextCursor
);