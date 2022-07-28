namespace DCMS.Core.RabbitMQ
{
    //public class Producer 
    //{
    //    //private readonly string _connectionString;
    //    //public Producer(DCMSConfig config)
    //    //{
    //    //    if (string.IsNullOrEmpty(config.RabbitMQConnectionString))
    //    //        throw new Exception("RabbitMQ connection string is empty");

    //    //    _connectionString = config.RabbitMQConnectionString;
    //    //}

    //    //private ConnectionFactory ConnectBulider()
    //    //{
    //    //    try
    //    //    {
    //    //        var dicts = CommonHelper.GetFormData(_connectionString, ';');
    //    //        var factory = new ConnectionFactory()
    //    //        {
    //    //            HostName = dicts.ContainsKey("HostName") ? dicts["HostName"] : "localhost",
    //    //            Port = dicts.ContainsKey("Port") ? int.Parse(dicts["Port"]) : 0,
    //    //            UserName = dicts.ContainsKey("UserName") ? dicts["UserName"] : "guest",
    //    //            Password = dicts.ContainsKey("Password") ? dicts["Password"] : "guest",
    //    //            VirtualHost = dicts.ContainsKey("VirtualHost") ? dicts["VirtualHost"] : "/",
    //    //        };
    //    //        return factory;
    //    //    }
    //    //    catch (Exception ex)
    //    //    {
    //    //        throw new Exception($"RabbitMQ Init Exception:{ex.Message}");
    //    //    }
    //    //}


    //    //private IConnection BuildConnection()
    //    //{
    //    //    //ConnectionFactory factory = new ConnectionFactory
    //    //    //{
    //    //    //    UserName = "sa",
    //    //    //    Password = "dcms.1",
    //    //    //    VirtualHost = "APPNotices",
    //    //    //    HostName = "172.16.162.230",
    //    //    //    Port = 5672,
    //    //    //    AutomaticRecoveryEnabled = true
    //    //    //};

    //    //    ConnectionFactory factory = ConnectBulider();

    //    //    IConnection conn = null;
    //    //    int retryCount = 0;
    //    //    do
    //    //    {

    //    //        try
    //    //        {
    //    //            conn = factory.CreateConnection();
    //    //        }
    //    //        catch (BrokerUnreachableException rootException) when (DecomposeExceptionTree(rootException).Any(it => it is ConnectFailureException && (it?.InnerException?.Message?.Contains("Connection refused") ?? false)))
    //    //        {
    //    //            System.Threading.Thread.Sleep(TimeSpan.FromSeconds((retryCount + 1) * 2));
    //    //        }
    //    //        catch
    //    //        {
    //    //            throw;
    //    //        }

    //    //    } while (conn == null && ++retryCount <= 5);
    //    //    if (conn == null)
    //    //        throw new InvalidOperationException($"在{retryCount}次尝试后无法连接到RabbitMQ.");
    //    //    return conn;
    //    //}

    //    ///// <summary>
    //    ///// 推送消息
    //    ///// </summary>
    //    ///// <param name="userId"></param>
    //    ///// <param name="message"></param>
    //    //public bool PushTopic(string userId, string message)
    //    //{
    //    //    var factory = ConnectBulider();
    //    //    if (factory == null)
    //    //        return false;

    //    //    try
    //    //    {
    //    //        //创建MQ连接
    //    //        using (var connection = factory.CreateConnection())
    //    //        using (var channel = connection.CreateModel())
    //    //        {
    //    //            //绑定交换器
    //    //            channel.ExchangeDeclare(exchange: $"topic/{userId}", type: "topic");
    //    //            var body = Encoding.UTF8.GetBytes(message);
    //    //            //对指定routingkey发送内容
    //    //            channel.BasicPublish(exchange: "amq.topic",
    //    //                                routingKey: userId,
    //    //                                basicProperties: null,
    //    //                                body: body);
    //    //        }

    //    //        return true;
    //    //    }
    //    //    catch (Exception)
    //    //    {
    //    //        return false;
    //    //    }
    //    //}

    //}
}
