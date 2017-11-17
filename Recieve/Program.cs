using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Recieve.Abstract;
using Recieve.Infrastructure;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Recieve
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine(" 消费者类型对应key：");
            Console.WriteLine(" 1： SimpleConsumer 消费者接收一个队列发送的消息");
            Console.WriteLine(" 2： WorkQueueConsumer 多消费者争夺指定队列消息");
            Console.WriteLine(" 3： PublishConsumer Fanout，所有绑定在对应交换器上的消费者队列都会收到每一条消息");
            Console.WriteLine(" 4： RoutingConsumer 带路由的，消费者根据routingkey接收对应消息");
            Console.WriteLine(" 5： TopicConsumer 复杂的会话，消费者根据复杂routingkey表达式接收对应消息");
            Console.WriteLine(" 6： RPCClient Remote procedure call 远程接收的消息");

            Console.Write("Type key: ");
            string type = Console.ReadLine();

            IConsumer consumer = ConsumerFactory.getConsumer(type);
            if (consumer != null)
                consumer.Recieve();
            else
                Console.WriteLine("Not Exist!");
        }
    }
}
