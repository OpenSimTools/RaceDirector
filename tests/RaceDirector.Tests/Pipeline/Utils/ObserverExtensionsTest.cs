using Microsoft.Reactive.Testing;
using RaceDirector.Pipeline.Utils;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Utils
{
    [UnitTest]
    public class ObserverExtensionsTest : ReactiveTest
    {
        [Fact]
        public void SelectorsTerminateWhenSourceEmits()
        {
            var scheduler = new TestScheduler();
            const long sourceStart = Subscribed + 5;
            const long sourceEmitOffset = 21;
            const long sourceCompleteOffset = 100;
            var source = scheduler.CreateHotObservable(
                OnNext(sourceStart, 0),
                OnNext(sourceStart + sourceEmitOffset, 0),
                OnCompleted<int>(sourceStart + sourceCompleteOffset)
            );
            const long selectorCompleteOffset = 50;
            var selector = scheduler.CreateColdObservable(
                OnNext(10, 1),
                OnNext(20, 2),
                OnNext(30, 3),
                OnNext(40, 4),
                OnCompleted<int>(selectorCompleteOffset)
            );

            var res = scheduler.Start(() =>
                source.SelectManyUntilNext(_ => selector)
            );
            
            source.Subscriptions.AssertEqual(
                // Test
                Subscribe(Subscribed, sourceStart + sourceCompleteOffset),
                // SelectManyUntilNext
                Subscribe(sourceStart, sourceStart + sourceEmitOffset),
                Subscribe(sourceStart + sourceEmitOffset, sourceStart + sourceEmitOffset + selectorCompleteOffset)
            );
            
            res.ReceivedValues().AssertEqual(1, 2, 1, 2, 3, 4);
        }
        
        [Fact]
        public void SelectorDoesNotTerminateWhenSourceCompletes()
        {
            var scheduler = new TestScheduler();
            const long sourceStart = Subscribed + 5;
            const long sourceCompleteOffset = 25;
            var source = scheduler.CreateHotObservable(
                OnNext(sourceStart, 0),
                OnCompleted<int>(sourceStart + sourceCompleteOffset)
            );
            var selector = scheduler.CreateColdObservable(
                OnNext(10, 1),
                OnNext(20, 2),
                OnNext(30, 3)
            );

            var res = scheduler.Start(() =>
                source.SelectManyUntilNext(_ => selector)
            );
            
            source.Subscriptions.AssertEqual(
                // Test
                Subscribe(Subscribed, sourceStart + sourceCompleteOffset),
                // SelectManyUntilNext
                Subscribe(sourceStart, sourceStart + sourceCompleteOffset)
            );
            
            res.ReceivedValues().AssertEqual(1, 2, 3);
        }
    }
}