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
                Thread.Sleep(30000);
            }
            Console.WriteLine("Hello World!");
        }
    }
}
