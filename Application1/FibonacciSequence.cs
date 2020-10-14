using System;
using System.Collections.Generic;
using System.Text;

namespace Application1
{
    public class FibonacciSequence
    {
        public int Current { set; get; }
        public bool Waiting { set; get; }


        public FibonacciSequence()
        {
            Current = 0;
        }

    }
}
