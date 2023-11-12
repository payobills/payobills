using AutoMapper;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts.DTOs;

public class BillsMappingProfile : Profile
{
    public BillsMappingProfile()
    {
        CreateMap<CreateBillDTO, Bill>();
        CreateMap<Bill, BillDTO>();

        CreateMap<BillPayment, PaymentDTO>()
            .ForMember(d => d.BillingPeriod, opt => opt.MapFrom(s => new Range<DateTime>(s.BillPeriodStart, s.BillPeriodEnd)));
    }
}