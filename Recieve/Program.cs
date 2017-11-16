using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Recieve
{
    class Program
    {
        /// <summary>
        /// 接收指定队列消息
        /// </summary>
        private static void recieveHello(Object o)
        {
            ConnectionFactory factory = (ConnectionFactory)o;
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

        /// <summary>
        /// 工作队列接收
        /// </summary>
        private static void recieveInput(Object o)
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

        /// <summary>
        /// 订阅（发布），Fanout
        /// </summary>
        private static void recieveMsg(Object o)
        {
            ConnectionFactory factory = (ConnectionFactory)o;

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "msg", type: "fanout");

                // 声明一次性非持久唯一队列，获取名称
                var queueName = channel.QueueDeclare().QueueName;
                // 绑定到交换器
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

        /// <summary>
        /// RoutingKey订阅
        /// </summary>
        private static void recieveRouting(Object o)
        {
            ConnectionFactory factory = (ConnectionFactory)o;

            string[] args;
            string threadName = Thread.CurrentThread.Name;

            if ("线程5".Equals(threadName))
            {
                args = new string[] { "warning", "error" };
            }
            else
            {
                args = new string[] { "info", "warning", "error" };
            }

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

        /// <summary>
        /// Topic模式订阅
        /// </summary>
        private static void recieveTopic(Object o)
        {
            ConnectionFactory factory = (ConnectionFactory)o;

            string[] args;
            string threadName = Thread.CurrentThread.Name;

            switch (threadName)
            {
                case "线程7": // '#' 匹配任意一个或多个词('.'分割的)
                    args = new string[] { "#" };
                    break;
                case "线程8": // '*' 匹配任意一个词
                    args = new string[] { "kern.*" };
                    break;
                case "线程9": // 多种匹配
                    args = new string[] { "kern.*", "*.critical" };
                    break;
                default:
                    args = new string[] { "#" };
                    break;
            }

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");
                var queueName = channel.QueueDeclare().QueueName;

                /*if (args.Length < 1)
                {
                    Console.Error.WriteLine("Usage: {0} [binding_key...]",
                                            Environment.GetCommandLineArgs()[0]);
                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                    Environment.ExitCode = 1;
                    return;
                }*/

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

        /// <summary>
        /// RPC Server
        /// </summary>
        private static void recieveServer(ConnectionFactory factory)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_queue", durable: false,
                  exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc_queue",
                  autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        int n = int.Parse(message);
                        Console.WriteLine(" [.] fib({0})", message);
                        response = fib(n).ToString();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                          basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }

        private static int fib(int n)
        {
            if (n == 0 || n == 1)
            {
                return n;
            }

            return fib(n - 1) + fib(n - 2);
        }

        public static void Main(string[] args)
        {
            Console.Write("Type: ");
            string type = Console.ReadLine();

            var factory = new ConnectionFactory() { HostName = "localhost" };

            switch (type)
            {
                case "1":
                    recieveHello(factory);
                    break;
                case "2":
                    Thread thread1 = new Thread(new ParameterizedThreadStart(recieveInput));
                    thread1.Name = "线程1";
                    Thread thread2 = new Thread(new ParameterizedThreadStart(recieveInput));
                    thread2.Name = "线程2";

                    Console.WriteLine(" Press [enter] twice to exit.");

                    thread1.Start(factory);
                    thread2.Start(factory);
                    
                    thread1.Join();
                    thread2.Join();
                    break;
                case "3":
                    Thread thread3 = new Thread(new ParameterizedThreadStart(recieveMsg));
                    thread3.Name = "线程3";
                    Thread thread4 = new Thread(new ParameterizedThreadStart(recieveMsg));
                    thread4.Name = "线程4";

                    Console.WriteLine(" Press [enter] twice to exit.");

                    thread3.Start(factory);
                    thread4.Start(factory);

                    thread3.Join();
                    thread4.Join();
                    break;
                case "4":
                    Thread thread5 = new Thread(new ParameterizedThreadStart(recieveRouting));
                    thread5.Name = "线程5";
                    thread5.Start(factory);
                    
                    Thread thread6 = new Thread(new ParameterizedThreadStart(recieveRouting));
                    thread6.Name = "线程6";
                    thread6.Start(factory);

                    Console.WriteLine(" Press [enter] twice to exit.");
                    
                    thread5.Join();
                    thread6.Join();
                    break;
                case "5":
                    Thread thread7 = new Thread(new ParameterizedThreadStart(recieveTopic));
                    thread7.Name = "线程7";
                    thread7.Start(factory);

                    Thread thread8 = new Thread(new ParameterizedThreadStart(recieveTopic));
                    thread8.Name = "线程8";
                    thread8.Start(factory);

                    Thread thread9 = new Thread(new ParameterizedThreadStart(recieveTopic));
                    thread9.Name = "线程9";
                    thread9.Start(factory);

                    Console.WriteLine(" To exit press CTRL+C");

                    thread7.Join();
                    thread8.Join();
                    thread9.Join();
                    break;
                case "6":
                    recieveServer(factory);
                    break;
                default:
                    Console.WriteLine("Not Exist!");
                    break;
            }
        }
    }
}
