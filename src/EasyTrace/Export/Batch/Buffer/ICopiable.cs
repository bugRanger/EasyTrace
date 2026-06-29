namespace EasyTrace.Export.Batch.Buffer;

public interface ICopiable<in T>
{
    void CopyFrom(T source);
    void CopyTo(T destination);
}