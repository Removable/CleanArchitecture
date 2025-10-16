using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using CleanArchitecture.Domain.Common;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Data;

public class EfBaseRepository<T>(AppDbContext dbContext) : RepositoryBase<T>(dbContext) where T : BaseEntity
{
    public async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ProjectToType<TResult>().FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<TResult>> ListAsync<TResult>(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ProjectToType<TResult>()
            .ToListAsync(cancellationToken);
    }

    public async Task<TResult[]> ArrayAsync<TResult>(ISpecification<T, TResult> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TResult[]> ArrayAsync<TResult>(ISpecification<T> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ProjectToType<TResult>().ToArrayAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
