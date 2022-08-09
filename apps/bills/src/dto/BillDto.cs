namespace payobills.bills.dtos;

public class BillDto
{
    public string Name { get; set; }
    public int BillingDate { get; set; }
    public int PayByDate { get; set; }
    public int LatePayByDate { get; set; }
}