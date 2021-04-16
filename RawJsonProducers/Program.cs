using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RawJsonProducers
{
    class Program
    {
        private const string ExchangeName = "RawJsonEvent.Fx";
        private const string QueueName = "RawJsonQueue";
        
        public static async Task Main()
        {
            var source = new CancellationTokenSource();
            var factory = new ConnectionFactory();

            try
            {
                using var conn = factory.CreateConnection();
                using var channel = conn.CreateModel();
                channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
                channel.QueueDeclare(QueueName, true, false, false, null);
                channel.QueueBind(QueueName, ExchangeName, "", null);

                while (true)
                {
                    for (var i = 0; i < 10; i++)
                    {
                        var regularEvent = new SomeRegularEventHappened
                        {
                            Value = $"Event-{i}-{Guid.NewGuid()}"
                        };
                        var json = JsonConvert.SerializeObject(regularEvent);
                        var body = Encoding.UTF8.GetBytes(json);
                        channel.BasicPublish(ExchangeName, "", null, body);
                    }

                    await Task.Delay(1 * 1000, source.Token);
                }
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.ToString());
            }
        }
    }

    public class SomeRegularEventHappened
    {
        public string Value { get; set; }
    }
}