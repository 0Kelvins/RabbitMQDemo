using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Recieve.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recieve.Concrete
{
    /// <summary>
    /// 复杂路由消费者
    /// </summary>
    public class TopicConsumer : IConsumer
    {
        private void Work(Object o)
        {
            string[] args = (string[])o;
            var factory = new ConnectionFactory() { HostName = "localhost" };

            string threadName = Thread.CurrentThread.Name;

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                #region 未输入启动参数
                /*if (args.Length < 1)
                {
                    Console.Error.WriteLine("Usage: {0} [binding_key...]",
                                            Environment.GetCommandLineArgs()[0]);
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;
                }*/
                #endregion

                foreach (var bindingKey in args)
                {
                    channel.QueueBind(queue: queueName,
                                      exchange: "topic_logs",
                                      routingKey: bindingKey);
                }

                Console.WriteLine(" [{0}] Waiting for messages. ", threadName);

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
            Console.WriteLine("线程7：#");
            Console.WriteLine("线程8：kern.*");
            Console.WriteLine("线程9：kern.* , *.critical");

            Thread thread7 = new Thread(new ParameterizedThreadStart(Work));
            thread7.Name = "线程7";
            thread7.Start(new string[] { "#" });

            Thread thread8 = new Thread(new ParameterizedThreadStart(Work));
            thread8.Name = "线程8";
            thread8.Start(new string[] { "kern.*" });

            Thread thread9 = new Thread(new ParameterizedThreadStart(Work));
            thread9.Name = "线程9";
            thread9.Start(new string[] { "kern.*", "*.critical" });

            Console.WriteLine(" To exit press CTRL+C");

            thread7.Join();
            thread8.Join();
            thread9.Join();
        }
    }
}
