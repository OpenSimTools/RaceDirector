using Microsoft.Reactive.Testing;
using RaceDirector.Pipeline.Utils;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Utils;

[UnitTest]
public class SelectManyUntilNextTest : ReactiveTest
{
    [Fact]
    public void SelectorsStopEmittingWhenSourceEmits()
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
    public void SelectorKeepsEmittingWhenSourceCompletes()
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
}