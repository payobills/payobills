namespace Payobills.NocoDB;

public record NocoDBOptions
{
    public required string XCToken { get; set; }
    public required string BaseUrl { get; set; }
}
