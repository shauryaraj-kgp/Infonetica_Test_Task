using System.Collections.Generic;
using System.Linq;

namespace WorkflowEngine.Storage
{
    public class InMemoryRepository<T> : IRepository<T> where T : class {
        private readonly Dictionary<string, T> _store = new();

        public void Add(T item) {
            var id = (string)item!.GetType().GetProperty("Id")!.GetValue(item)!;
            _store[id] = item;
        }

        public T? Get(string id) => _store.TryGetValue(id, out var item) ? item : null;

        public List<T> GetAll() => _store.Values.ToList();
    }
} 