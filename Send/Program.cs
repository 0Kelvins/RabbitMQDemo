using Send.Abstract;
using Send.Infrastructure;
using System;

namespace Send
{
    class Program
    {
        /// <summary>
        /// 入口
        /// </summary>
        public static void Main(string[] args)
        {
            Console.WriteLine(" 生产者类型对应key：");
            Console.WriteLine(" 1： SimpleProducer 简单的，给一个队列发送消息，一个消费者接收消息");
            Console.WriteLine(" 2： WorkQueueProducer 工作队列，发送给指定队列信息，队列上的所有消费者争夺消息");
            Console.WriteLine(" 3： PublishProducer Fanout，所有绑定在对应交换器上的队列都会收到每一条消息");
            Console.WriteLine(" 4： RoutingProducer 带路由的，发送带routingkey的消息");
            Console.WriteLine(" 5： TopicProducer 复杂的会话，发送复杂routingkey表达式的消息");
            Console.WriteLine(" 6： RPCClient Remote procedure call 远程请求的消息");

            Console.Write("Type key: ");
            string type = Console.ReadLine();

            IProducer producer = ProducerFactory.getProducer(type);
            producer.Send();

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
