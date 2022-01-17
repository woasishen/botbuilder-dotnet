// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;

namespace Microsoft.Bot.Builder
{
    /// <summary>
    /// A console stopwatch.
    /// </summary>
    public class StopwatchPlus
    {
        private readonly string _id;
        private readonly Stopwatch _stopwatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopwatchPlus"/> class.
        /// </summary>
        /// <param name="id">The id for the stopwatch.</param>
        /// <param name="message">An optional message.</param>
        public StopwatchPlus(string id, string message = null)
        {
            _id = id;
            _stopwatch = new Stopwatch();
            Console.WriteLine($"[{DateTime.Now}:{_id}-Start] {message}");
            _stopwatch.Start();
        }

        ///// <summary>
        ///// Starts the stopwatch.
        ///// </summary>
        ///// <param name="message">An optional message.</param>
        //public void Start(string message = null)
        //{
        //    Console.WriteLine($"[{DateTime.Now}:{_id}-Start] {message}");
        //    _stopwatch.Start();
        //}

        /// <summary>
        /// Displays elapsed time.
        /// </summary>
        /// <param name="message">An optional message.</param>
        public void Elapsed(string message = null)
        {
            Console.WriteLine($"[{DateTime.Now}:{_id}-Elapsed] {_stopwatch.ElapsedMilliseconds:##,###} ms {message}");
        }

        /// <summary>
        /// Displays elapsed time and restarts.
        /// </summary>
        /// <param name="message">An optional message.</param>
        public void Restart(string message = null)
        {
            Console.WriteLine($"[{DateTime.Now}:{_id}-Restart] {_stopwatch.ElapsedMilliseconds:##,###} ms {message}");
            _stopwatch.Restart();
        }

        /// <summary>
        /// Stops the stopwatch.
        /// </summary>
        /// <param name="message">An optional message.</param>
        public void Stop(string message = null)
        {
            Console.WriteLine($"[{DateTime.Now}:{_id}-Stop] {_stopwatch.ElapsedMilliseconds:##,###} ms {message}");
            _stopwatch.Stop();
        }
    }
}
