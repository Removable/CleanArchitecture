using CleanArchitecture.Application.Features.TodoLists.Queries.GetTodos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CleanArchitecture.Web.Endpoints.TodoLists;

public sealed class GetTodoLists(IMediator mediator)
    : EndpointWithoutRequest<Results<Ok<TodosVm>, UnauthorizedHttpResult>>
{
    public override void Configure()
    {
        AllowAnonymous();
        Version(1);
        Get("/");
        Group<TodoListGroup>();
    }

    public override async Task<Results<Ok<TodosVm>, UnauthorizedHttpResult>> ExecuteAsync(CancellationToken ct)
    {
        var vm = await mediator.Send(new GetTodosQuery(), ct).ConfigureAwait(false);
        return TypedResults.Ok(vm);
    }
}
