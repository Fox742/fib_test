using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using EasyNetQ;
using System.Linq;

namespace Application1
{
    class FibonacciPool: IDisposable
    {

        private List<FibonacciSequence> _sequences = new List<FibonacciSequence>();
        private Task QueryTask = null;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private IBus _bus;

        public FibonacciPool(int SequenceAmount)
        {
            _bus = RabbitHutch.CreateBus("host=localhost");
            _bus.Subscribe<Tuple<int, int>>(string.Empty,OnResponse);

            for (int i = 0; i < SequenceAmount; i++)
                _sequences.Add(new FibonacciSequence());
            QueryTask = new Task(() => Query(cancellationToken.Token) );
            
        }

        public void Start()
        {
            QueryTask.Start();
        }

        public void PrintFibCurrentValues()
        {
            foreach (FibonacciSequence fs in _sequences)
            {
                lock (fs)
                {
                    if ( _sequences.First()!=fs )
                    {
                        Console.Write(" ");
                    }
                    Console.Write(fs.Current);
                }
            }
            Console.WriteLine();
        }

        private void SendQuery(int current)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri("https://localhost:44370/fibonacci/next?n="+current.ToString());
            request.Method = HttpMethod.Get;
            request.Headers.Add("Accept", "application/json");
            client.SendAsync(request);
        }

        private void Query(CancellationToken token)
        {
            int i = 1;
            while (!token.IsCancellationRequested)
            {
                lock(_sequences[i])
                {
                    if (!_sequences[i].Waiting)
                    {
                        SendQuery(_sequences[i].Current);
                        _sequences[i].Waiting = true;
                    }
                }
                i++;
                if (i >= _sequences.Count)
                    i = 0;
            }
        }

        public void OnResponse(Tuple<int, int> message)
        {
            foreach (FibonacciSequence fs in _sequences)
            {
                lock (fs)
                {
                    if ((fs.Waiting)&&(fs.Current==message.Item1))
                    {
                        fs.Current = message.Item2;
                        fs.Waiting = false;
                        break ;
                    }
                }
            }
            Task.Run( ()=> { PrintFibCurrentValues(); } );
        }

        public void Dispose()
        {
            _bus.Dispose();
            cancellationToken.Cancel();
            QueryTask.Wait();
        }

    }
}
