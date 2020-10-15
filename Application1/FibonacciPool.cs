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
        private Dictionary<string, FibonacciSequence> _sequences = new Dictionary<string, FibonacciSequence>();
        List<string> keys = new List<string>();
        private Task QueryTask = null;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private IBus _bus;
        private string _webApiLink;
        private string _rabbitServiceURL;
        private List<int> printedValuesLast = new List<int>();

        public FibonacciPool(int SequenceAmount, string webAPIString, string rabbitService)
        {
            _webApiLink = webAPIString;
            _rabbitServiceURL = rabbitService;

            _bus = RabbitHutch.CreateBus(_rabbitServiceURL);
            _bus.Subscribe<Tuple<string, int>>(Guid.NewGuid().ToString(), OnResponse);

            for (int i = 0; i < SequenceAmount; i++)
            {
                string currentGuid = Guid.NewGuid().ToString();
                _sequences[currentGuid] = new FibonacciSequence();
                keys.Add(currentGuid);
            }
            QueryTask = new Task(() => Query(cancellationToken.Token) );
            
        }

        public void Start()
        {
            QueryTask.Start();
        }

        public void PrintFibCurrentValues()
        {
            List<int> ValuesToPrint = new List<int>();
            foreach (string oneKey in keys)
            {
                ValuesToPrint.Add(_sequences[oneKey].Current);
            }
            if (!ValuesToPrint.SequenceEqual<int>(printedValuesLast))
            {
                foreach (int oneVal in ValuesToPrint)
                {
                    Console.Write(String.Format("{0,8}|", oneVal));
                }
                Console.WriteLine();
                printedValuesLast = ValuesToPrint;
            }

        }

        private void SendQuery(int current,string guid)
        {
            
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(_webApiLink + "fibonacci/next?n="+current.ToString()+"&guid="+guid.ToString());
            request.Method = HttpMethod.Get;
            request.Headers.Add("Accept", "application/json");
            client.SendAsync(request).Wait();
        }

        private void Query(CancellationToken token)
        {
            int i = 1;

            while (!token.IsCancellationRequested)
            {

                    FibonacciSequence current = _sequences[keys[i]];
                    lock (current)
                    {
                        if (!current.Waiting)
                        {
                            try
                            {
                                SendQuery(current.Current, keys[i]);
                                current.Waiting = true;
                            }
                            catch(System.Exception e)
                            {

                            }
                        }
                    }

                i++;
                if (i >= _sequences.Count)
                    i = 0;
            }
        }
        
        public void OnResponse(Tuple<string, int> message)
        {
                lock (_sequences)
                {
                    _sequences[message.Item1].Current = message.Item2;
                    _sequences[message.Item1].Waiting = false;
                    PrintFibCurrentValues();
                }
        }
        
        public void Dispose()
        {
            _bus.Dispose();
            cancellationToken.Cancel();
            QueryTask.Wait();
        }

    }
}
