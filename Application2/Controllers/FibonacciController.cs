using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciController : ControllerBase
    {

        public string Get()
        {
            return "Сервис запущен! Воспользуйтесь методом fibonacci/next?n=<int> для получения следующего числа последовательности Фибоначчи";
        }

        [HttpGet("next")]
        public string[] Next(int number)
        {
            FibonacciCalculator.Enqueue(number);
            return new string[]
            {
                "Next",
                "element",
                number.ToString()
            };
        }
    }
}
