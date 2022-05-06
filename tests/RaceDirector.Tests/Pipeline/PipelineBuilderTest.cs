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
        private static TimeSpan Timeout = TimeSpan.FromMilliseconds(500);

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

        public interface IV0<T1> { T1 P1 { get; } }
        public interface IV1<T1, T2> : IV0<T1> { T2 P2 { get; } }

        public record V1<T1, T2>(T1 P1, T2 P2) : IV1<T1, T2>;

        public class OneSourceNode<TO1> : INode
        {
            public ISourceBlock<TO1> S1 => _s1;
            private BufferBlock<TO1> _s1 = new();
            public void Post1(TO1 o1) => _s1.Post(o1);
        }

        public class OneTargetNode<TI1> : INode
        {
            public ITargetBlock<TI1> T1 => _t1;
            private BufferBlock<TI1> _t1 = new();
            public TI1 EventuallyReceive1() => _t1.Receive(Timeout);
        }

        public class TwoTargetsNode<TI1, TI2> : OneTargetNode<TI1>
        {
            public ITargetBlock<TI2> T2 => _t2;
            private BufferBlock<TI2> _t2 = new();
            public TI2? TryReceive2()
            {
                try
                {
                    return _t2.Receive(TimeSpan.Zero);
                }
                catch
                {
                    return default;
                };
            }
        }

        public class TransformationNode<TI, TO> : INode
        {
            public TransformBlock<TI, TO> SourceAndTarget { get; private set; }
            
            public TransformationNode(Func<TI, TO> f)
            {
                SourceAndTarget = new TransformBlock<TI, TO>(f);
            }
        }

        #endregion
    }
}
