using DCMS.Core.Configuration;
using EasyNetQ;
using System;


namespace DCMS.Core.RabbitMQ
{
    /// <summary>
    /// 消息服务器连接器
    /// </summary>
    public class BusBuilder
    {
        private readonly string _connectionString;

        public BusBuilder(DCMSConfig config)
        {
            if (string.IsNullOrEmpty(config.RabbitMQConnectionString))
            {
                throw new Exception("RabbitMQ connection string is empty");
            }

            _connectionString = config.RabbitMQConnectionString;
        }

        public BusBuilder(string RabbitMQConnectionString)
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
    }
}
