using System;
using RabbitMQ.Client;
using Send.Abstract;
using System.Text;

namespace Send.Concrete
{
    /// <summary>
    /// 工作队列生产者
    /// durable真、exchange空
    /// </summary>
    public class WorkQueueProducer : IProducer
    {
        public void Send()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "input",
                     durable: true, // 持久化队列
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                Console.WriteLine("根据发送消息结尾‘.’的个数等待秒数，发送消息q退出发送");
                while (true)
                {
                    var message = Console.ReadLine();

                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "input",   // 发送到指定持久化队列
                                         basicProperties: properties,
                                         body: body);

                    if ("q".Equals(message))
                    {
                        channel.QueueDelete("input", false, false);
                        break;
                    }
                }
            }
        }
    }
}
