using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EnginePoc
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true);
            var configuration = builder.Build();

            var connection = GetConnection(configuration);
            Task.Run(() => connection.StartAsync()).Wait();

            int i = 1;
            while (true)
            {
                var message = "test message " + i++;
                Console.WriteLine(message);
                Task.Run(() => connection.InvokeAsync("SendMessage", 1, "test", message)).Wait();
                Thread.Sleep(100);
            }
        }

        public static HubConnection GetConnection(IConfigurationRoot config)
        {
            var hubUrl = config["HubUrl"];
            var connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(config["Token"]);
            })
            .Build();

            return connection;
        }
    }
}
