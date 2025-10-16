using CleanArchitecture.Domain.TodoListAggregate;
using CleanArchitecture.Domain.TodoListAggregate.Specifications;

namespace CleanArchitecture.Application.Features.TodoLists.Commands.CreateTodoList;

public sealed class CreateTodoListCommandValidator : AbstractValidator<CreateTodoListCommand>
{
    private readonly IRepository<TodoList> _repository;
    private readonly IUser _user;

    public CreateTodoListCommandValidator(IRepository<TodoList> repository, IUser user)
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

    private async Task<bool> BeUniqueTitle(CreateTodoListCommand model, string title,
        CancellationToken cancellationToken)
    {
        return !await _repository
            .AnyAsync(new TodoListTitleUniquenessCheckSpec(null, _user.Id!, title), cancellationToken);
    }
}
