using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Broadcast;
using Contracts;
using MassTransit;
using MassTransit.MessageData;
using RabbitMQ.Client;

namespace Consumers
{
    class Program
    {
        public static async Task Main()
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Message<ValueEntered>(x => x.SetEntityName("MyEvent"));
                // Using contracts project.
                cfg.ReceiveEndpoint(nameof(ValueEntered), e =>
                {
                    // Create 'MyEvent' exchange if exchange doesn't exist.
                    e.Bind("MyEvent");
                    // e.Bind<ValueEntered>();
                    e.PrefetchCount = 1;
                    
                    // delegate consumer factory
                    // e.Consumer(() => new SubmitOrderConsumer());
                    e.Consumer(() => new ValueEnteredConsumer(Console.Out));
                    
                    // another delegate consumer factory, with dependency
                    // e.Consumer(() => new LogOrderSubmittedConsumer(Console.Out));

                    // a type-based factory that returns an object (specialized uses)
                    // var consumerType = typeof(SubmitOrderConsumer);
                    // e.Consumer(consumerType, type => Activator.CreateInstance(consumerType));
                });
                
                cfg.Message<UserKicked>(x => x.SetEntityName("Broadcast"));
                // Using namespace of self project.
                cfg.ReceiveEndpoint(nameof(UserKicked), e =>
                {
                    e.PrefetchCount = 1;
                    e.Consumer(() => new UserKickedConsumer());
                });
                
                // Consume the raw json way.
                // cfg.Message<RawJsonEventHappenedConsumer>(x => x.SetEntityName("RawJsonEvent.Fx"));
                cfg.ReceiveEndpoint("RawJsonQueue", e =>
                {
                    // Disable for using the CustomerType property.
                    e.ConfigureConsumeTopology = false;
                    e.Bind("RawJsonEvent.Fx", x =>
                    {
                        x.ExchangeType = ExchangeType.Fanout;
                    });
                    

                    // Using default raw json deserializer
                    e.ClearMessageDeserializers();
                    e.UseRawJsonSerializer();
 
                    e.Consumer<RawJsonEventHappenedConsumer>();
                });
            });

            var source = new CancellationTokenSource();
            await busControl.StartAsync(source.Token);

            try
            {
                while (true)
                {
                    await Task.Delay(1000, source.Token);
                }
            }
            finally
            {
                await busControl.StopAsync(source.Token);
            }
        }
    }

    internal class UserKickedConsumer : IConsumer<UserKicked>
    {
        public UserKickedConsumer()
        {
        }

        public Task Consume(ConsumeContext<UserKicked> context)
        {
            Console.WriteLine($"User kicked:{context.Message.UserId}");
            return Task.CompletedTask;
        }
    }
    
    public class ValueEnteredConsumer : IConsumer<ValueEntered>
    {
        readonly TextWriter _writer;

        public ValueEnteredConsumer(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Consume(ConsumeContext<ValueEntered> context)
        {
            try
            {
                await _writer.WriteLineAsync($"Consumer receive ValueEntered event {context.Message.Value}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }

    class LogOrderSubmittedConsumer :
        IConsumer<SubmitOrder>
    {
        readonly TextWriter _writer;

        public LogOrderSubmittedConsumer(TextWriter writer)
        {
            _writer = writer;
        }

        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            await _writer.WriteLineAsync($"Order submitted: {context.Message.OrderId}");
        }
    }

    class SubmitOrderConsumer :
        IConsumer<SubmitOrder>
    {
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            Console.WriteLine("Consumer receive SubmitOrder event");
            await context.Publish<SubmitOrder>(new
            {
                context.Message.OrderId
            });
        }
    }
}