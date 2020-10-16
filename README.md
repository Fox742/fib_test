# fib_test
Задание кандидату на вакансию разработчика C#  
(Fibonacci)  
#### Задание
Два приложения общаются друг с другом через транспорт, реализуя расчет чисел Фибоначчи.  
  
Логика расчета одной последовательности такая:  
Первое инициализирует расчет.  
Первое отправляет второму N(i)  
Второе вычисляет N(i-1) + N(i) и шлет обратно  
Логика повторяется симметрично.  
И так до остановки приложений.  
#### Особенности  
Первое приложение при старте получает параметр – целое число, сколько асинхронных расчетов начать. Все расчеты
идут параллельно.  
Передача данных от 1 к 2 идет через Rest WebApi  
Передача данных от 2 к 1 идет посредством MessageBus.  
Язык C#, среда MS .NET Framework версии от 4.0.  
#### Рекомендуемые технологии  
REST: ASP.NET WebApi + HttpClient  
MessageBus: RabbitMQ + EasyNetQ  
### Реализация
Приложение1 (Application1) - Консольное приложение .NET Core  
Приложение2 (Application2) - Приложение ASP.NET Core WebAPI  
#### Настройка
### Приложение1 (Application1)
Файл appsettings.json содержит два ключа:
`"web_api_url"` - адрес хоста web-службы
`"rabbit_api_url"` - connection string шины rabbitMQ
### Приложение2 (Application2)
Файл appsettings.json содержит ключ:
`"rabbit_connection_string"` - connection string шины rabbitMQ
