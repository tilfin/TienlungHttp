# TienlungHttp
C# HTTP Server Library

## Try to run HTTP File server

```csharp
using System;

namespace ConsoleHTTPApp
{
    class Program {
        static void Main(string[] args) {
            var service = new TienlungHttp.HttpService(8080, @"C:\Web\DocumentRoot");
            service.Start();
            Console.ReadLine();
            service.Stop();
        }
    }
}
```
