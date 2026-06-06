using AutoMapper;
using CoreForge.Application.Payments.DTOs;
using CoreForge.Domain.Entities;

namespace CoreForge.Application.Payments.Mappings;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<Subscription, SubscriptionDto>();
        CreateMap<PaymentTransaction, PaymentTransactionDto>();
    }
}
