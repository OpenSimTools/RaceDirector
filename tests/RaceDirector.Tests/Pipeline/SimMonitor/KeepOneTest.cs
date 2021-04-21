using Xunit;
using RaceDirector.Pipeline.SimMonitor;
using System.Linq;
using Xunit.Categories;
using System.Collections.Generic;

namespace RaceDirector.Tests.Pipeline.SimMonitor
{
    [UnitTest]
    public class KeepOneTest
    {
        private static string?[] NoOutput = new string?[0];
        private static string?[] NullOutput = new string?[] { null };

        [Fact]
        public void EmitsWhenFirstMatched()
        {
            AssertIOKeepOneAB(
                input: new[] {
                    new[] { "a" }
                },
                expectedOutput: new string?[][] {
                    new string?[] { "a" }
                }
            );
        }

        [Fact]
        public void DoesntEmitIfFirstNotMatched()
        {
            AssertIOKeepOneAB(
                input: new[] {
                    new[] { "x" },
                },
                expectedOutput: new string?[][] {
                    NoOutput
                }
            );
        }

        [Fact]
        public void EmitsWhenMatchAfterNoMatch()
        {
            AssertIOKeepOneAB(
                input: new[] {
                    new[] { "x" },
                    new[] { "a" },
                },
                expectedOutput: new string?[][] {
                    NoOutput,
                    new string?[] { "a" }
                }
            );
        }

        [Fact]
        public void EmitsTheFirstWhenMultipleFound()
        {
            AssertIOKeepOneAB(
                input: new[] {
                    new[] { "x", "b", "a" }
                },
                expectedOutput: new string?[][] {
                    new string?[] { "b" }
                }
            );
        }

        [Fact]
        public void EmitsWhenNotMatchingAnymore()
        {
            AssertIOKeepOneAB(
                input: new[] {
                    new[] { "a" },
                    new[] { "x" }
                },
                expectedOutput: new string?[][] {
                    new string?[] { "a" },
                    NullOutput
                }
            );
        }

        [Fact]
        public void DoesntEmitWhenPreviousMatchPresent()
        {
            AssertIOKeepOneAB(
                input: new[] {
                    new[] { "a" },
                    new[] { "a", "b" },
                    new[] { "b" }
                },
                expectedOutput: new string?[][] {
                    new string?[] { "a" },
                    NoOutput,
                    new string?[] { "b" }
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
            var enumerableInput = Enumerable.OfType<IEnumerable<string>>(input);
            var output = enumerableInput.Select(kos.Call).ToArray();
            Assert.Equal(expectedOutput, output);
        }
    }
}
