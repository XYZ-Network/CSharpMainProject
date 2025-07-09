using System;
using System.Collections.Generic;

namespace UnitBrains.Pathfinding
{
    // TODO: Replace with native `PriorityQueue` when Unity/.NET version is updated to .NET6+.
    //   This is just a drop-in replacement of native `PriorityQueue`, as initially A* for this task was
    //   implemented and tested in a separate project which uses in newer .NET.
    //   This `PriorityQueue` is highly non-performant, but it's just enough to complete the task.
    //   General recommendation is to update Unity and .NET version.
    public class PriorityQueue<T, P> where P : IComparable<P>
    {
        private List<(T item, P priority)> _items = new();

        public int Count => _items.Count;

        public void Enqueue(T item, P priority)
        {
            _items.Add((item, priority));
        }

        public T Dequeue()
        {
            if (_items.Count == 0)
                throw new InvalidOperationException("Queue is empty.");

            int bestIndex = 0;
            P bestPriority = _items[0].priority;

            for (int i = 1; i < _items.Count; i++)
            {
                if (_items[i].priority.CompareTo(bestPriority) < 0)
                {
                    bestPriority = _items[i].priority;
                    bestIndex = i;
                }
            }

            T bestItem = _items[bestIndex].item;
            _items.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}