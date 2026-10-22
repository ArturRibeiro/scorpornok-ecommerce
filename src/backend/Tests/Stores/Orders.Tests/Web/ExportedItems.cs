namespace Orders.Tests.Web;

// Destino dos exportadores em memória: o exportador acrescenta de outra thread (lote) enquanto o teste lê.
public sealed class ExportedItems<T> : ICollection<T>
{
    private readonly List<T> _items = [];

    public int Count { get { lock (_items) return _items.Count; } }
    public bool IsReadOnly => false;

    public void Add(T item) { lock (_items) _items.Add(item); }
    public void Clear() { lock (_items) _items.Clear(); }
    public bool Contains(T item) { lock (_items) return _items.Contains(item); }
    public void CopyTo(T[] array, int arrayIndex) { lock (_items) _items.CopyTo(array, arrayIndex); }
    public bool Remove(T item) { lock (_items) return _items.Remove(item); }

    // Enumera uma cópia.
    public IEnumerator<T> GetEnumerator() { lock (_items) return _items.ToList().GetEnumerator(); }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
