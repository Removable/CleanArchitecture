using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;
using FastEndpoints;

namespace CleanArchitecture.Application.Features.TodoItems.Queries.GetTodoItemsWithPagination;

[Authorize]
public sealed record GetTodoItemsWithPaginationQuery : IRequest<PaginatedList<TodoItemBriefDto>>
{
    public Guid ListId { get; init; }
    [BindFrom("page")] public int PageNumber { get; init; } = 1;
    [BindFrom("size")] public int PageSize { get; init; } = 10;
}

public sealed class
    GetTodoItemsWithPaginationQueryHandler(IReadRepository<TodoItem> repository, IUser user)
    : IRequestHandler<GetTodoItemsWithPaginationQuery,
        PaginatedList<TodoItemBriefDto>>
{
    public async ValueTask<PaginatedList<TodoItemBriefDto>> Handle(GetTodoItemsWithPaginationQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new GetTodoItemsByListIdSpec(request.ListId, Guard.Against.NullOrEmpty(user.Id), request.PageNumber,
            request.PageSize);

        var result = await repository.ArrayAsync<TodoItemBriefDto>(spec, cancellationToken).ConfigureAwait(false);
        var totalCount = await repository.CountAsync(spec, cancellationToken).ConfigureAwait(false);

        return PaginatedList<TodoItemBriefDto>.Create(result, totalCount, request.PageNumber, request.PageSize);
    }
}
