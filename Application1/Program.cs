using System;
using System.Configuration;
using System.IO;
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
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            int sequenceAmount = 3;
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
