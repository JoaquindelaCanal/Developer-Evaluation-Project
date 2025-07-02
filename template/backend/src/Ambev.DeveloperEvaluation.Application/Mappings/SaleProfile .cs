using Ambev.DeveloperEvaluation.Application.DTOs;
using Ambev.DeveloperEvaluation.Domain.Entities;

using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Mappings
{
    public class SaleProfile : Profile
    {
        public SaleProfile()
        {
            CreateMap<Sale, SaleDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<SaleItem, SaleItemDto>();

            // CreateMap<CreateSaleRequest, CreateSaleCommand>();
            // CreateMap<UpdateSaleRequest, UpdateSaleCommand>();
            // CreateMap<AddItemToSaleRequest, AddItemToSaleCommand>();
        }
    }
}
