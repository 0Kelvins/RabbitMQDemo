using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Recieve.Abstract;
using System;
using System.Text;
using System.Threading;

namespace Recieve.Concrete
{
    /// <summary>
    /// 工作队列消费者
    /// </summary>
    public class WorkQueueConsumer : IConsumer
    {
        private void Work(Object o)
        {
            ConnectionFactory factory = (ConnectionFactory)o;

            // 一个线程一个连接，连接不能多线程共享
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "input",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                // prefetchSize:一次接收消息大小，0不限制
                // prefetchCount:一次接收消息数量
                // global：true\false 是否将上面设置应用于channel，简单点说，就是上面限制是channel级别的还是consumer级别
                // prefetchSize固定0，global固定false
                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                string threadName = Thread.CurrentThread.Name;  // 下面内部使用了新的线程接受消息
                Console.WriteLine(" [{0}] Waiting for messages.", threadName);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" {0} Received {1}", threadName, message);

                    int dots = message.Split('.').Length - 1;
                    Thread.Sleep(dots * 1000);

                    //deliveryTag: 该消息的index
                    //multiple：是否批量.true:将一次性拒绝所有小于deliveryTag的消息。
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    //basicNack.requeue：被拒绝的是否重新入队列
                    Console.WriteLine(" {0} Done", threadName);
                };
                channel.BasicConsume(queue: "input", autoAck: false, consumer: consumer);

                Console.ReadLine();
            }
        }

        public void Recieve()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            Thread thread1 = new Thread(new ParameterizedThreadStart(Work));
            thread1.Name = "线程1";
            Thread thread2 = new Thread(new ParameterizedThreadStart(Work));
            thread2.Name = "线程2";

            Console.WriteLine(" Press [enter] twice to exit.");

            thread1.Start(factory);
            thread2.Start(factory);

            thread1.Join();
            thread2.Join();
        }
    }
}
