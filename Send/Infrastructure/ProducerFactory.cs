using Send.Abstract;
using Send.Concrete;
using System.Collections;

namespace Send.Infrastructure
{
    public class ProducerFactory
    {
        public static IProducer getProducer(string key)
        {
            switch (key)
            {
                case "1":
                    return new SimpleProducer();
                case "2":
                    return new WorkQueueProducer();
                case "3":
                    return new PublishProducer();
                case "4":
                    return new RoutingProducer();
                case "5":
                    return new TopicProducer();
                case "6":
                    return new RPCClient();
                default:
                    return null;
            }
        }
    }
}
