using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Recieve.Abstract;
using System;
using System.Text;
using System.Threading;

namespace Recieve.Concrete
{
    public class SubscribeConsumer : IConsumer
    {
        private void Work()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 若不存在，声明交换器
                channel.ExchangeDeclare(exchange: "msg", type: "fanout");

                // 声明一次性非持久唯一队列，获取名称
                var queueName = channel.QueueDeclare().QueueName;
                // 绑定到 Fanout 交换器
                channel.QueueBind(queue: queueName,
                                  exchange: "msg",
                                  routingKey: "");

                string threadName = Thread.CurrentThread.Name;
                Console.WriteLine(" [{0}] Waiting for msg.", threadName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [{0}] {1}", threadName, message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.ReadLine();
            }
        }

        public void Recieve()
        {
            Thread thread3 = new Thread(new ThreadStart(Work));
            thread3.Name = "线程3";
            Thread thread4 = new Thread(new ThreadStart(Work));
            thread4.Name = "线程4";

            Console.WriteLine(" Press [enter] twice to exit.");

            thread3.Start();
            thread4.Start();

            thread3.Join();
            thread4.Join();
        }
    }
}
