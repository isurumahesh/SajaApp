// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Saj.Az204;
using StackExchange.Redis;
using System;

class Program
{
    static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection();
        //serviceProvider.AddSingleton<IConnectionMultiplexer>(x => ConnectionMultiplexer.Connect(builder.Configuration.GetSection("Redis:ConnectionString").Value.ToString()));

        Console.WriteLine("Hello, World!");
      

        var obj=new ExceptionTest();

        obj.Test();

        try
        {
            int [] numbers = [1, 2, 3];
            List<int> numbers2 = [1, 2];

            HelloWorld();
        }
        catch (Exception)
        {

            Console.WriteLine("Error");
        }
       

        Console.WriteLine("Completed");
    }


    public static string HelloWorld() => "Hello World";
    
}
