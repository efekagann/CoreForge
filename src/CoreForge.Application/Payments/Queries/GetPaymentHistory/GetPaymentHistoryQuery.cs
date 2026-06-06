using CoreForge.Application.Payments.DTOs;
using MediatR;

namespace CoreForge.Application.Payments.Queries.GetPaymentHistory;

public record GetPaymentHistoryQuery : IRequest<IList<PaymentTransactionDto>>;
