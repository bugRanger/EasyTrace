using System.Reflection;
using EasyTrace.Activity.Pool;
using EasyTrace.Export.Batch.Buffer;
using Microsoft.Extensions.AsyncState;

namespace EasyTrace.Activity.Context;

public class ContextAsyncState : ITraceActivityContext
{
    private readonly IAsyncState _currentState;
    private readonly IAsyncContext<TraceActivity> _currentContext;

    public TraceActivity? Current
    {
        get => _currentContext.TryGet(out var current) ? current : null;
        set
        {
            if (!_currentContext.TryGet(out _))
            {
                _currentState.Initialize();
            }

            if (value is null)
            {
                _currentState.Reset();
            }
            else
            {
                _currentContext.Set(value);
            }
        }
    }

    public ITraceActivityPool Pool { get; }

    public ContextAsyncState(IFactory<TraceActivity> factory)
    {
        Pool = new DefaultObjectPool(factory);
        (_currentState, _currentContext) = AssemblyCreate<TraceActivity>();
    }

    private static (IAsyncState State, IAsyncContext<T> Context) AssemblyCreate<T>() where T : class
    {
        var assembly = typeof(IAsyncState).Assembly;

        var asyncStateType = assembly.GetType("Microsoft.Extensions.AsyncState.AsyncState")!;
        var stateInstance = Activator.CreateInstance(asyncStateType, nonPublic: true)!;

        var asyncContextType = assembly.GetType("Microsoft.Extensions.AsyncState.AsyncContext`1")!
            .MakeGenericType(typeof(T));

        var contextInstance = Activator.CreateInstance(
            asyncContextType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: [stateInstance],
            culture: null)!;

        return ((IAsyncState)stateInstance, (IAsyncContext<T>)contextInstance);
    }
}