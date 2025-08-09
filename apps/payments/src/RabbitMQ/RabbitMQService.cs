using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payobills.Payments.Services.Contracts;
using System.Text.Json.Serialization;
using RabbitMQ.Client;

namespace Payobills.Payments.RabbitMQ;

public class RabbitMQService
{
  private readonly RabbitMQOptions rabbitMQOptions;
  private  IChannel channel;
  private  IConnection connection;

  public RabbitMQService(
    IOptions<RabbitMQOptions> rabbitMQOptions
  )
  {
    this.rabbitMQOptions = rabbitMQOptions.Value;
  }

  private async Task createRabbitMQChannelAsync()
  {
    if (channel is not null && connection is not null)
    {
      return; // Channel already created
    }

    var factory = new ConnectionFactory()
    {
      Uri = new Uri(rabbitMQOptions.ConnectionString)
    };
    this.connection = await factory.CreateConnectionAsync();
    this.channel = await connection.CreateChannelAsync();

  }

  public async Task PublishMessageAsync(string queueName, string marshalledMessageString)
  {
    if (string.IsNullOrEmpty(queueName))
    {
      throw new ArgumentException("Queue name cannot be null or empty.", nameof(queueName));
    }

    if (marshalledMessageString == null)
    {
      throw new ArgumentNullException(nameof(marshalledMessageString), "Message cannot be null.");
    }

    await this.createRabbitMQChannelAsync();

    this.channel.BasicPublishAsync(
          string.Empty,
          routingKey: queueName,
          body: Encoding.UTF8.GetBytes(marshalledMessageString)
        );
  }
}
