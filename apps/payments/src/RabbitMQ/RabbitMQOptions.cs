namespace Payobills.Payments.RabbitMQ;

public record RabbitMQOptions
{
    public required string ConnectionString { get; set; }
}
