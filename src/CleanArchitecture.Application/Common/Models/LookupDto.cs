namespace CleanArchitecture.Application.Common.Models;

public sealed record LookupDto
{
    public int Id { get; init; }

    public string? Title { get; init; }
}
