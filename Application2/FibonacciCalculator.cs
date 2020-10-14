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
        private static int lastCalculated = 1;
        private static Dictionary<int, int> FibonacciCache = new Dictionary<int, int>();

        static FibonacciCalculator()
        {
            FibonacciProcessor = new Thread(ProcessFibonacciRequests);
            FibonacciProcessor.IsBackground = true;
            FibonacciProcessor.Start();
        }

        private static void sendResult(int result)
        {

        }

        private static void ProcessFibonacciRequests()
        {
            while (true)
            {
                int number;
                if ( _queue.TryDequeue(out number) )
                {
                    // Process number
                    if ( FibonacciCache.ContainsKey(number) )
                    {
                        sendResult(FibonacciCache[number]); 
                    }
                    else
                    {
                        lastCalculated = number + lastCalculated;
                        FibonacciCache[number] = lastCalculated;
                        sendResult(lastCalculated);
                    }

                }
            }
        }

        public static void Enqueue(int current)
        {
            _queue.Enqueue(current);
        }
    }
}
