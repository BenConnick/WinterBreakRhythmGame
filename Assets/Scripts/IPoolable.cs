using System;
public interface IPoolable<D>
{
    public bool CanUseFor(D data);
    public void Free();
}

public interface IPool<T,D> where T : IPoolable<D>
{
    public T Get(D creationArg);

    public void Free(T toFree);

    public void FreeAll();
}

public interface IPoolProvider<T,D> where T : IPoolable<D>
{
    public IPool<T,D> GetPool();
}
