using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceDirector.Pipeline.SimMonitor
{
    public class KeepOne<T> where T : class
    {
        private readonly T[] Options;
        private T? Current;
        static private readonly IEnumerable<T> NoOutput = Enumerable.Empty<T>();

        public KeepOne(T[] options)
        {
            Options = options;
        }

        public IEnumerable<T?> Call(IEnumerable<T> input)
        {
            if (input.Contains(Current))
                return NoOutput;

            var matching = input.Intersect(Options);

            if (matching.Any())
            {
                Current = matching.First();
                return new[] { Current };
            }
            else if (Current is not null)
            {
                Current = null;
                return new[] { Current };
            }
            else
            {
                return NoOutput;
            }
        }
    }
}
