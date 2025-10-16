using CleanArchitecture.Application.Features.TodoLists.Commands.CreateTodoList;

namespace CleanArchitecture.Web.Endpoints.TodoLists;

sealed class CreateTodoListResponse
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
}

sealed class CreateTodoListEndpoint(IMediator mediator) : Endpoint<CreateTodoListCommand, CreateTodoListResponse>
{
    public override void Configure()
    {
        AllowAnonymous();
        Version(1);
        Post("");
        Group<TodoListGroup>();
    }

    public override async Task HandleAsync(CreateTodoListCommand r, CancellationToken c)
    {
        var id = await mediator.Send(r, c);
        var res = new CreateTodoListResponse { Id = id, Title = r.Title };
        await Send.CreatedAtAsync<CreateTodoListEndpoint>(new { id }, res, cancellation: c);
    }
}
