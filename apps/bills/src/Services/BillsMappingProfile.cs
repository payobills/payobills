using AutoMapper;
using Payobills.Bills.Data.Contracts.Models;
using Payobills.Bills.Services.Contracts.DTOs;

public class BillsMappingProfile : Profile
{
    public BillsMappingProfile()
    {
        CreateMap<CreateBillDTO, Bill>();
        CreateMap<Bill, BillDTO>();
    }
}