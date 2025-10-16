namespace CleanArchitecture.Domain.Common.Interfaces;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : BaseEntity
{
    Task<TResult?> FirstOrDefaultAsync<TResult>(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<List<TResult>> ListAsync<TResult>(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);

    Task<TResult[]> ArrayAsync<TResult>(ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = default);

    Task<TResult[]> ArrayAsync<TResult>(
        ISpecification<T> specification,
        CancellationToken cancellationToken = default);
}
