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
            return "Сервис запущен! Теперь вы можете запустить Приложение 1 для вычисления последовательностей Фибоначчи";
        }

        [HttpGet("next")]
        public void Next(int n, string guid)
        {
            // Добавляем запрос в очередь на обработку
            FibonacciCalculator.Enqueue(new FibonacciReq() { id = guid, previousNumber = n });
        }
    }
}
