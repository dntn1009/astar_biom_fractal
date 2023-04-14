using System;
using System.Collections.Generic;
using UnityEngine;

public interface IHeapItem<T> : IComparable<T>
{
    int _heapIndex
    {
        get;set;
    }

}


public class Heap<T> where T : IHeapItem<T>
{
    T[] _items;
    int _currentItemCount;

    public int _count
    {
        get { return _currentItemCount; }
    }

    public Heap(int maxHeapSize)
    {
        _items = new T[maxHeapSize];
    }

    void Swap(T itemA, T itemB)
    {
        _items[itemA._heapIndex] = itemB;
        _items[itemB._heapIndex] = itemA;
        int itemAIndex = itemA._heapIndex;
        itemA._heapIndex = itemB._heapIndex;
        itemB._heapIndex = itemAIndex;
    }

    void SortUp(T item)
    {
        
        while(true)
        {
            int parentIndex = (item._heapIndex - 1) / 2;
            T parentItem = _items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
                Swap(item, parentItem);
            else
                break;
            
        }
    }

    void SortDown(T item)
    {
        while(true)
        {
            int childIndexLeft = item._heapIndex * 2 + 1;
            int childIndexRight = item._heapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < _currentItemCount)
            {
                swapIndex = childIndexLeft;
                if(childIndexRight < _currentItemCount)
                {
                    if (_items[childIndexLeft].CompareTo(_items[childIndexRight]) < 0)
                        swapIndex = childIndexRight;
                }
                if (item.CompareTo(_items[swapIndex]) < 0)
                    Swap(item, _items[swapIndex]);
                else
                    return;
            }
            else
                return;
        }
    }

    public void Add(T item)
    {
        item._heapIndex = _currentItemCount;
        _items[_currentItemCount] = item;
        SortUp(item);
        _currentItemCount++;
    }

    public T RemoveFirst()
    {
        T firstItem = _items[0];
        _currentItemCount--;
        _items[0] = _items[_currentItemCount];
        _items[0]._heapIndex = 0;
        SortDown(_items[0]);

        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public bool Contains(T item)
    {
        return Equals(_items[item._heapIndex], item);
    }
}
