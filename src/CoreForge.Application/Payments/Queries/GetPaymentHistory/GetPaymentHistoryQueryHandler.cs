using AutoMapper;
using CoreForge.Application.Payments.DTOs;
using CoreForge.Domain.Entities;
using CoreForge.Domain.Interfaces;
using MediatR;

namespace CoreForge.Application.Payments.Queries.GetPaymentHistory;

public class GetPaymentHistoryQueryHandler(
    IRepository<PaymentTransaction> repository,
    IMapper mapper
) : IRequestHandler<GetPaymentHistoryQuery, IList<PaymentTransactionDto>>
{
    public async Task<IList<PaymentTransactionDto>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        // Global query filter scopes this to current tenant automatically.
        var transactions = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IList<PaymentTransactionDto>>(transactions);
    }
}
