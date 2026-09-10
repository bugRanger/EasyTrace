namespace EasyTrace.Export.Batch.Buffer;

public interface IFactory<out T>
{
    T Create();
}