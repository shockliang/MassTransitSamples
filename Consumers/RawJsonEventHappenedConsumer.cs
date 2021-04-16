using System;
using System.Threading.Tasks;
using MassTransit;

namespace Consumers
{
    public class RawJsonEventHappenedConsumer : IConsumer<RawJsonEventHappened>
    {
        public Task Consume(ConsumeContext<RawJsonEventHappened> context)
        {
            var document = context.Message.Value;
            throw new Exception($"Some error {document}");
            return Task.CompletedTask;
        }
    }
}