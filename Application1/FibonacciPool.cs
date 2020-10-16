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
    /// <summary>
    /// Класс, управляющий параллельным расчётом последовательностей
    ///     Хранит внутри себя информацию о каждой из расчитываемых последовательностей (её текущее значение n и флаг ожидания следующего числа)
    ///     В отдельном потоке происходит постоянная проверка каждой последовательности по списку, ожидает ли последовательность ответа от сервера.
    ///         Если последовательность не ожидает ответа от сервера, то по WebAPI мы отправляем текущее значение последовательности и выставляем для данной последовательности флаг ожидания ответа флаг 
    ///     Ответы от сервера приходят по RabbitMQ с использованием библиотеки EasyNetQ
    ///         При получении ответа от сервера - присланное им число записывается в соответствующую послежовательность и с последовательности снимается флаг ожидания ответа
    /// </summary>
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

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="SequenceAmount">Количество одновременно расчитываемых последовательностей</param>
        /// <param name="webAPIString">Адрес хоста web-службы</param>
        /// <param name="rabbitService">Адрес RabbitMQ службы</param>
        public FibonacciPool(int SequenceAmount, string webAPIString, string rabbitService)
        {
            _webApiLink = webAPIString;
            _rabbitServiceURL = rabbitService;

            // Подписываемся на rabbitMQ
            _bus = RabbitHutch.CreateBus(_rabbitServiceURL);
            _bus.Subscribe<Tuple<string, int>>(Guid.NewGuid().ToString(), OnResponse);

            // Создаём словарь последовательностей
            for (int i = 0; i < SequenceAmount; i++)
            {
                string currentGuid = Guid.NewGuid().ToString();
                _sequences[currentGuid] = new FibonacciSequence();
                keys.Add(currentGuid);
            }
            QueryTask = new Task(() => Query(cancellationToken.Token) );
            
        }

        /// <summary>
        /// Начинаем отправлять запросы серверу
        /// </summary>
        public void Start()
        {
            QueryTask.Start();
        }

        /// <summary>
        /// Печать значений всех последовательностей
        /// </summary>
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

        /// <summary>
        /// Метод отправки запроса следующего числа по WebAPI
        /// </summary>
        /// <param name="current">Текущее число последовательности (n)</param>
        /// <param name="guid">Идентификатор последовательности</param>
        private void SendQuery(int current,string guid)
        {
            
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(_webApiLink + "fibonacci/next?n="+current.ToString()+"&guid="+guid.ToString());
            request.Method = HttpMethod.Get;
            request.Headers.Add("Accept", "application/json");
            client.SendAsync(request).Wait();
        }

        /// <summary>
        /// Метод, рассылающий запросы серверу для получения следующих членов последовательностей
        /// </summary>
        /// <param name="token"></param>
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
        
        /// <summary>
        /// Обработчик ответа от сервера
        /// </summary>
        /// <param name="message"></param>
        public void OnResponse(Tuple<string, int> message)
        {
                lock (_sequences)
                {
                    // Берём из ответа слудующее число последовательности
                    _sequences[message.Item1].Current = message.Item2;

                    // Помечаем последовательность как неждущую ответа
                    _sequences[message.Item1].Waiting = false;
                    
                    // Печатаем состояние последовательностей
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
