using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Reactive.Tests;

[UnitTest]
public class ObservableExtensionsTest : ReactiveTest
{
    #region SelectManyUntilNext

    [Fact]
    public void SelectManyUntilNextStopEmittingWhenSourceEmits()
    {
        var scheduler = new TestScheduler();
        const long sourceStart = Subscribed + 5;
        const long sourceEmitOffset = 21;
        const long sourceCompleteOffset = 100;
        const long selectorCompleteOffset = 50;

        var source = scheduler.CreateHotObservable(
            OnNext(sourceStart, 0),
            OnNext(sourceStart + sourceEmitOffset, 1),
            OnCompleted<int>(sourceStart + sourceCompleteOffset)
        );

        var res = scheduler.Start(() =>
            source.SelectManyUntilNext(i => scheduler.CreateColdObservable(
                OnNext(10, 10*i+1),
                OnNext(20, 10*i+2),
                OnNext(30, 10*i+3),
                OnNext(40, 10*i+4),
                OnCompleted<int>(selectorCompleteOffset)
            ))
        );
            
        source.Subscriptions.AssertEqual(
            // Test
            Subscribe(Subscribed, sourceStart + sourceCompleteOffset),
            // SelectManyUntilNext
            Subscribe(sourceStart, sourceStart + sourceEmitOffset),
            Subscribe(sourceStart + sourceEmitOffset, sourceStart + sourceEmitOffset + selectorCompleteOffset)
        );
            
        res.Messages.AssertEqual(
            OnNext(sourceStart + 10, 1),
            OnNext(sourceStart + 20, 2),
            OnNext(sourceStart + sourceEmitOffset + 10, 11),
            OnNext(sourceStart + sourceEmitOffset + 20, 12),
            OnNext(sourceStart + sourceEmitOffset + 30, 13),
            OnNext(sourceStart + sourceEmitOffset + 40, 14),
            OnCompleted<int>(sourceStart + sourceCompleteOffset)
        );
    }
        
    [Fact]
    public void SelectManyUntilNextKeepsEmittingWhenSourceCompletes()
    {
        var scheduler = new TestScheduler();
        const long sourceStart = Subscribed + 5;
        const long sourceCompleteOffset = 25;
        const long selectorCompleteOffset = 50;

        var source = scheduler.CreateHotObservable(
            OnNext(sourceStart, 0),
            OnCompleted<int>(sourceStart + sourceCompleteOffset)
        );

        var res = scheduler.Start(() =>
            source.SelectManyUntilNext(_ => scheduler.CreateColdObservable(
                OnNext(10, 1),
                OnNext(20, 2),
                OnNext(30, 3),
                OnCompleted<int>(selectorCompleteOffset)
            ))
        );
            
        source.Subscriptions.AssertEqual(
            // Test
            Subscribe(Subscribed, sourceStart + sourceCompleteOffset),
            // SelectManyUntilNext
            Subscribe(sourceStart, sourceStart + sourceCompleteOffset)
        );
            
        res.Messages.AssertEqual(
            OnNext(sourceStart + 10, 1),
            OnNext(sourceStart + 20, 2),
            OnNext(sourceStart + 30, 3),
            OnCompleted<int>(sourceStart + selectorCompleteOffset)
        );
    }
    
    #endregion

    #region SpacedBy

