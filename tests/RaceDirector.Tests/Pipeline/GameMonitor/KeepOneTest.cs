using System;
using Xunit;
using RaceDirector.Pipeline.GameMonitor;
using System.Linq;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.GameMonitor
{
    [UnitTest]
    public class KeepOneTest
    {
        private static readonly string?[] NoOutput = Array.Empty<string?>();
        private static readonly string?[] NullOutput = { null };

        [Fact]
        public void EmitsWhenFirstMatched()
        {
            AssertIOKeepOneAB(
                input: new[]
                {
                    new[] { "a" }
                },
                expectedOutput: new[]
                {
                    new[] { "a" }
                }
            );
        }

        [Fact]
        public void DoesntEmitIfFirstNotMatched()
        {
            AssertIOKeepOneAB(
                input: new[]
                {
                    new[] { "x" },
                },
                expectedOutput: new[]
                {
                    NoOutput
                }
            );
        }

        [Fact]
        public void EmitsWhenMatchAfterNoMatch()
        {
            AssertIOKeepOneAB(
                input: new[]
                {
                    new[] { "x" },
                    new[] { "a" },
                },
                expectedOutput: new[]
                {
                    NoOutput,
                    new[] { "a" }
                }
            );
        }

        [Fact]
        public void EmitsTheFirstWhenMultipleFound()
        {
            AssertIOKeepOneAB(
                input: new[]
                {
                    new[] { "x", "b", "a" }
                },
                expectedOutput: new[]
                {
                    new[] { "b" }
                }
            );
        }

        [Fact]
        public void EmitsWhenNotMatchingAnymore()
        {
            AssertIOKeepOneAB(
                input: new[]
                {
                    new[] { "a" },
                    new[] { "x" }
                },
                expectedOutput: new[]
                {
                    new[] { "a" },
                    NullOutput
                }
            );
        }

        [Fact]
        public void DoesntEmitWhenPreviousMatchPresent()
        {
            AssertIOKeepOneAB(
                input: new[]
                {
                    new[] { "a" },
                    new[] { "a", "b" },
                    new[] { "b" }
                },
                expectedOutput: new[]
                {
                    new[] { "a" },
                    NoOutput,
                    new[] { "b" }
                }
            );
        }

        private void AssertIOKeepOneAB(string[][] input, string?[][] expectedOutput)
        {
             AssertIO(new[] { "a", "b" }, input, expectedOutput);
        }

        private void AssertIO(string[] config, string[][] input, string?[][] expectedOutput)
        {
            var kos = new KeepOne<string>(config);
            var output = input.AsEnumerable().Select(kos.Call).ToArray();
            Assert.Equal(expectedOutput.AsEnumerable(), output);
        }
    }
}
