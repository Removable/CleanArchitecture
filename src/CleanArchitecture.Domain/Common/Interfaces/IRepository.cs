namespace CleanArchitecture.Domain.Common.Interfaces;

public interface IRepository<T> : IRepositoryBase<T> where T : BaseEntity;
