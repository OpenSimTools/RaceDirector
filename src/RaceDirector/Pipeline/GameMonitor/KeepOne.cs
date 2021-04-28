using System;
using System.Collections.Generic;
using System.Linq;

namespace RaceDirector.Pipeline.GameMonitor
{
    public class KeepOne<T> where T : class
    {
        private readonly IEnumerable<T> _options;
        private T? _current;

        static private readonly IEnumerable<T> NoOutput = Enumerable.Empty<T>();

        public KeepOne(IEnumerable<T> options)
        {
            _options = options;
        }

        public IEnumerable<T?> Call(IEnumerable<T> input)
        {
            if (input.Contains(_current))
                return NoOutput;

            var matching = input.Intersect(_options);

            if (matching.Any())
            {
                _current = matching.First();
                return new[] { _current };
            }
            else if (_current is not null)
            {
                _current = null;
                return new[] { _current };
            }
            else
            {
                return NoOutput;
            }
        }
    }
}
