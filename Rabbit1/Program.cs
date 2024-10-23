using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
namespace Rabbit1
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread threadPublisher = new Thread(Publish);
            Thread threadConsumer = new Thread(Consume);
            threadPublisher.Start();
            threadConsumer.Start();
        }
        private static void Publish()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "MainQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);

            while (true)
            {
                string message = Console.ReadLine();
                if (string.IsNullOrEmpty(message)) continue;

                string prefMessage = $"Rabbit1:{message}";
                var body = Encoding.UTF8.GetBytes(prefMessage);
                channel.BasicPublish(exchange: "", routingKey: "MainQueue", basicProperties: null, body: body);
                Console.WriteLine(" [X] Sent: {0}", prefMessage);
            }
        }
        private static void Consume()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: "SecondQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if (!message.StartsWith("Rabbit1:"))
                {
                    Console.WriteLine($"Received: {message}");
                }
            };
            channel.BasicConsume(queue: "SecondQueue", autoAck: true, consumer: consumer);
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }

}
