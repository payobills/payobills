using Payobills.Payments.Services.Contracts;
using Payobills.Payments.Services.Contracts.DTOs;

public class TripDTOType : ObjectType<TripDTO>
{
    protected override void Configure(IObjectTypeDescriptor<TripDTO> descriptor)
    {
        descriptor.Field("transactions")
            .UsePaging()
            .UseSorting()
            .ResolveWith<TripDTOResolvers>(r => r.GetTransactionsAsync(default!, default!));
    }
}

public class TripDTOResolvers
{
    public async Task<IEnumerable<TransactionDTO>> GetTransactionsAsync(
        [Parent] TripDTO trip,
        [Service] ITransactionsService transactionsService)
    {
        var filters = new TransactionFiltersInput
        {
            StartPaidAt = trip.StartDate,
            EndPaidAt = trip.EndDate
        };
        return await transactionsService.GetTransactionsAsync(null!, filters);
    }
}
