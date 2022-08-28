using Microsoft.Reactive.Testing;
using System.Reactive;

namespace TestUtils;

public static class TestableObserverExtensions
{
    public static IEnumerable<T> ReceivedValues<T>(this ITestableObserver<T> testableObserver) =>
        testableObserver.Messages
            .Where(n => n.Value.Kind == NotificationKind.OnNext)
            .Select(n => n.Value.Value);
}