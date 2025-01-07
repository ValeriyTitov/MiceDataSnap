using DAC.DataService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace DAC.QueueManager
{
    public class QueueManagerClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        private readonly IBasicProperties props;

        public static QueueManagerClient DefaultInstance = new QueueManagerClient();

        public QueueManagerClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            props = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();

            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            props.Headers = new Dictionary<string, object>();
            props.Headers.Add("Checksum", "-1");
                            

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    respQueue.Add(response);
                }
            };

            channel.BasicConsume( consumer, queue: replyQueueName,  autoAck: true);
        }

        public string Call(string DataRequest)
        {
            var messageBytes = Encoding.UTF8.GetBytes(DataRequest);
            props.Headers["Checksum"] = TDataSecurity.HashWithSalt(DataRequest);

            channel.BasicPublish(exchange: "", routingKey: "MiceQueue", basicProperties: props, body: messageBytes);
            return respQueue.Take();
        }

        public void Close()
        {
            connection.Close();
        }
    }
}