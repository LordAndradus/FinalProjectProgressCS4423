using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

public class ObservableStack<T> : Stack<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    T type;

    public event NotifyCollectionChangedEventHandler CollectionChanged;
    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableStack() {}

    public ObservableStack(IEnumerable<T> collection)
    {
        foreach (var item in collection) base.Push(item);
    }

    public ObservableStack(List<T> list)
    {
        foreach (var item in list) base.Push(item);
    }

    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        this.RaiseCollectionsChanged(e);
    }

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs  e)
    {
        this.RaisePropertyChanged(e);
    }

    private void RaiseCollectionsChanged(NotifyCollectionChangedEventArgs e)
    {
        if(this.CollectionChanged != null) this.CollectionChanged(this, e);
    }

    private void RaisePropertyChanged(PropertyChangedEventArgs  e)
    {
        if(this.PropertyChanged != null) this.PropertyChanged(this, e);
    }

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add { this.PropertyChanged += value; }
        remove { this.PropertyChanged -= value; }
    }

    public new virtual void Clear()
    {
        base.Clear();
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    new public virtual void Push(T item)
    {
        base.Push(item);
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    new public virtual T Pop()
    {
        var item = base.Pop();
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        return item;
    }
}