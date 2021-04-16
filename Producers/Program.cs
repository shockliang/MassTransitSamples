using System.Threading;
using System.Threading.Tasks;
using Broadcast;
using Contracts;
using MassTransit;
using RabbitMQ.Client;

namespace Producers
{
    class Program
    {
        public static async Task Main()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                // Should same as binding exchange name 
                cfg.Message<ValueEntered>(x => x.SetEntityName("MyEvent"));
                // Using contracts project
                cfg.Publish<ValueEntered>(x =>
                {
                    x.BindQueue("MyEvent", "ValueEntered",
                        config =>
                        {
                            config.ExchangeType = ExchangeType.Fanout;
                        });
                });

                cfg.Message<UserKicked>(x =>
                {
                    x.SetEntityName("Broadcast");
                });
                // Using namespace only in self project. according the "Message" section of MassTransit
                cfg.Publish<UserKicked>(x =>
                {
                    
                    // cfg.UseRawJsonSerializer();
                    x.BindQueue("Broadcast", nameof(UserKicked),
                        config =>
                        {
                            config.ExchangeType = ExchangeType.Fanout;
                        });
                });
            });

            var source = new CancellationTokenSource();

            await busControl.StartAsync(source.Token);
            try
            {
                while (true)
                {
                    for (var i = 0; i < 10; i++)
                    {
                        await busControl.Publish<ValueEntered>(new
                        {
                            Value = $"{i}"
                        }, source.Token);

                        await busControl.Publish<UserKicked>(new
                        {
                            UserId = i
                        }, source.Token);
                    }

                    await Task.Delay(1 * 1000, source.Token);
                }
            }
            finally
            {
                await busControl.StopAsync(source.Token);
            }
        }
    }
}