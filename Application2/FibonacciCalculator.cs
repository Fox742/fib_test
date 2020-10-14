using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;

namespace Application2
{
    public class FibonacciCalculator
    {
        private static ConcurrentQueue<int> _queue = new ConcurrentQueue<int>();
        private static Thread FibonacciProcessor = null;
        private static int lastCalculated = 1;
        private static Dictionary<int, int> FibonacciCache = new Dictionary<int, int>();

        public static void Start()
        {
            FibonacciProcessor = new Thread(ProcessFibonacciRequests);
            FibonacciProcessor.IsBackground = true;
            FibonacciProcessor.Start();
        }

        private static void sendResult(int parameter, int result, IBus _bus)
        {
            _bus.PublishAsync<Tuple<int, int>>(new Tuple<int, int>(parameter, result));
        }

        private static void ProcessFibonacciRequests()
        {
            using (IBus bus = RabbitHutch.CreateBus("host=localhost"))
            {
                while (true)
                {
                    int number;
                    if (_queue.TryDequeue(out number))
                    {
                        // Process number
                        if (FibonacciCache.ContainsKey(number))
                        {
                            sendResult(number,FibonacciCache[number],bus);
                        }
                        else
                        {
                            lastCalculated = number + lastCalculated;
                            FibonacciCache[number] = lastCalculated;
                            sendResult(number,lastCalculated,bus);
                        }

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
