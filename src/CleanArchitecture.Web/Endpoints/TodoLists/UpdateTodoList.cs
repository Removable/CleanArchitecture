using CleanArchitecture.Application.Features.TodoLists.Commands.UpdateTodoList;

namespace CleanArchitecture.Web.Endpoints.TodoLists;

sealed class UpdateTodoListResponse
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
}

sealed class UpdateTodoListEndpoint(IMediator mediator) : Endpoint<UpdateTodoListCommand, UpdateTodoListResponse>
{
    public override void Configure()
    {
        Patch("{Id:guid}");
        AllowAnonymous();
        Version(1);
        Group<TodoListGroup>();
    }

    public override async Task HandleAsync(UpdateTodoListCommand r, CancellationToken c)
    {
        await mediator.Send(r, c).ConfigureAwait(false);
        Response = new UpdateTodoListResponse { Id = r.Id, Title = r.Title };
    }
}
