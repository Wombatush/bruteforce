// This software was originally developed by Evgeny Nazarov
// Copyright © 2016 Evgeny Nazarov
// http://nazarov.com.au
// http://ultima-labs.com.au
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// We do not mind if you remove the above copyright notice 
// and this permission notice.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

namespace BruteForce
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public sealed class Combinator
    {
        private readonly List<IEnumerable> sequences = new List<IEnumerable>();

        internal Combinator()
        {
        }

        public T OneOf<T>()
        {
            if (typeof(T) == typeof(bool))
            {
                sequences.Add(new[] {true, false});
            }
            else if (typeof(T) == typeof(bool?))
            {
                sequences.Add(new[] { true, false, default(bool?) });
            }
            else
            {
                sequences.Add(Enum.GetValues(typeof(T)));
            }

            //// Returning the stub.
            return default(T);
        }

        public T Only<T>(T atLeastOne, params T[] orAnyNumberOfOther)
        {
            sequences.Add(new[] { atLeastOne }.Concat(orAnyNumberOfOther).ToArray());
            //// Returning the stub.
            return default(T);
        }

        internal IEnumerable<T> Yield<T>()
        {
            var anonymousTestCaseType = typeof(T);
            var props = anonymousTestCaseType.GetProperties();
            Assert.That(sequences.Count, Is.EqualTo(props.Length), $"It appears that {nameof(Combinator)} has not been used to define all properties");

            var ctors = anonymousTestCaseType.GetConstructors();
            Assert.That(ctors.Length, Is.EqualTo(1), $"It appears that {nameof(Combinator)} has not been used with anonymous type representing the test case");

            var ctor = ctors[0];
            Assert.That(ctor.GetParameters().Length, Is.EqualTo(props.Length), $"It appears that {nameof(Combinator)} has not been used with anonymous type representing the test case");

            var enumerators = sequences.Select(x => x.GetEnumerator()).ToArray();
            var pendingEnumerators = new Stack<IEnumerator>(enumerators.Reverse());
            var activeEnumerators = new Stack<IEnumerator>();

            if (sequences.Count == 0)
            {
                yield break;
            }

            if (sequences.Count == 1)
            {
                foreach (var instance in sequences[0])
                {
                    yield return (T) Activator.CreateInstance(anonymousTestCaseType, instance);
                }
            }
            else
            {
                while (true)
                {
                    if (pendingEnumerators.Any())
                    {
                        while (pendingEnumerators.Any())
                        {
                            var current = pendingEnumerators.Pop();

                            if (current.MoveNext())
                            {
                                activeEnumerators.Push(current);
                            }
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            var current = activeEnumerators.Pop();

                            if (current.MoveNext())
                            {
                                activeEnumerators.Push(current);
                                break;
                            }

                            if (pendingEnumerators.Any() && !activeEnumerators.Any())
                            {
                                yield break;
                            }

                            current.Reset();
                            pendingEnumerators.Push(current);
                        }

                        if (pendingEnumerators.Any())
                        {
                            continue;
                        }

                    }

                    var args = enumerators.Select(x => x.Current).ToArray();

                    yield return (T)Activator.CreateInstance(anonymousTestCaseType, args);
                }
            }
        }
    }
}