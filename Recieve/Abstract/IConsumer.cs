using RabbitMQ.Client;

namespace Recieve.Abstract
{
    public interface IConsumer
    {
        void Recieve();
    }
}
