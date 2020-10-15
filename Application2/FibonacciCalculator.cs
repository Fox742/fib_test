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
        private static ConcurrentQueue<FibonacciReq> _queue = new ConcurrentQueue<FibonacciReq>();
        private static Dictionary<string,int> SequenceCache = new Dictionary<string,int>();
        private static Thread FibonacciProcessor = null;
        private static int previousMember = 1;
        private static string _rabbitConnectionString;



        private static Dictionary<int, int> FibonacciCache = new Dictionary<int, int>();

        public static void Start(string rabbitConnectionString)
        {
            _rabbitConnectionString = rabbitConnectionString;
            FibonacciProcessor = new Thread(ProcessFibonacciRequests);
            FibonacciProcessor.IsBackground = true;
            FibonacciProcessor.Start();
        }

        private static void sendResult(string guid, int result, IBus _bus)
        {
            _bus.PublishAsync<Tuple<string, int>>(new Tuple<string, int>(guid, result));
        }

        private static void ProcessFibonacciRequests()
        {
            using (IBus bus = RabbitHutch.CreateBus(_rabbitConnectionString))
            {
                while (true)
                {
                    FibonacciReq fr;
                    if (_queue.TryDequeue(out fr))
                    {
                        Thread.Sleep(350);
                        if(! SequenceCache.ContainsKey(fr.id))
                        {
                            SequenceCache[fr.id] = 0;
                        }

                        if (fr.previousNumber==0)
                        {
                            sendResult(fr.id, 1, bus);
                        }
                        else
                        {
                            int sum = fr.previousNumber + SequenceCache[fr.id];
                            SequenceCache[fr.id] = fr.previousNumber;
                            sendResult(fr.id, sum, bus);
                        }


                    }
                }
            }
        }

        public static void Enqueue(FibonacciReq fr)
        {
            _queue.Enqueue(fr);
        }
    }
}
