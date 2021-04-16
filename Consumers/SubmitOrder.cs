using System;

namespace Consumers
{
    public interface SubmitOrder
    {
        Guid OrderId { get; }
    }
}