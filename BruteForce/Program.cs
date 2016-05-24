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

    static class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sequential combinatory tests:");

            Combinations
                .Compose(x => new 
                {
                    Boolean = x.OneOf<bool?>()
                })
                .RunSequentially(test =>
                {
                    Console.WriteLine($"{nameof(test.Boolean)}: {{0}}", test.Boolean);
                });

            Console.WriteLine("Parallel combinatory tests: (order is not deterministic)");

            Combinations
                .Compose(x => new
                {
                    Greeting = x.Only("Hello", "Howdy", "GDay"),
                    Participant = x.Only("John", "James", "Bob")
                })
                .RunInParallel(test =>
                {
                    Console.WriteLine("{0}, {1}", test.Greeting, test.Participant);
                });
        }
    }
}
