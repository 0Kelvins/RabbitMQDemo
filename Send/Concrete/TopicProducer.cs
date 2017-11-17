using Send.Abstract;
using System;
using RabbitMQ.Client;
using System.Text;
using System.Linq;

namespace Send.Concrete
{
    /// <summary>
    /// 会话生产者，有选择的发布（订阅）
    /// topic、灵活
    /// </summary>
    public class TopicProducer : IProducer
    {
        public void Send()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 声明topic类型交换器
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");

                while (true)
                {
                    string[] args = Console.ReadLine().Split(' ');

                    var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                    var message = (args.Length > 1)
                                  ? string.Join(" ", args.Skip(1).ToArray())
                                  : "Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);

                    // 发布信息
                    channel.BasicPublish(exchange: "topic_logs",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
                }
            }
        }
    }
}
