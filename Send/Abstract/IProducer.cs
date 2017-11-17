using RabbitMQ.Client;

namespace Send.Abstract
{
    public interface IProducer
    {
        void Send();
    }
}
