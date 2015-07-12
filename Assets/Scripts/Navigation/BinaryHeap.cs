using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BinaryHeap<T> : ICollection<T> where T : IComparable<T>
{
    private const int DEFAULT_SIZE = 4;
    private T[] _data = new T[DEFAULT_SIZE];
    private int _count = 0;
    private int _capacity = DEFAULT_SIZE;
    private bool _sorted;

    public int Count
    {
        get { return _count; }
    }
    public int Capacity
    {
        get { return _capacity; }
        set
        {
            int previouseCapacity = _capacity;
            _capacity = Math.Max(value, _count);
            if (_capacity != previouseCapacity)
            {
                T[] temp = new T[_capacity];
                Array.Copy(_data, temp, _count);
                _data = temp;
            }
        }
    }

    public BinaryHeap()
    { 
    }
    private BinaryHeap(T[] data, int count)
    {
        Capacity = count;
        _count = count;
        Array.Copy(data, _data, count);
    }
    public T Peek()
    {
        return _data[0];
    }
    public void Clear()
    {
        this._count = 0;
        _data = new T[_capacity];
    }
    public void Add(T item)
    {
        if (_count == _capacity)
        {
            Capacity *= 2;
        }
        _data[_count] = item;
        UpHeap();
        _count++;
    }
    public T Remove()
    {
        if (this._count == 0)
        {
            throw new InvalidOperationException("Cannot remove item, heap is empty.");
        }
        T v = _data[0];
        _count--;
        _data[0] = _data[_count];
        _data[_count] = default(T);
        DownHeap();
        return v;
    }
    private void UpHeap()
    {
        _sorted = false;
        int p = _count;
        T item = _data[p];
        int par = Parent(p);
        while (par > -1 && item.CompareTo(_data[par]) < 0)
        {
            //Swap
            _data[p] = _data[par];
            p = par;
            par = Parent(p);
        }
        _data[p] = item;
    }
    private void DownHeap()
    {
        _sorted = false;
        int n;
        int p = 0;
        T item = _data[p];
        while (true)
        {
            int ch1 = Child1(p);
            if (ch1 >= _count) break;
            int ch2 = Child2(p);
            if (ch2 >= _count)
                n = ch1;
            else
                n = _data[ch1].CompareTo(_data[ch2]) < 0 ? ch1 : ch2;
            if (item.CompareTo(_data[n]) > 0)
            {
                //Swap
                _data[p] = _data[n];
                p = n;
            }
            else
                break;
        }
        _data[p] = item;
    }
    private void EnsureSort()
    {
        if (_sorted) return;
        Array.Sort(_data, 0, _count);
        _sorted = true;
    }
    private static int Parent(int index)
    {
        return (index - 1) >> 1;
    }
    private static int Child1(int index)
    {
        return (index << 1) + 1;
    }
    private static int Child2(int index)
    {
        return (index << 1) + 2;
    }
    public BinaryHeap<T> Copy()
    {
        return new BinaryHeap<T>(_data, _count);
    }
    public IEnumerator<T> GetEnumerator()
    {
        EnsureSort();
        for (int i = 0; i < _count; i++)
        {
            yield return _data[i];
        }
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    public bool Contains(T item)
    {
        EnsureSort();
        return Array.BinarySearch<T>(_data, 0, _count, item) >= 0;
    }
    public void CopyTo(T[] array, int arrayIndex)
    {
        EnsureSort();
        Array.Copy(_data, array, _count);
    }
    public bool IsReadOnly
    {
        get { return false; }
    }
    public bool Remove(T item)
    {
        EnsureSort();
        int i = Array.BinarySearch<T>(_data, 0, _count, item);
        if (i < 0) return false;
        Array.Copy(_data, i + 1, _data, i, _count - i);
        _data[_count] = default(T);
        _count--;
        return true;
    }
}
