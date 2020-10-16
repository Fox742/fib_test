using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;

namespace Application2
{
    /// <summary>
    /// Класс, обеспечивающий обработку запросов о вычислении членов последовательностей Фибоначчи
    ///     Получает на вход Очередь из запросов на вычисления очередного элемента последовательности Фибоначчи
    ///     В отдельном потоке обрабатывает очередь запросов и по RabbitMQ отправляет клиенту обратно
    /// </summary>
    public class FibonacciCalculator
    {
        private static ConcurrentQueue<FibonacciReq> _queue = new ConcurrentQueue<FibonacciReq>();  // Очередь запросов
        private static Dictionary<string,int> SequenceCache = new Dictionary<string,int>();         // Словарь, хранящий по уникальному ключу (guid) последнее вычисленное значение для конкретной последовательности
                                                                                                    //   Данный ключ отправляет клиент серверу и сервер также отправляет его клиенту обратно
        
        private static Thread FibonacciProcessor = null;                                            // Поток, в котором обрабатываются запросы от клиента из очереди
        private static string _rabbitConnectionString;                                              // ConnectionString для RabbitMQ


        /// <summary>
        /// Запустить поток обработки очереди запросов
        /// </summary>
        /// <param name="rabbitConnectionString"></param>
        public static void Start(string rabbitConnectionString)
        {
            _rabbitConnectionString = rabbitConnectionString;
            FibonacciProcessor = new Thread(ProcessFibonacciRequests);
            FibonacciProcessor.IsBackground = true;
            FibonacciProcessor.Start();
        }

        /// <summary>
        /// Отправка значения кленту
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="result"></param>
        /// <param name="_bus"></param>
        private static void sendResult(string guid, int result, IBus _bus)
        {
            _bus.PublishAsync<Tuple<string, int>>(new Tuple<string, int>(guid, result));
        }

        /// <summary>
        /// Обработка запросов из очереди
        /// </summary>
        private static void ProcessFibonacciRequests()
        {
            using (IBus bus = RabbitHutch.CreateBus(_rabbitConnectionString))
            {
                while (true)
                {
                    FibonacciReq fr;
                    if (_queue.TryDequeue(out fr))
                    {
                        // Здесь спим только для того чтобы замедлить работу программы - для наглядности
                        Thread.Sleep(350);

                        // Смотрим есть ли ключ в словаре для конкретной последовательности. Если нет - значит это первый запрос для конкретного расчёта
                        if(! SequenceCache.ContainsKey(fr.id))
                        {
                            SequenceCache[fr.id] = 0;
                        }

                        // Второй элемент - вычисляется не как сумма предыдущих, а равен единице (по определению послежовательности Фибоначчи)
                        if (fr.previousNumber==0)
                        {
                            sendResult(fr.id, 1, bus);
                        }
                        else
                        {
                            // Складываем число из словаря последовательностей и число, пришедшее от клиента
                            // Полученную сумму отправляем клиенту, а число, полученное от клиента в запросе - сохраняем в словарь последовательностей
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
