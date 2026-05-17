using AutoMapper;
using AvalonPizza.Server.Models;
using AvalonPizza.Server.DTOs;

namespace AvalonPizza.Server.Profiles;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.StatusId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
            .ForMember(dest => dest.Toppings, opt => opt.MapFrom(src =>
                src.Toppings.Select(id => new Topping { ToppingId = id }).ToList()
            ));
    }
}