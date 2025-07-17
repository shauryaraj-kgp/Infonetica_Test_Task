namespace WorkflowEngine.Storage
{
    public interface IRepository<T> {
        void Add(T item);
        T? Get(string id);
        List<T> GetAll();
    }
} 