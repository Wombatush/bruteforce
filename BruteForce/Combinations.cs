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
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    public abstract class Combinations
    {
        private Combinations()
        {
        }

        public sealed class Bucket<T> : Combinations
        {
            private readonly Combinator combinator;

            internal Bucket(Combinator combinator)
            {
                this.combinator = combinator;
            }

            /// <summary>
            /// Executes the test with given test cases sequentially.
            /// </summary>
            /// <param name="action">Action representing the test, where an argument is a test case values being generated.</param>
            public void RunSequentially(Action<T> action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                var total = 0;
                var failures = new ConcurrentDictionary<T, Exception>();

                foreach (var test in combinator.Yield<T>())
                {
                    ExecuteWrapped(test, action, failures);

                    ++total;
                }

                Assert.That(total, Is.GreaterThan(0), "No any test cases has been executed - please verify the combinatory setup");

                if (failures.Any())
                {
                    var counter = 0;
                    var builder = new StringBuilder();
                    builder.AppendFormat("The following ({0}) test case(s) failed out of ({1}) executed:", failures.Count, total);
                    foreach (var failure in failures)
                    {
                        builder.AppendLine();
                        builder.AppendLine();
                        builder.AppendFormat("({0}) : {1}", ++counter, failure.Key);
                        builder.AppendLine();
                        builder.Append(failure.Value);
                    }

                    Assert.Fail(builder.ToString());
                }

                Debug.WriteLine("Total test cases executed in sequence: {0}", total);
            }

            /// <summary>
            /// Executes the test with given test cases in parallel.
            /// </summary>
            /// <param name="action">Action representing the test, where an argument is a test case values being generated.</param>
            public void RunInParallel(Action<T> action)
            {
                if (action == null)
                {
                    throw new ArgumentNullException(nameof(action));
                }

                var total = 0;
                var failures = new ConcurrentDictionary<T, Exception>();

                Parallel.ForEach(combinator.Yield<T>(), test => { ExecuteWrapped(test, action, failures); Interlocked.Increment(ref total); });

                Assert.That(total, Is.GreaterThan(0), "No any test cases has been executed - please verify the combinatory setup");

                if (failures.Any())
                {
                    var counter = 0;
                    var builder = new StringBuilder();
                    builder.AppendFormat("The following ({0}) test case(s) failed out of ({1}) executed:", failures.Count, total);
                    foreach (var failure in failures)
                    {
                        builder.AppendLine();
                        builder.AppendLine();
                        builder.AppendFormat("({0}) : {1}", ++counter, failure.Key);
                        builder.AppendLine();
                        builder.AppendFormat(failure.Value.ToString());
                    }

                    Assert.Fail(builder.ToString());
                }

                Debug.WriteLine("Total test cases executed in parallel: {0}", total);
            }

            private void ExecuteWrapped(T test, Action<T> action, ConcurrentDictionary<T, Exception> failures)
            {
                try
                {
                    action(test);
                }
                catch (Exception exception)
                {
                    failures.AddOrUpdate(test, _ => exception, (_, __) => exception);
                }
            }
        }

        /// <summary>
        /// Composes the combinatory test cases.
        /// </summary>
        /// <typeparam name="T">A resulting anonymous type used for the test case.</typeparam>
        /// <param name="func">A method used to identify the resulting anonymous type and configure the combinatory.</param>
        /// <returns>A bucket used to generate test cases.</returns>
        public static Bucket<T> Compose<T>(Func<Combinator, T> func)
        {
            var combinator = new Combinator();
            func(combinator);
            return new Bucket<T>(combinator);
        }
    }
}