    [Fact]
    public void SpacedByDoesNothingIfValueAreAlreadySpaced()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 2),
            OnNext(Subscribed + 30, 3),
            OnCompleted<int>(Subscribed + 40)
        );
    
        _testScheduler.Start(() =>
            observable.SpacedBy(TimeSpan.FromTicks(5), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 2),
            OnNext(Subscribed + 30, 3),
            OnCompleted<int>(Subscribed + 40)
        );
    }

    [Fact]
    public void SpacedByDelaysValuesIfComingTooFast()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 15, 2),
            OnNext(Subscribed + 20, 3),
            OnCompleted<int>(Subscribed + 25)
        );
    
        _testScheduler.Start(() =>
            observable.SpacedBy(TimeSpan.FromTicks(10), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 2),
            OnNext(Subscribed + 30, 3),
            OnCompleted<int>(Subscribed + 40)
        );
    }
    
    #endregion

    #region IfNoChange
    
    [Fact]
    public void IfNoChangeWhenNoChange()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 1),
            OnCompleted<int>(Subscribed + 30)
        );
    
        _testScheduler.Start(() =>
            observable.IfNoChange(Observable.Return("a"), LongTimeout, _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 30, "a"),
            OnCompleted<string>(Subscribed + 30)
        );
    }
    
    [Fact]
    public void IfNoChangeWhenChanges()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 2),
            OnCompleted<int>(Subscribed + 30)
        );
    
        _testScheduler.Start(() =>
            observable.IfNoChange(Observable.Return("a"), LongTimeout, _testScheduler)
        ).Messages.AssertEqual(
            OnCompleted<string>(Subscribed + 20)
        );
    }
    
    #endregion
    
    #region WaitForFieldChange

    [Fact]
    public void WaitForChangeNotEnoughElements()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnCompleted<int>(Subscribed + 50)
        );

        _testScheduler.Start(() =>
            observable.WaitForChange(LongTimeout, _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 50, false),
            OnCompleted<bool>(Subscribed + 50)
        );
    }

    [Fact]
    public void WaitForChangeSuccess()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 1),
            OnNext(Subscribed + 30, 2),
            OnNext(Subscribed + 40, 1),
            OnCompleted<int>(Subscribed + 50)
        );

        _testScheduler.Start(() =>
            observable.WaitForChange(LongTimeout, _testScheduler)
            ).Messages.AssertEqual(
                OnNext(Subscribed + 30, true),
                OnCompleted<bool>(Subscribed + 30)
            );
    }

    [Fact]
    public void WaitForChangeTimesOut()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, 1),
            OnNext(Subscribed + 20, 1),
            OnNext(Subscribed + 30, 1),
            OnNext(Subscribed + 40, 1),
            OnCompleted<int>(Subscribed + 50)
        );

        _testScheduler.Start(() =>
            observable.WaitForChange(TimeSpan.FromTicks(35), _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 35, false),
            OnCompleted<bool>(Subscribed + 35)
        );
    }

    [Fact]
    public void WaitForChangeIgnoresNulls()
    {
        const int transformingToNull = 2;

        var observable = _testScheduler.CreateHotObservable(
            OnNext(Subscribed + 10, transformingToNull),
            OnNext(Subscribed + 20, transformingToNull + 1),
            OnNext(Subscribed + 30, transformingToNull),
            OnNext(Subscribed + 40, transformingToNull + 1),
            OnNext(Subscribed + 50, transformingToNull + 2),
            OnCompleted<int>(Subscribed + 60)
        ).Select<int, int?>(
            i => i != transformingToNull ? i : null
        );

        _testScheduler.Start(() =>
            observable.WaitForChange(LongTimeout, _testScheduler)
        ).Messages.AssertEqual(
            OnNext(Subscribed + 50, true),
            OnCompleted<bool>(Subscribed + 50)
        );
    }

    #endregion
    
    #region CompareWithFirst

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

    #endregion

    #region WaitTrue

    [Fact]
    public void WaitTrueEmpty()
    {
        var observable = _testScheduler.CreateHotObservable(
            OnCompleted<bool>(300)
        );
        _testScheduler.Start(() =>
            observable.WaitTrue(TimeSpan.FromTicks(500), _testScheduler)
        ).Messages.AssertEqual(
            OnCompleted<bool>(300)
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
            OnNext(Subscribed + 500, false),
            OnCompleted<bool>(Subscribed + 500)
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
    
    #endregion

    #region Test Setup

    private static readonly TimeSpan LongTimeout = TimeSpan.FromTicks(Disposed);

    private readonly TestScheduler _testScheduler = new();
    
    #endregion
}