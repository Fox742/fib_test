using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

namespace Application1
{
    class Program
    {
        static void Main(string[] args)
        {
            int sequenceAmount = 3;

            // Считываем количество асинхронных расчётов
            if (args.Length > 0)
            {
                int amountEntered;
                if (Int32.TryParse(args[0],out amountEntered))
                {
                    sequenceAmount = amountEntered;
                }
                else
                {
                    Console.WriteLine("Неправильно введено число расчётов. Нужно ввести целое число");
                    return;
                }
            }

            // Подключем файл конфигурации, чтобы в нём прописать адрес web и rabbit служб
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();


            Console.WriteLine("                     <----- КАЖДЫЙ СТОЛБЕЦ - ОТДЕЛЬНАЯ ПОСЛЕДОВАТЕЛЬНОСТЬ ФИБОНАЧЧИ ------>");
            Console.WriteLine();
            using (FibonacciPool fp = new FibonacciPool(sequenceAmount, config["web_api_url"], config["rabbit_api_url"]))
            {
                fp.Start();
                Thread.Sleep(300000);
            }
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
