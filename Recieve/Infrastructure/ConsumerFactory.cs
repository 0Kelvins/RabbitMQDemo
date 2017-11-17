using Recieve.Abstract;
using Recieve.Concrete;

namespace Recieve.Infrastructure
{
    public class ConsumerFactory
    {
        public static IConsumer getConsumer(string key)
        {
            switch(key)
            {
                case "1":
                    return new SimpleConsumer();
                case "2":
                    return new WorkQueueConsumer();
                case "3":
                    return new SubscribeConsumer();
                case "4":
                    return new RoutingConsumer();
                case "5":
                    return new TopicConsumer();
                case "6":
                    return new RPCServer();
                default:
                    return null;
            }
        }
    }
}
