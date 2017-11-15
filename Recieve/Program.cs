using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace Recieve
{
    class Program
    {
        static void recieveHello()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] Received {0}", message);
                };
                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        static void recieveInput()
        {
            string threadName = Thread.CurrentThread.Name;  // 下面内部使用了新的线程接受消息
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "input",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                Console.WriteLine(" [*] Waiting for messages.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" {0} Received {1}", threadName, message);

                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    Console.WriteLine(" {0} Done", threadName);
                };
                channel.BasicConsume(queue: "input", autoAck: false, consumer: consumer);
                
                Console.ReadLine();
            }
        }

        static void Main(string[] args)
        {
            Console.Write("Type: ");
            string type = Console.ReadLine();

            switch (type)
            {
                case "1":
                    recieveHello();
                    break;
                case "2":
                    Thread thread1 = new Thread(new ThreadStart(recieveInput));
                    thread1.Name = "线程1";
                    Thread thread2 = new Thread(new ThreadStart(recieveInput));
                    thread2.Name = "线程2";

                    thread1.Start();
                    thread2.Start();

                    Console.WriteLine(" Press [enter] twice to exit.");

                    thread1.Join();
                    thread2.Join();
                    break;
                default:
                    Console.WriteLine("Not Exist!");
                    break;
            }
        }
    }
}
