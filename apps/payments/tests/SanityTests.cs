namespace payobills.payments.test;

public class SanityTests
{
    [Fact]
    public void Should_Pass_Sanity_Tests() {
        (1 + 1).Should().Be(2);
    }
}