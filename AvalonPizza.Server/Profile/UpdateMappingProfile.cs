using AvalonPizza.Server.DTOs;
using AvalonPizza.Server.Models;

namespace AvalonPizza.Server.Profile;

public class UpdateMappingProfile : AutoMapper.Profile
{
    public UpdateMappingProfile()
    {
        CreateMap<UpdateRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
            .ForMember(dest => dest.DeliveryAddress, opt => opt.Ignore())
            .ForMember(dest => dest.TotalPrice, opt => opt.Ignore())
            .ForMember(dest => dest.StatusId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Toppings, opt => opt.MapFrom(src =>
                src.Toppings.Select(id => new Topping { ToppingId = id }).ToList()
            ));
    }
}
