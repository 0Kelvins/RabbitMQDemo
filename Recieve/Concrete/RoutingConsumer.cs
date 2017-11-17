using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Recieve.Abstract;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Recieve.Concrete
{
    public class RoutingConsumer : IConsumer
    {
        public void Work(Object o)
        {
            string[] args = (string[])o;
            var factory = new ConnectionFactory() { HostName = "localhost" };

            string threadName = Thread.CurrentThread.Name;

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");
                var queueName = channel.QueueDeclare().QueueName;

                /*if (args == null || args.Length < 1)
                {
                    Console.Error.WriteLine("Usage: [info] [warning] [error]");

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;
                }*/

                // 绑定对应路由
                foreach (var severity in args)
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: "direct_logs",
                                      routingKey: severity);
                }

                Console.WriteLine(threadName + " " + string.Join(" ", args.ToArray()));

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var routingKey = ea.RoutingKey;
                    Console.WriteLine(" [{0}] Received '{1}':'{2}'",
                                      threadName, routingKey, message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.ReadLine();
            }
        }

        public void Recieve()
        {
            Thread thread5 = new Thread(new ParameterizedThreadStart(Work));
            thread5.Name = "线程5";
            thread5.Start(new string[] { "warning", "error" });

            Thread thread6 = new Thread(new ParameterizedThreadStart(Work));
            thread6.Name = "线程6";
            thread6.Start(new string[] { "info", "warning", "error" });

            Console.WriteLine(" Press [enter] twice to exit.");

            thread5.Join();
            thread6.Join();
        }
    }
}
