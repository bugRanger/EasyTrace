using System.Reflection;
using Microsoft.Extensions.AsyncState;

namespace EasyTrace.Activity;

public class TraceActivityParentAsync : ITraceActivityParent
{
    private readonly IAsyncState _state;
    private readonly IAsyncContext<TraceActivity> _context;

    public TraceActivity? Activity
    {
        get => _context.TryGet(out var current) ? current : null;
        set
        {
            if (!_context.TryGet(out _))
            {
                _state.Initialize();
            }

            if (value is null)
            {
                _state.Reset();
            }
            else
            {
                _context.Set(value);
            }
        }
    }

    public TraceActivityParentAsync()
    {
        (_state, _context) = AsyncStateFactory.Create<TraceActivity>();
    }
}

public static class AsyncStateFactory
{
    public static (IAsyncState State, IAsyncContext<T> Context) Create<T>() where T : class
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