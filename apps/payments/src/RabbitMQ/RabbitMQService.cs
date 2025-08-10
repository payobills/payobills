using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Payobills.Payments.Services.Contracts;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using System.Threading;

namespace Payobills.Payments.RabbitMQ;

public class RabbitMQService : IAsyncDisposable
{
  private readonly RabbitMQOptions rabbitMQOptions;
  private IChannel channel;
  private IConnection connection;
  private readonly Semaphore channelLock = new Semaphore(1, 1);

  public RabbitMQService(
    IOptions<RabbitMQOptions> rabbitMQOptions
  )
  {
    this.rabbitMQOptions = rabbitMQOptions.Value;
  }

  private async Task createRabbitMQChannelAsync()
  {
    channelLock.WaitOne();
    try
    {
      if (string.IsNullOrWhiteSpace(rabbitMQOptions.ConnectionString))
        throw new InvalidOperationException("RabbitMQ connection string is not configured.");

      if ((connection?.IsOpen ?? false) && (channel?.IsOpen ?? false))
      { return; }

      var factory = new ConnectionFactory()
      {
        Uri = new Uri(rabbitMQOptions.ConnectionString)
      };

      this.connection = await factory.CreateConnectionAsync().ConfigureAwait(false);
      this.channel = await connection.CreateChannelAsync().ConfigureAwait(false);
    }
    finally
    {
      channelLock.Release();
    }
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

    await this.channel.BasicPublishAsync(
          string.Empty,
          routingKey: queueName,
          body: Encoding.UTF8.GetBytes(marshalledMessageString)
        );
  }

  public async ValueTask DisposeAsync()
  {
    try
    {
      if (channel is not null)
        await channel.CloseAsync().ConfigureAwait(false);
    }
    catch { /* swallow on shutdown */ }
    finally
    {
      channel?.Dispose();
    }

    try
    {
      if (connection is not null)
        await connection.CloseAsync().ConfigureAwait(false);
    }
    catch { }
    finally
    {
      connection?.Dispose();
    }

    try
    {
      GC.SuppressFinalize(this);
    }
    catch { }
  }
}
