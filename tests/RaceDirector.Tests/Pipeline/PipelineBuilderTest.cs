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
using TestUtils;

namespace RaceDirector.Tests.Pipeline;

[IntegrationTest]
public class PipelineBuilderTest
{
    private static int EventsTimestamp = 42;

    private readonly TestScheduler testScheduler = new ();

    [Fact]
    public void LinksObservableToObserversOfAssignableType()
    {
        var message = new V1<int, long>(1, 2);

        var observableNodeV1 = new OneObservableNode<V1<int, long>>(testScheduler, message);
        var observerNodeIV0 = new OneObserverNode<IV0<int>>(testScheduler);
        var observerNodeIV1 = new OneObserverNode<IV1<int, long>>(testScheduler);

        LinkAndRun(testScheduler, observableNodeV1, observerNodeIV0, observerNodeIV1);

        Assert.Equal(message, observerNodeIV0.T1.ReceivedValues().First());
        Assert.Equal(message, observerNodeIV1.T1.ReceivedValues().First());
    }

    [Fact]
    public void DoesNotLinkObservableWhenObserverTypeUnssignable()
    {
        var observableNode = new OneObservableNode<int>(testScheduler, 42);
        var observerNode = new TwoObserversNode<int, string>(testScheduler);

        LinkAndRun(testScheduler, observableNode, observerNode);

        Assert.Equal(42, observerNode.T1.ReceivedValues().First());
        Assert.Empty(observerNode.T2.ReceivedValues());
    }

    [Fact]
    public void SupportsBlocksThatAreBothObservableAndObserver()
    {
        var observableNode = new OneObservableNode<int>(testScheduler, 42);
        var transformationNode = new TransformationNode<int, string>(i => i.ToString());
        var observerNode = new OneObserverNode<string>(testScheduler);

        LinkAndRun(testScheduler, observableNode, transformationNode, observerNode);

        Assert.Equal("42", observerNode.T1.ReceivedValues().First());
    }

    #region Test setup

    public interface IV0<T1> { T1 P1 { get; } }
    public interface IV1<T1, T2> : IV0<T1> { T2 P2 { get; } }

    public record V1<T1, T2>(T1 P1, T2 P2) : IV1<T1, T2>;

    public class OneObservableNode<T> : INode
    {
        public ITestableObservable<T> S1  { get; private set; }

        public OneObservableNode(TestScheduler testScheduler, params T[] messages)
        {
            var recordedNotifications = messages.Select(m =>
                new Recorded<Notification<T>>(EventsTimestamp, Notification.CreateOnNext(m))
            ).ToArray();
            S1 = testScheduler.CreateHotObservable<T>(recordedNotifications);
        }
    }

    public class OneObserverNode<T> : INode
    {
        public ITestableObserver<T> T1 { get; private set; }

        public OneObserverNode(TestScheduler testScheduler)
        {
            T1 = testScheduler.CreateObserver<T>();
        }
    }

    public class TwoObserversNode<TI1, TI2> : OneObserverNode<TI1>
    {
        public ITestableObserver<TI2> T2 { get; private set; }

        public TwoObserversNode(TestScheduler testScheduler) : base(testScheduler)
        {
            T2 = testScheduler.CreateObserver<TI2>();
        }
    }

    public class TransformationNode<TI, TO> : INode
    {
        public ISubject<TI, TO> ObservableAndObserver { get; private set; }
            
        public TransformationNode(Func<TI, TO> f)
        {
            var subject = new Subject<TI>();
            ObservableAndObserver = Subject.Create(subject, subject.Select(f));
        }
    }

    private static void LinkAndRun(TestScheduler testScheduler, params INode[] nodes)
    {
        PipelineBuilder.LinkNodes(nodes);
        testScheduler.AdvanceTo(EventsTimestamp);
    }

    #endregion
}