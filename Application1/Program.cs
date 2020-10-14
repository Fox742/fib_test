using System;

namespace Application1
{
    class Program
    {
        static void Main(string[] args)
        {
            int sequenceAmount = 3;
            FibonacciPool fp = new FibonacciPool(sequenceAmount);
            fp.Start();
            Console.WriteLine("Hello World!");
        }
    }
}
