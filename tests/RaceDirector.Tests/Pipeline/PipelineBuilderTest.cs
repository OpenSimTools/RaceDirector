using RaceDirector.Pipeline;
using static RaceDirector.Pipeline.PipelineBuilder;
using System.Threading.Tasks.Dataflow;
using Xunit;
using System;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline
{
    [IntegrationTest]
    public class PipelineBuilderTest
    {
        protected static TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

        [Fact]
        public void LinksSourceToTargetsOfAssignableType()
        {
            var sourceNodeV1 = new OneSourceNode<V1<int, long>>();
            var targetNodeIV0 = new OneTargetNode<IV0<int>>();
            var targetNodeIV1 = new OneTargetNode<IV1<int, long>>();
            var message = new V1<int, long>(1, 2);

            LinkNodes(sourceNodeV1, targetNodeIV0, targetNodeIV1);

            sourceNodeV1.Post1(message);
            Assert.Equal(message.P1, targetNodeIV0.EventuallyReceive1().P1);
            Assert.Equal(message, targetNodeIV1.EventuallyReceive1());
        }

        [Fact]
        public void DoesNotLinkSourceWhenTargetTypeUnssignable()
        {
            var sourceNode = new OneSourceNode<int>();
            var targetNode = new TwoTargetsNode<int, string>();

            LinkNodes(sourceNode, targetNode);

            sourceNode.Post1(42);
            Assert.Equal(42, targetNode.EventuallyReceive1());
            Assert.Null(targetNode.TryReceive2());
        }

        [Fact]
        public void SupportsBlocksThatAreBothSourceAndTarget()
        {
            var sourceNode = new OneSourceNode<int>();
            var transformationNode = new TransformationNode<int, string>(i => i.ToString());
            var targetNode = new OneTargetNode<string>();

            LinkNodes(sourceNode, transformationNode, targetNode);

            sourceNode.Post1(42);
            Assert.Equal("42", targetNode.EventuallyReceive1());
        }

        #region Test setup

        public interface IV0<T> { T P1 { get; } }
        public interface IV1<T, V> : IV0<T> { V P2 { get; } }

        public record V1<T, V>(T P1, V P2) : IV1<T, V>;

        public class OneSourceNode<O1> : INode
        {
            public ISourceBlock<O1> S1 => _s1;
            private BufferBlock<O1> _s1 = new BufferBlock<O1>();
            public void Post1(O1 o1) => _s1.Post(o1);
        }

        public class OneTargetNode<I1> : INode
        {
            public ITargetBlock<I1> T1 => _t1;
            private BufferBlock<I1> _t1 = new BufferBlock<I1>();
            public I1 EventuallyReceive1() => _t1.Receive(Timeout);
        }

        public class TwoTargetsNode<T, V> : OneTargetNode<T>
        {
            public ITargetBlock<V> T2 => _t2;
            private BufferBlock<V> _t2 = new BufferBlock<V>();
            public V Receive2() => _t2.Receive(Timeout);
            public V? TryReceive2()
            {
                try
                {
                    return _t2.Receive(TimeSpan.Zero);
                }
                catch
                {
                    return default(V);
                };
            }
        }

        public class TransformationNode<I, O> : INode
        {
            public TransformBlock<I, O> SourceAndTarget { get; private set; }
            
            public TransformationNode(Func<I, O> f)
            {
                SourceAndTarget = new TransformBlock<I, O>(f);
            }
        }

        #endregion
    }
}
