using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace Send
{
    class Program
    {
        /// <summary>
        /// 指定队列发送消息
        /// </summary>
        private static void sendHello(ConnectionFactory factory)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //durable：true、false true：在服务器重启时，能够存活
                //exclusive ：是否为当前连接的专用队列，在连接断开后，会自动删除该队列，生产环境中应该很少用到吧。
                //autodelete：当没有任何消费者使用时，自动删除该队列。this means that the queue will be deleted when there are no more processes consuming messages from it.
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                //routingKey：路由键，#匹配0个或多个单词，*匹配一个单词，在topic exchange做消息转发用
                //mandatory：true：如果exchange根据自身类型和消息routeKey无法找到一个符合条件的queue，那么会调用basic.return方法将消息返还给生产者。false：出现上述情形broker会直接将消息扔掉
                //immediate：true：如果exchange在将消息route到queue(s)时发现对应的queue上没有消费者，那么这条消息不会放入队列中。当与消息routeKey关联的所有queue(一个或多个)都没有消费者时，该消息会通过basic.return方法返还给生产者。
                //basicProperties ：需要注意的是BasicProperties.deliveryMode，0:不持久化 1：持久化 这里指的是消息的持久化，配合channel(durable = true),queue(durable)可以实现，即使服务器宕机，消息仍然保留
                //简单来说：mandatory标志告诉服务器至少将该消息route到一个队列中，否则将消息返还给生产者；immediate标志告诉服务器如果该消息关联的queue上有消费者，则马上将消息投递给它，如果所有queue都没有消费者，直接把消息返还给生产者，不用将消息入队列等待消费者了
                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        /// <summary>
        /// 工作队列发送
        /// </summary>
        private static void sendInput(ConnectionFactory factory)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "input",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                while (true)
                {
                    var message = Console.ReadLine();

                    var body = Encoding.UTF8.GetBytes(message);

                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                    channel.BasicPublish(exchange: "",
                                         routingKey: "input",
                                         basicProperties: properties,
                                         body: body);
                   
                    if ("q".Equals(message))
                        break;
                }
            }
        }

        /// <summary>
        /// 发布(订阅)，Fanout
        /// </summary>
        private static void sendMsg(ConnectionFactory factory)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // 声明交换器，类型为扇形
                channel.ExchangeDeclare(exchange: "msg", type: "fanout");
                
                var message = Console.ReadLine();
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "msg",
                                        routingKey: "",
                                        basicProperties: null,
                                        body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        /// <summary>
        /// 发布订阅，direct + routingKey
        /// </summary>
        private static void sendRouting(ConnectionFactory factory)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "direct_logs",
                                        type: "direct");

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

        /// <summary>
        /// Topic模式发布
        /// </summary>
        private static void sendTopic(ConnectionFactory factory)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "topic_logs",
                                        type: "topic");

                while (true)
                {
                    string[] args = Console.ReadLine().Split(' ');

                    var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                    var message = (args.Length > 1)
                                  ? string.Join(" ", args.Skip(1).ToArray())
                                  : "Hello World!";
                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish(exchange: "topic_logs",
                                         routingKey: routingKey,
                                         basicProperties: null,
                                         body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", routingKey, message);
                }
            }
        }



        public static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            Console.Write("Type: ");
            string type = Console.ReadLine();
            switch (type)
            {
                case "1":
                    sendHello(factory);
                    break;
                case "2":
                    sendInput(factory);
                    break;
                case "3":
                    sendMsg(factory);
                    break;
                case "4":
                    sendRouting(factory);
                    break;
                case "5":
                    sendTopic(factory);
                    break;
                case "6":
                    var rpcClient = new RPCClient();

                    Console.WriteLine(" [x] Requesting fib(30)");
                    var response = rpcClient.Call("30");

                    Console.WriteLine(" [.] Got '{0}'", response);
                    rpcClient.Close();
                    break;
                default:
                    Console.WriteLine("Not Exist!");
                    break;
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
