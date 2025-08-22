using System.Collections.Concurrent;
using System.Collections.Generic;

public class ObjectPool<T> where T : GridNode, new()
{
    // коллекция для хранения объектов
    private readonly ConcurrentBag<T> pool = new();
    // список объектов
    private readonly List<T> inUse = new();

    /// <summary>
    /// Получить объекта из пула
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public T Get(int x, int y)
    {
        T item = pool.TryTake(out var result) ? result : new T();
        item.X = x;
        item.Y = y;
        item.Parent = null;
        inUse.Add(item);
        
        return item;
    }

    /// <summary>
    /// Очистка текущего списка и возвращаем объекты обратно в пул
    /// </summary>
    public void Clear()
    {
        foreach (var node in inUse)
        {
            pool.Add(node);
        }
        inUse.Clear();
    }
}