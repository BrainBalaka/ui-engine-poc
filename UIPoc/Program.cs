using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
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

            Console.WriteLine("Enter the job queue id you want to monitor the log:");
            var jobQueueId = Console.ReadLine();
            var connection = GetConnection(configuration, jobQueueId);

            // get and display the past log for this job
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("bearer " + configuration["Token"]);
            Console.WriteLine(client.GetStringAsync($"{configuration["ApiUrl"]}/queue/{jobQueueId}/logs").Result);

            // listen for the latest log
            connection.StartAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}", task.Exception.GetBaseException());
                }
                else
                {
                    connection.On<string, string>("ReceiveMessage", (taskType, message) =>
                    {
                        Console.WriteLine(message);
                    });
                }

            }).Wait();

            Console.Read();
            connection.StopAsync();
        }

        public static HubConnection GetConnection(IConfigurationRoot config, string jobQueueId)
        {
            var hubUrl = config["HubUrl"] + "?jobQueueId=" + jobQueueId;
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
