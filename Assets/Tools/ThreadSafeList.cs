using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ThreadSafeList<T> : IList<T>, IList
{
    [SerializeField] List<T> items;
    object sync;

    public ThreadSafeList()
    {
        this.items = new List<T>();
        this.sync = new object();
    }

    public ThreadSafeList(object syncRoot)
    {
        if (syncRoot == null)
            throw new ArgumentNullException("syncRoot");

        this.items = new List<T>();
        this.sync = syncRoot;
    }

    public ThreadSafeList(object syncRoot, IEnumerable<T> list)
    {
        if (syncRoot == null)
            throw new ArgumentNullException("syncRoot");
        if (list == null)
            throw new ArgumentNullException("list");

        this.items = new List<T>(list);
        this.sync = syncRoot;
    }

    public ThreadSafeList(object syncRoot, params T[] list)
    {
        if (syncRoot == null)
            throw new ArgumentNullException("syncRoot");
        if (list == null)
            throw new ArgumentNullException("list");

        this.items = new List<T>(list.Length);
        for (int i = 0; i < list.Length; i++)
            this.items.Add(list[i]);

        this.sync = syncRoot;
    }

    public int Count
    {
        get
        {
            lock (this.sync)
            {
                return this.items.Count;
            }
        }
    }

    protected List<T> Items
    {
        get { return this.items; }
    }

    public object SyncRoot
    {
        get { return this.sync; }
    }

    public T this[int index]
    {
        get
        {
            lock (this.sync)
            {
                return this.items[index];
            }
        }
        set
        {
            lock (this.sync)
            {
                if (index < 0 || index >= this.items.Count)
                    throw new ArgumentOutOfRangeException("index");

                this.SetItem(index, value);
            }
        }
    }

    public void Add(T item)
    {
        lock (this.sync)
        {
            int index = this.items.Count;
            this.InsertItem(index, item);
        }
    }

    public void Add(IEnumerable<T> items)
    {
        lock (this.sync)
        {
            int index = this.items.Count;
            foreach (var item in items)
            {
                this.InsertItem(index, item);
            }
        }
    }


    public void Clear()
    {
        lock (this.sync)
        {
            this.ClearItems();
        }
    }

    public void CopyTo(T[] array, int index)
    {
        lock (this.sync)
        {
            this.items.CopyTo(array, index);
        }
    }

    public List<T> ReturnCopy()
    {
        lock (this.sync)
        {
            return new List<T>(items);
        }
    }

    public bool Contains(T item)
    {
        lock (this.sync)
        {
            return this.items.Contains(item);
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        lock (this.sync)
        {
            return this.items.GetEnumerator();
        }
    }

    public int IndexOf(T item)
    {
        lock (this.sync)
        {
            return this.InternalIndexOf(item);
        }
    }

    public void Insert(int index, T item)
    {
        lock (this.sync)
        {
            if (index < 0 || index > this.items.Count)
                throw new ArgumentOutOfRangeException("index");

            this.InsertItem(index, item);
        }
    }

    int InternalIndexOf(T item)
    {
        int count = items.Count;

        for (int i = 0; i < count; i++)
        {
            if (object.Equals(items[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    public bool Remove(T item)
    {
        lock (this.sync)
        {
            int index = this.InternalIndexOf(item);
            if (index < 0)
                return false;

            this.RemoveItem(index);
            return true;
        }
    }

    public bool Remove(IEnumerable<T> items)
    {
        lock (this.sync)
        {
            foreach (var item in items)
            {
                int index = this.InternalIndexOf(item);
                if (index < 0)
                    return false;

                this.RemoveItem(index);
            }

            return true;
        }
    }

    public void RemoveAt(int index)
    {
        lock (this.sync)
        {
            if (index < 0 || index >= this.items.Count)
                throw new ArgumentOutOfRangeException("index");


            this.RemoveItem(index);
        }
    }

    protected virtual void ClearItems()
    {
        this.items.Clear();
    }

    protected virtual void InsertItem(int index, T item)
    {
        this.items.Insert(index, item);
    }

    protected virtual void RemoveItem(int index)
    {
        this.items.RemoveAt(index);
    }

    protected virtual void SetItem(int index, T item)
    {
        this.items[index] = item;
    }

    bool ICollection<T>.IsReadOnly
    {
        get { return false; }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IList)this.items).GetEnumerator();
    }

    bool ICollection.IsSynchronized
    {
        get { return true; }
    }

    object ICollection.SyncRoot
    {
        get { return this.sync; }
    }

    void ICollection.CopyTo(Array array, int index)
    {
        lock (this.sync)
        {
            ((IList)this.items).CopyTo(array, index);
        }
    }

    object IList.this[int index]
    {
        get { return this[index]; }
        set
        {
            VerifyValueType(value);
            this[index] = (T)value;
        }
    }

    bool IList.IsReadOnly
    {
        get { return false; }
    }

    bool IList.IsFixedSize
    {
        get { return false; }
    }

    int IList.Add(object value)
    {
        VerifyValueType(value);

        lock (this.sync)
        {
            this.Add((T)value);
            return this.Count - 1;
        }
    }

    bool IList.Contains(object value)
    {
        VerifyValueType(value);
        return this.Contains((T)value);
    }

    int IList.IndexOf(object value)
    {
        VerifyValueType(value);
        return this.IndexOf((T)value);
    }

    void IList.Insert(int index, object value)
    {
        VerifyValueType(value);
        this.Insert(index, (T)value);
    }

    void IList.Remove(object value)
    {
        VerifyValueType(value);
        this.Remove((T)value);
    }

    static void VerifyValueType(object value)
    {
        if (value == null)
        {
            if (typeof(T).IsValueType)
            {
                throw new ArgumentException("is not value type");
            }
        }
        else if (!(value is T))
        {
            throw new ArgumentException(value.GetType().FullName);
        }
    }

    public T Find(Func<T, bool> func)
    {
        lock (this.sync)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (func.Invoke(items[i]))
                {
                    return items[i];
                }
            }
        }

        return default(T);
    }

    public int FindIndex(Func<T, bool> func)
    {
        lock (this.sync)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (func.Invoke(items[i]))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public static implicit operator List<T>(ThreadSafeList<T> tsl)
    {
        return new List<T>(tsl.GetEnumerator().Iterate());
    }
}