using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Features.TodoItems.Queries.GetTodoItemsWithPagination;
using CleanArchitecture.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints.TodoItems;

sealed class GetTodoItemsWithPaginationEndpoint(IMediator mediator)
    : Endpoint<GetTodoItemsWithPaginationQuery, Results<Ok<PaginatedList<TodoItemBriefDto>>, NotFound, ProblemDetails>>
{
    public override void Configure()
    {
        Get("");
        AllowAnonymous();
        Version(1);
        Group<TodoItemGroup>();
    }

    public override async Task HandleAsync(GetTodoItemsWithPaginationQuery r, CancellationToken c)
    {
        var vm = await mediator.Send(r, c).ConfigureAwait(false);
        await Send.ResultAsync(TypedResults.Ok(vm)).ConfigureAwait(false);
    }
}
