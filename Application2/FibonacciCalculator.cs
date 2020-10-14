using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application2
{
    public class FibonacciCalculator
    {
        private static ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();
        private static Thread FibonacciProcessor = null;

        static FibonacciCalculator()
        {
            FibonacciProcessor = new Thread(ProcessFibonacciRequests);
            FibonacciProcessor.IsBackground = true;
            FibonacciProcessor.Start();
        }

        private static void ProcessFibonacciRequests()
        {
            while (true)
            {
                int number;
                if ( _queue.TryDequeue(out number) )
                {
                    // Process number


                }
            }
        }

        public static void Enqueue(int current)
        {
            _queue.Enqueue(current);
        }
    }
}
