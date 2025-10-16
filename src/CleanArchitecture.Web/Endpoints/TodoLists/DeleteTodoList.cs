using CleanArchitecture.Application.Features.TodoLists.Commands.DeleteTodoList;

namespace CleanArchitecture.Web.Endpoints.TodoLists;

sealed class DeleteTodoListEndpoint(IMediator mediator) : Endpoint<DeleteTodoListCommand>
{
    public override void Configure()
    {
        Delete("{Id:guid}");
        AllowAnonymous();
        Group<TodoListGroup>();
        Version(1);
    }

    public override async Task HandleAsync(DeleteTodoListCommand r, CancellationToken c)
    {
        await mediator.Send(r, c).ConfigureAwait(false);
        await Send.NoContentAsync(c).ConfigureAwait(false);
    }
}
