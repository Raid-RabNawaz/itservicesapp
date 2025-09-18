using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Pipeline
{
    public sealed record StartBookingDraftCommand(BookingPipelineStartRequestDto Dto) : IRequest<BookingPipelineStateDto>;

    public sealed class StartBookingDraftCommandHandler : IRequestHandler<StartBookingDraftCommand, BookingPipelineStateDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _current;
        private readonly IMapper _mapper;

        public StartBookingDraftCommandHandler(IUnitOfWork uow, ICurrentUserService current, IMapper mapper)
        {
            _uow = uow;
            _current = current;
            _mapper = mapper;
        }

        public async Task<BookingPipelineStateDto> Handle(StartBookingDraftCommand request, CancellationToken ct)
        {
            var dto = request.Dto ?? throw new ArgumentNullException(nameof(request.Dto));

            if (dto.ServiceIssueId <= 0)
            {
                throw new InvalidOperationException("Service issue must be provided.");
            }

            var issueCache = new Dictionary<int, ServiceIssue>();

            async Task<ServiceIssue> LoadIssueAsync(int issueId)
            {
                if (!issueCache.TryGetValue(issueId, out var cached))
                {
                    cached = await _uow.ServiceIssues.GetByIdAsync(issueId, ct)
                              ?? throw new InvalidOperationException($"Service issue {issueId} not found.");
                    issueCache[issueId] = cached;
                }
                return cached;
            }

            var primaryIssue = await LoadIssueAsync(dto.ServiceIssueId);

            var draft = new BookingDraft
            {
                Id = Guid.NewGuid(),
                ServiceIssueId = dto.ServiceIssueId,
                ServiceCategoryId = dto.ServiceCategoryId > 0 ? dto.ServiceCategoryId : primaryIssue.ServiceCategoryId,
                Notes = dto.Notes,
                PreferredPaymentMethod = dto.PreferredPaymentMethod,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(6),
                GuestFullName = dto.FullName,
                GuestEmail = dto.Email,
                GuestPhone = dto.Phone
            };

            if (_current.IsAuthenticated && _current.UserIdInt > 0)
            {
                draft.UserId = _current.UserIdInt;

                if (string.IsNullOrWhiteSpace(draft.GuestEmail) || string.IsNullOrWhiteSpace(draft.GuestFullName))
                {
                    var me = await _uow.Users.GetByIdAsync(_current.UserIdInt, ct);
                    if (me != null)
                    {
                        draft.GuestEmail ??= me.Email;
                        draft.GuestFullName ??= me.FullName;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(draft.GuestEmail) || string.IsNullOrWhiteSpace(draft.GuestFullName))
            {
                throw new InvalidOperationException("Guest name and email are required to start a booking.");
            }

            var totalDuration = 0;
            if (dto.Items is { Count: > 0 })
            {
                foreach (var item in dto.Items)
                {
                    var itemIssue = await LoadIssueAsync(item.ServiceIssueId);
                    var baseDuration = itemIssue.EstimatedDurationMinutes ?? 60;
                    var quantity = Math.Max(1, item.Quantity);
                    var duration = baseDuration * quantity;
                    totalDuration += duration;

                    draft.Items.Add(new BookingDraftItem
                    {
                        ServiceIssueId = item.ServiceIssueId,
                        Quantity = quantity,
                        UnitPrice = item.UnitPrice ?? itemIssue.BasePrice,
                        DurationMinutes = baseDuration,
                        Notes = item.Notes
                    });
                }
            }
            else
            {
                var baseDuration = primaryIssue.EstimatedDurationMinutes ?? 60;
                totalDuration = baseDuration;

                draft.Items.Add(new BookingDraftItem
                {
                    ServiceIssueId = dto.ServiceIssueId,
                    Quantity = 1,
                    UnitPrice = primaryIssue.BasePrice,
                    DurationMinutes = baseDuration
                });
            }

            draft.EstimatedDurationMinutes = totalDuration > 0 ? totalDuration : 60;

            await _uow.BookingDrafts.AddAsync(draft, ct);
            await _uow.SaveChangesAsync(ct);

            return _mapper.Map<BookingPipelineStateDto>(draft);
        }
    }
}
