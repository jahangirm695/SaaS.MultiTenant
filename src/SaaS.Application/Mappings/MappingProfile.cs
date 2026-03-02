using AutoMapper;
using SaaS.Application.DTOs.Auth;
using SaaS.Application.DTOs.Subscriptions;
using SaaS.Application.DTOs.Tenants;
using SaaS.Domain.Entities;

namespace SaaS.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.TenantName, opt => opt.MapFrom(src => src.Tenant.Name));

        // Tenant mappings
        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.Users.Count));

        // Subscription mappings
        CreateMap<SubscriptionPlan, SubscriptionPlanDto>();
        
        CreateMap<TenantSubscription, TenantSubscriptionDto>()
            .ForMember(dest => dest.DaysRemaining, opt => opt.MapFrom(src => src.DaysRemaining));
    }
}
