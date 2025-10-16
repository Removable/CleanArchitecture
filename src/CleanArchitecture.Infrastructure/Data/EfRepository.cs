using CleanArchitecture.Domain.Common;
using CleanArchitecture.Domain.Common.Interfaces;

namespace CleanArchitecture.Infrastructure.Data;

public sealed class EfRepository<T>(AppDbContext dbContext) :
    EfBaseRepository<T>(dbContext), IRepository<T>, IReadRepository<T> where T : BaseEntity, IAggregateRoot;
