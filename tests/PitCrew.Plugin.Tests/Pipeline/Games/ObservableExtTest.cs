using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using RaceDirector.PitCrew.Pipeline.Games;
using Xunit;
using Xunit.Categories;

namespace PitCrew.Plugin.Tests.Pipeline.Games;

[UnitTest]
public class ObservableExtTest : ReactiveTest
{
    [Fact]
    public void CompareWithFirstEmpty()
    {
        _testScheduler.Start(() => Observable.Empty<int>().CompareWithFirst((_, _) => true))
            .Messages.AssertEqual(OnCompleted<bool>(Subscribed));
    }

    [Fact]
    public void CompareWithFirstOneElement()
    {
        _testScheduler.Start(() =>
            Observable.Return(2).CompareWithFirst((_, _) => true)
        ).Messages.AssertEqual(
            OnNext(Subscribed, true), 
            OnCompleted<bool>(Subscribed)
        );
    }

    [Fact]
    public void CompareWithFirstMultipleElements()
    {
        var observable = _testScheduler.CreateColdObservable(
            OnNext(1, 12),
            OnNext(2, 13),
            OnNext(3, 11),
            OnCompleted<int>(4)
        );
        _testScheduler.Start(() =>
            observable.CompareWithFirst((ti, t0) => ti > t0)
        ).Messages.AssertEqual(
            // +1 because it subscribes again and it's a cold observable
            OnNext(Subscribed + 2, false),
            OnNext(Subscribed + 3, true),
            OnNext(Subscribed + 4, false),
            OnCompleted<bool>(Subscribed + 5)
        );
    }

    [Fact]
    public void WaitTrueEmpty()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnCompleted<bool>(300)
        );
        _testScheduler.Start(() =>
            observable.WaitTrue(TimeSpan.FromTicks(500), _testScheduler)
        ).Messages.AssertEqual(
            OnCompleted<bool>(300) // ???
        );
    }
    
    [Fact]
    public void WaitTrueNotSatisfiedWithinTimeout()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(300, false),
            OnNext(1000, true),
            OnCompleted<bool>(1001)
        );
        _testScheduler.Start(() =>
            observable.WaitTrue(TimeSpan.FromTicks(500), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(700, false),
            OnCompleted<bool>(700)
        );
    }

    [Fact]
    public void WaitTrueSatisfiedWithinTimeout()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(301, false),
            OnNext(302, false),
            OnNext(303, true),
            OnNext(304, true),
            OnNext(305, false),
            OnCompleted<bool>(306)
        );
        _testScheduler.Start(() =>
            observable.WaitTrue(TimeSpan.FromTicks(500), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(303, true),
            OnCompleted<bool>(303)
        );
    }

    #region Test Setup

    private readonly TestScheduler _testScheduler = new();
    
    #endregion
}