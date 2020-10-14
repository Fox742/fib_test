using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace Application1
{
    class FibonacciPool: IDisposable
    {

        private List<FibonacciSequence> _sequences = new List<FibonacciSequence>();
        Task QueryTask = null;
        CancellationTokenSource cancellationToken = new CancellationTokenSource();


        public FibonacciPool(int SequenceAmount)
        {
            for (int i = 0; i < SequenceAmount; i++)
                _sequences.Add(new FibonacciSequence());
            QueryTask = new Task(() => Query(cancellationToken.Token) );
            QueryTask.Start();
        }

        public void Start()
        {

        }

        public void PrintFibCurrentValues()
        {

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
            int i = 0;
            while (!token.IsCancellationRequested)
            {
                lock(_sequences)
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

        public void OnResponse()
        {

        }

        public void Dispose()
        {
            cancellationToken.Cancel();
            QueryTask.Wait();
        }

    }
}
