using System;
using System.Reactive.Linq;

namespace RaceDirector.Pipeline.Utils
{
    public static class ObserverExtensions
    {

        /// <summary>
        /// Like <see cref="M:SelectMany"/> but the selector terminates when the source emits an element.
        /// </summary>
        /// <param name="source">TODO</param>
        /// <param name="selector">TODO</param>
        /// <typeparam name="TSource">TODO</typeparam>
        /// <typeparam name="TResult">TODO</typeparam>
        /// <returns>TODO</returns>
        public static IObservable<TResult> SelectManyUntilNext<TSource, TResult>(this IObservable<TSource> source,
            Func<TSource, IObservable<TResult>> selector)
        {
            return source.SelectMany(s => selector(s).TakeUntil(source));
        }
    }
}