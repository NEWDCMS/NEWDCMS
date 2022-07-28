using DCMS.Core.Configuration;
using EasyNetQ;
using EasyNetQ.Topology;
using System;


namespace DCMS.Core.RabbitMQ
{
    public class MQFactory
    {
        public static RabbitMQHelper Use(DCMSConfig config) => new RabbitMQHelper(config);
        public static RabbitMQHelper Use(string conn) => new RabbitMQHelper(conn);
    }


    public class RabbitMQHelper
    {

        private readonly string _connectionString;

        public RabbitMQHelper(DCMSConfig config)
        {
            if (string.IsNullOrEmpty(config.RabbitMQConnectionString))
            {
                throw new Exception("RabbitMQ connection string is empty");
            }

            _connectionString = config.RabbitMQConnectionString;
        }

        public RabbitMQHelper(string RabbitMQConnectionString)
        {
            if (string.IsNullOrEmpty(RabbitMQConnectionString))
            {
                throw new Exception("RabbitMQ connection string is empty");
            }

            _connectionString = RabbitMQConnectionString;
        }


        public IBus CreateMessageBus()
        {
            // 消息服务器连接字符串
            if (_connectionString == null || _connectionString == string.Empty)
            {
                throw new Exception($"RabbitMQ connection not exist,  messageserver connection string is missing or empty");
            }

            return RabbitHutch.CreateBus(_connectionString);
        }

        #region 

        /// <summary>
        /// 创建队列
        /// </summary>
        /// <param name="adbus"></param>
        /// <param name="queueName"></param>
        /// <returns></returns>
        private IQueue CreateQueue(IAdvancedBus adbus, string queueName = "")
        {
            if (adbus == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(queueName))
            {
                return adbus.QueueDeclare();
            }
            //QueueDeclare(string name, bool passive = false, bool durable = true, bool exclusive = false, bool autoDelete = false, int? perQueueMessageTtl = null, int? expires = null, int? maxPriority = null, string deadLetterExchange = null, string deadLetterRoutingKey = null, int? maxLength = null, int? maxLengthBytes = null);
            //队列持久化
            return adbus.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);
        }


        #endregion

        #region 用于扇形交换

