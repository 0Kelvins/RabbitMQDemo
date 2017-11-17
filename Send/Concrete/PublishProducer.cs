using Send.Abstract;
using System;
using RabbitMQ.Client;
using System.Text;

namespace Send.Concrete
{
    /// <summary>
    /// 发布（订阅）生产者
    /// Exchange、Fanout
    /// </summary>
    public class PublishProducer : IProducer
    {
        public void Send()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 声明交换器，类型为扇形
                channel.ExchangeDeclare(exchange: "msg", type: "fanout");

                var message = Console.ReadLine();
                var body = Encoding.UTF8.GetBytes(message);

                // 发布信息
                channel.BasicPublish(exchange: "msg",   // 指定交换机
                                     routingKey: "",    // 不用指定路由
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }
        }
    }
}
