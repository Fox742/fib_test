using System;
using System.Threading;

namespace Application1
{
    class Program
    {
        static void Main(string[] args)
        {
            int sequenceAmount = 3;
            using (FibonacciPool fp = new FibonacciPool(sequenceAmount))
            {
                fp.Start();
                Thread.Sleep(300000);
            }
            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
