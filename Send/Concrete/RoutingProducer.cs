using Send.Abstract;
using System;
using RabbitMQ.Client;
using System.Text;
using System.Linq;

namespace Send.Concrete
{
    /// <summary>
    /// 路由生产者
    /// direct、routingkey
    /// </summary>
    public class RoutingProducer : IProducer
    {
        public void Send()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 直达类型交换器
                channel.ExchangeDeclare(exchange: "direct_logs", type: "direct");

                Console.WriteLine(" [*] Waiting for logs. To exit press CTRL+C");
                while (true)
                {
                    string[] args = Console.ReadLine().Split(' ');

                    // log level
                    var severity = (args.Length > 0) ? args[0] : "info";

                    // log msg
                    var message = (args.Length > 1)
                                  ? string.Join(" ", args.Skip(1).ToArray())
                                  : "Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "direct_logs",
                                         routingKey: severity,  // 按照输入设置日志等级作为routingkey
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
                }
            }
        }
    }
}
