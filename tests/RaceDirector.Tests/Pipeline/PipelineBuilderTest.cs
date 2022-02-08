using RaceDirector.Pipeline;
using Xunit;
using System;
using Xunit.Categories;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Microsoft.Reactive.Testing;
using System.Linq;
using RaceDirector.Tests.Pipeline.Utils;
using System.Reactive;

namespace RaceDirector.Tests.Pipeline
{
    [IntegrationTest]
    public class PipelineBuilderTest
    {
        private static int EventsTimestamp = 42;

        private readonly TestScheduler testScheduler = new ();

        [Fact]
        public void LinksSourceToTargetsOfAssignableType()
        {
            var message = new V1<int, long>(1, 2);

            var sourceNodeV1 = new OneSourceNode<V1<int, long>>(testScheduler, message);
            var targetNodeIV0 = new OneTargetNode<IV0<int>>(testScheduler);
            var targetNodeIV1 = new OneTargetNode<IV1<int, long>>(testScheduler);

            LinkAndRun(testScheduler, sourceNodeV1, targetNodeIV0, targetNodeIV1);

            Assert.Equal(message, targetNodeIV0.T1.ReceivedValues().First());
            Assert.Equal(message, targetNodeIV1.T1.ReceivedValues().First());
        }

        [Fact]
        public void DoesNotLinkSourceWhenTargetTypeUnssignable()
        {
            var sourceNode = new OneSourceNode<int>(testScheduler, 42);
            var targetNode = new TwoTargetsNode<int, string>(testScheduler);

            LinkAndRun(testScheduler, sourceNode, targetNode);

            Assert.Equal(42, targetNode.T1.ReceivedValues().First());
            Assert.Empty(targetNode.T2.ReceivedValues());
        }

        [Fact]
        public void SupportsBlocksThatAreBothSourceAndTarget()
        {
            var sourceNode = new OneSourceNode<int>(testScheduler, 42);
            var transformationNode = new TransformationNode<int, string>(i => i.ToString());
            var targetNode = new OneTargetNode<string>(testScheduler);

            LinkAndRun(testScheduler, sourceNode, transformationNode, targetNode);

            Assert.Equal("42", targetNode.T1.ReceivedValues().First());
        }

        #region Test setup

        public interface IV0<T1> { T1 P1 { get; } }
        public interface IV1<T1, T2> : IV0<T1> { T2 P2 { get; } }

        public record V1<T1, T2>(T1 P1, T2 P2) : IV1<T1, T2>;

        public class OneSourceNode<T> : INode
        {
            public ITestableObservable<T> S1  { get; private set; }

            public OneSourceNode(TestScheduler testScheduler, params T[] messages)
            {
                var recordedNotifications = messages.Select(m =>
                    new Recorded<Notification<T>>(EventsTimestamp, Notification.CreateOnNext(m))
                ).ToArray();
                S1 = testScheduler.CreateHotObservable<T>(recordedNotifications);
            }
        }

        public class OneTargetNode<T> : INode
        {
            public ITestableObserver<T> T1 { get; private set; }

            public OneTargetNode(TestScheduler testScheduler)
            {
                T1 = testScheduler.CreateObserver<T>();
            }
        }

        public class TwoTargetsNode<TI1, TI2> : OneTargetNode<TI1>
        {
            public ITestableObserver<TI2> T2 { get; private set; }

            public TwoTargetsNode(TestScheduler testScheduler) : base(testScheduler)
            {
                T2 = testScheduler.CreateObserver<TI2>();
            }
        }

        public class TransformationNode<TI, TO> : INode
        {
            public ISubject<TI, TO> SourceAndTarget { get; private set; }
            
            public TransformationNode(Func<TI, TO> f)
            {
                var subject = new Subject<TI>();
                SourceAndTarget = Subject.Create(subject, subject.Select(f));
            }
        }

        private static void LinkAndRun(TestScheduler testScheduler, params INode[] nodes)
        {
            PipelineBuilder.LinkNodes(nodes);
            testScheduler.AdvanceTo(EventsTimestamp);
        }

        #endregion
    }
}
