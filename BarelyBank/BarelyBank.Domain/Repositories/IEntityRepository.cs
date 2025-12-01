public interface IEntityRepository<T> where T : class
{
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<T?> GetAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
}
