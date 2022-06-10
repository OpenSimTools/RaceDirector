using Microsoft.Reactive.Testing;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace RaceDirector.Tests.Pipeline.Utils
{
    public static class TestableObserverExtensions
    {
        public static IEnumerable<T> ReceivedValues<T>(this ITestableObserver<T> testableObserver) =>
            testableObserver.Messages
                .Where(n => n.Value.Kind == NotificationKind.OnNext)
                .Select(n => n.Value.Value);
    }
}