        /// <summary>
        ///  消息消耗（fanout）
        /// </summary>`
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="handler">回调</param>
        /// <param name="exChangeName">交换器名</param>
        /// <param name="queueName">队列名</param>
        /// <param name="routingKey">路由名</param>
        public void FanoutConsume<T>(Action<T> handler, string exChangeName = "fanout_mq", string queueName = "fanout_queue_default", string routingKey = "") where T : class
        {
            var bus = CreateMessageBus();
            var adbus = bus.Advanced;
            var exchange = adbus.ExchangeDeclare(exChangeName, ExchangeType.Fanout);
            var queue = CreateQueue(adbus, queueName);
            adbus.Bind(exchange, queue, routingKey);
            adbus.Consume(queue, registration =>
            {
                //registration.Add<T>((message, info) =>
                //{
                //    handler(message.Body);
                //});
            });
        }
        /// <summary>
        /// 消息上报（fanout）
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="topic">主题名</param>
        /// <param name="t">消息命名</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public bool FanoutPush<T>(T t, out string msg, string exChangeName = "fanout_mq", string routingKey = "") where T : class
        {
            msg = string.Empty;
            try
            {
                using (var bus = CreateMessageBus())
                {
                    var adbus = bus.Advanced;
                    var exchange = adbus.ExchangeDeclare(exChangeName, ExchangeType.Fanout);
                    adbus.Publish(exchange, routingKey, false, new Message<T>(t));
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }
        }
        #endregion

        #region  用于直连交换
        /// <summary>
        /// 消息发送（direct）
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="queue">发送到的队列</param>
        /// <param name="message">发送内容</param>
        public void DirectSend<T>(string queue, T message) where T : class
        {
            using (var bus = CreateMessageBus())
            {
                //bus.Send(queue, message);
            }
        }
        /// <summary>
        /// 消息接收（direct）
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="queue">接收的队列</param>
        /// <param name="callback">回调操作</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public bool DirectReceive<T>(string queue, Action<T> callback, out string msg) where T : class
        {
            msg = string.Empty;
            try
            {
                var bus = CreateMessageBus();
                //bus.Receive<T>(queue, callback);
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 消息发送
        /// <![CDATA[（direct EasyNetQ高级API）]]>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="msg"></param>
        /// <param name="exChangeName"></param>
        /// <param name="routingKey"></param>
        /// <returns></returns>
        public bool DirectPush<T>(T t, out string msg, string exChangeName = "direct_mq", string queueName = "message", string routingKey = "direct_rout_default") where T : class
        {
            msg = string.Empty;
            try
            {
                using (var bus = CreateMessageBus())
                {
                    var adbus = bus.Advanced;

                    //(string name, string type, bool passive = false, bool durable = true, bool autoDelete = false, bool @internal = false, string alternateExchange = null, bool delayed = false);

                    //Exchange持久化
                    var exchange = adbus.ExchangeDeclare(exChangeName, ExchangeType.Direct, durable: true, autoDelete: false);

                    //申明message 队列
                    var queue = adbus.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

                    //以下是消息持久化
                    /*
                     MQP.BasicProperties.Builder builder = new AMQP.BasicProperties.Builder();
                    builder.deliveryMode(2);
                    AMQP.BasicProperties properties = builder.build();
                    channel.basicPublish("exchange.persistent", "persistent",properties, "persistent_test_message".getBytes());

                    设置消息持久化，需要设置basicProperties的DeliveryMode=2 (Non-persistent (1) or persistent (2)).

                    //发送消息
                    ch.BasicPublish("", queueName, null, msgBytes);

                      void Publish<T>(IExchange exchange, string routingKey, bool mandatory, IMessage<T> message) where T : class;
                      void Publish(IExchange exchange, string routingKey, bool mandatory, MessageProperties messageProperties, byte[] body);
                     */

                    //绑定队列到交换并映射路由
                    adbus.Bind(exchange, queue, routingKey);

                    var basicProperties = new MessageProperties
                    {
                        //这里的deliveryMode=1代表不持久化，deliveryMode=2代表持久化。
                        DeliveryMode = 2
                    };
                    adbus.Publish(exchange, routingKey, false, new Message<T>(t, basicProperties));

                    /*
                     
                    //申明direct类型的direct_mq交换
                    model.ExchangeDeclare(
                        exchange: "direct_mq",
                        type: "direct",
                        durable: true,
                        autoDelete: false,
                        arguments: null);

                    //申明message 队列
                    model.QueueDeclare(
                        queue: "message",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
                    
                    //绑定队列到交换并映射路由
                    model.QueueBind(
                        queue: "message", 
                        exchange: "direct_mq",
                        routingKey: "13002929017");


                    
                        model.BasicPublish(exchange: "direct_mq",
                                           routingKey: "13002929017",
                                           basicProperties: props,
                                           body: messageBodyBytes);

                     */
                    //adbus.PublishAsync(exchange, routingKey, false, new Message<T>(t, basicProperties))
                    //    .ContinueWith(task =>
                    //{
                    //    // this only checks that the task finished
                    //    // IsCompleted will be true even for tasks in a faulted state
                    //    // we use if (task.IsCompleted && !task.IsFaulted) to check for success
                    //    if (task.IsCompleted)
                    //    {
                    //        //Console.Out.WriteLine("{0} Completed", count);
                    //    }

                    //    if (task.IsFaulted)
                    //    {
                    //        //Client Command Dispatcher Thread
                    //        Console.Out.WriteLine("\n\n");
                    //        Console.Out.WriteLine(task.Exception);
                    //        Console.Out.WriteLine("\n\n");
                    //    }
                    //});


                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 消息接收
        ///  <![CDATA[（direct EasyNetQ高级API）]]>
        /// </summary>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="handler">回调</param>
        /// <param name="exChangeName">交换器名</param>
        /// <param name="queueName">队列名</param>
        /// <param name="routingKey">路由名</param>
        public bool DirectConsume<T>(Action<T> handler, out string msg, string exChangeName = "direct_mq", string queueName = "direct_queue_default", string routingKey = "direct_rout_default") where T : class
        {
            msg = string.Empty;
            try
            {
                var bus = CreateMessageBus();
                var adbus = bus.Advanced;

                //Exchange持久化
                var exchange = adbus.ExchangeDeclare(exChangeName, ExchangeType.Direct, durable: true);
                var queue = CreateQueue(adbus, queueName);

                adbus.Bind(exchange, queue, routingKey);
                //adbus.Bind(exchange, queue, "Consumer2");
                //  IDisposable Consume(IQueue queue, Action<IHandlerRegistration> addHandlers, Action<IConsumerConfiguration> configure);

                //消费者配置
                //Action<IConsumerConfiguration> configure = new Action<IConsumerConfiguration>(c =>
                //{
                //});

                //var orderEvents = new List<IMessage<OrderEvent>>();
                //var positionEvents = new List<IMessage<PositionEvent>>();

                adbus.Consume(queue, registration =>
                {
                    //registration
                    //.Add<OrderEvent>((message, info) => orderEvents.Add(message))
                    //.Add<PositionEvent>((message, info) => positionEvents.Add(message));

                    //registration.Add<T>((message, info) =>
                    //{
                    //    handler(message.Body);
                    //});
                });
                //adbus.Consume(queue, registration =>
                //{
                //    registration.Add<string>((message, info) =>
                //    {
                //        System.Diagnostics.Trace.WriteLine("接收到消息【{0}】", message.Body);
                //    });
                //});


                //orderEvents.ForEach(orderEventMessage =>
                //      queue.Acknowledge(orderEventMessage))
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }

            return true;
        }
        #endregion

        #region 用于主题交换

        /// <summary>
        /// 获取主题 
        /// </summary>
        /// <typeparam name="T">主题内容类型</typeparam>
        /// <param name="subscriptionId">订阅者ID</param>
        /// <param name="callback">消息接收响应回调</param>
        ///  <param name="topics">订阅主题集合</param>
        public void TopicSubscribe<T>(string subscriptionId, Action<T> callback, params string[] topics) where T : class
        {
            var bus = CreateMessageBus();
            //bus.Subscribe(subscriptionId, callback, (config) =>
            //{
            //    foreach (var item in topics)
            //    {
            //        config.WithTopic(item);
            //    }
            //});
        }
        /// <summary>
        /// 发布主题
        /// </summary>
        /// <typeparam name="T">主题内容类型</typeparam>
        /// <param name="topic">主题名称:routingKey</param>
        /// <param name="message">主题内容</param>
        /// <param name="msg">错误信息</param>
        /// <returns></returns>
        public bool TopicPublish<T>(T message, string topic, out string msg) where T : class
        {
            msg = string.Empty;
            try
            {
                using (var bus = CreateMessageBus())
                {
                    //bus.Publish(message, topic);
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }
        }
        /// <summary>
        /// 发布主题
        /// </summary>
        /// <![CDATA[（topic EasyNetQ高级API）]]>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="t">消息内容</param>
        /// <param name="topic">主题名:routingKey</param>
        /// <param name="msg">错误信息</param>
        /// <param name="exChangeName">交换器名</param>
        /// <returns></returns>
        public bool TopicPublish<T>(T t, string topic, out string msg, string exChangeName = "amq.topic") where T : class
        {
            msg = string.Empty;
            try
            {
                //        //创建MQ连接
                //        using (var connection = factory.CreateConnection())
                //        using (var channel = connection.CreateModel())
                //        {
                //            //绑定交换器
                //            channel.ExchangeDeclare(exchange: $"topic/{userId}", type: "topic");
                //            var body = Encoding.UTF8.GetBytes(message);
                //            //对指定routingkey发送内容
                //            channel.BasicPublish(exchange: "amq.topic",
                //                                routingKey: userId,
                //                                basicProperties: null,
                //                                body: body);
                //        }

                if (string.IsNullOrWhiteSpace(topic))
                {
                    throw new Exception("推送主题不能为空");
                }

                using (var bus = CreateMessageBus())
                {
                    var adbus = bus.Advanced;
                    //var queue = adbus.QueueDeclare("user.notice.xx");
                    //amq.topic
                    var exchange = adbus.ExchangeDeclare(exChangeName, ExchangeType.Topic);
                    adbus.Publish(exchange, topic, false, new Message<T>(t));
                    return true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 获取主题 
        /// </summary>
        /// <![CDATA[（topic EasyNetQ高级API）]]>
        /// <typeparam name="T">消息类型</typeparam>
        /// <param name="subscriptionId">订阅者ID</param>
        /// <param name="callback">回调</param>
        /// <param name="exChangeName">交换器名</param>
        /// <param name="topics">主题名</param>
        public void TopicConsume<T>(Action<T> callback, string exChangeName = "amq.topic", string subscriptionId = "topic_subid", params string[] topics) where T : class
        {
            var bus = CreateMessageBus();
            var adbus = bus.Advanced;
            var exchange = adbus.ExchangeDeclare(exChangeName, ExchangeType.Topic);
            var queue = adbus.QueueDeclare(subscriptionId);
            foreach (var item in topics)
            {
                adbus.Bind(exchange, queue, item);
            }

            adbus.Consume(queue, registration =>
            {
                //registration.Add<T>((message, info) =>
                //{
                //    callback(message.Body);
                //});
            });
        }



        #endregion

    }
}
