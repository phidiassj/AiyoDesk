using System.Collections.Generic;

namespace AiyoDesk.Models;

public class FixedSizeQueue<T>
{
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly int _maxSize;

    public FixedSizeQueue(int maxSize)
    {
        _maxSize = maxSize;
    }

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        while (_queue.Count > _maxSize)
        {
            _queue.Dequeue(); // 移除最舊的
        }
    }

    public int Count => _queue.Count;

    public IEnumerable<T> Items => _queue;

    public T Peek() => _queue.Peek();

    public bool TryDequeue(out T item) => _queue.TryDequeue(out item!);
}

