using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Common.Interfaces;

namespace CleanArchitecture.Infrastructure.Data;

public sealed class EfReadRepository<T>(AppDbContext dbContext)
    : EfBaseRepository<T>(dbContext), IReadRepository<T> where T : BaseEntity;
