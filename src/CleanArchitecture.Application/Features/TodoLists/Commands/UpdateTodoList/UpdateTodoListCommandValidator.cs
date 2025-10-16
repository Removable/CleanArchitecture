using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.UpdateTodoList;

public sealed class UpdateTodoListCommandValidator : AbstractValidator<UpdateTodoListCommand>
{
    private readonly IRepository<TodoList> _repository;
    private readonly IUser _user;

    public UpdateTodoListCommandValidator(IRepository<TodoList> repository, IUser user)
    {
        _repository = repository;
        _user = user;

        RuleFor(v => v.Title)
            .NotEmpty()
            .MaximumLength(200)
            .MustAsync(BeUniqueTitle)
            .WithMessage("'{PropertyName}' must be unique.")
            .WithErrorCode("Unique");
    }

    private async Task<bool> BeUniqueTitle(UpdateTodoListCommand model, string title,
        CancellationToken cancellationToken)
    {
        return !await _repository
            .AnyAsync(new TodoListTitleUniquenessCheckSpec(model.Id, _user.Id!, title), cancellationToken);
    }
}
