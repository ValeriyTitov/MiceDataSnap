using DAC.DataService;
using DAC.DataService.DocFlow;
using DAC.ObjectModels;
using DAC.XDataSet;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace DAC.QueueManager
{
    public class RPCServer
    {
        private void ValidateCheckSum(string Body, string MessageCheckSum)
        {
            string ThisCheckSum = TDataSecurity.HashWithSalt(Body);
            Console.WriteLine("Checksum for message is: " + MessageCheckSum);
            Console.WriteLine("ThisCheckSum for message is: " + ThisCheckSum);
            if (MessageCheckSum.Equals(ThisCheckSum) == false)
                throw new Exception("Invalid checksum for message");
        }
        private void HoldConsole()
        {
            while (true)
            {
                var s = Console.ReadLine();
                if (s.ToLower() == "cls" || s.ToLower() == "clear")
                    Console.Clear();
            }
        }
        private TBasicDataClass FindDataClass(string ProviderName)
        {
            TBasicDataClass Result;
            if (ProviderName == "sysdf_DocumentPush")
                Result = new sysdf_DocumentPush();
            else
                if (ProviderName == "sysdf_DocumentRollBack")
                Result = new sysdf_DocumentRollBack();
            else
            {
                throw new Exception("Class not found for " + ProviderName);
            }

            return Result;
        }
        private string ProcessRequest(string JsonRequest)
        {
            TMiceDataRequest MiceRequest = JsonConvert.DeserializeObject<TMiceDataRequest>(JsonRequest);
            Console.WriteLine(DateTime.Now.ToString() + " " + MiceRequest.ExecutionContext.ProviderName);

            if (TDataSecurity.AllowedToExecute(MiceRequest, User) == false)
                throw new Exception("You are not allowed to perform this type of queries");

            TBasicDataClass DataClass = FindDataClass(MiceRequest.ExecutionContext.ProviderName);

            TMiceDataResponse MiceDataResponse;
            MiceDataResponse = DataClass.Run(MiceRequest, User).ToNewMiceDataResponse();
            MiceDataResponse.ExecutionContext.Status = (int)TCommandStatus.etDataScript; ; //Source : DataScript

            var Result = MiceDataResponse.ToString();
            return Result;
        }
        private void OnReceiveMessage(object Sender, BasicDeliverEventArgs ea, IModel channel)
        {
            string Response = null;

            var body = ea.Body.ToArray();
            var props = ea.BasicProperties;
            var replyProps = channel.CreateBasicProperties();
            replyProps.CorrelationId = props.CorrelationId;
            
            string CheckSum = Encoding.UTF8.GetString((byte[])props.Headers["Checksum"]);
            string JsonString = Encoding.UTF8.GetString(body);

            try
            {
                ValidateCheckSum(JsonString, CheckSum);
                Response = ProcessRequest(JsonString);
                Console.WriteLine(Response);
            }
            catch (Exception e)
            {
                Response = TMiceException.CreateExceptionJsonString(e);
                Console.WriteLine(Response);
            }
            finally
            {
                var responseBytes = Encoding.UTF8.GetBytes(Response);
                channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
        }
        private void StartListen(string ServerAddress, string QueueName)
        {
            var factory = new ConnectionFactory() { HostName = ServerAddress };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
                consumer.Received += (model, ea) => OnReceiveMessage(model, ea, channel);

                this.HoldConsole();

            }
        }
        
        
        public TMiceUser User { get; set; }
        public void Start(string ServerAddress, string QueueName)
        {
            if (User == null)
                Console.WriteLine("User is null");
            else
                Console.WriteLine("User: " + User.FullName);
            
            Console.WriteLine("Worker started. Waiting for tasks in queue.");
            this.StartListen(ServerAddress, QueueName);
           
        }


    }
}