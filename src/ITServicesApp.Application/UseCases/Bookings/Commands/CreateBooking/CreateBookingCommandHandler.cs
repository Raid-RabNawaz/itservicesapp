using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Events;
using ITServicesApp.Domain.Interfaces;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking
{
    public sealed class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingResponseDto>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _clock;
        private readonly IBackgroundJobService _jobs;
        private readonly IMediator _mediator;

        public CreateBookingCommandHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IDateTimeProvider clock,
            IBackgroundJobService jobs,
            IMediator mediator)
        {
            _uow = uow; _mapper = mapper; _clock = clock; _jobs = jobs; _mediator = mediator;
        }

        public async Task<BookingResponseDto> Handle(CreateBookingCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            // IDEMPOTENCY: if a booking already exists for this user + clientRequestId, return it
            if (!string.IsNullOrWhiteSpace(request.ClientRequestId))
            {
                var existing = await _uow.Bookings.ListAsync(b =>
                    b.UserId == dto.UserId && b.ClientRequestId == request.ClientRequestId, ct);

                var first = existing.FirstOrDefault();
                if (first != null)
                {
                    var withNavs = await _uow.Bookings.GetByIdAsync(first.Id, ct) ?? first;
                    return _mapper.Map<BookingResponseDto>(withNavs);
                }
            }

            // Guards
            var daySlots = await _uow.TechnicianSlots.GetAvailableAsync(dto.TechnicianId, dto.Start.Date, ct);
            if (!daySlots.Any(s => s.StartUtc <= dto.Start && dto.End <= s.EndUtc))
                throw new InvalidOperationException("Selected technician has no available slot covering the requested time.");

            if (await _uow.Bookings.HasOverlapAsync(dto.TechnicianId, dto.Start, dto.End, ct))
                throw new InvalidOperationException("The technician is already booked in the selected time range.");

            // Persist
            var entity = _mapper.Map<Booking>(dto);
            entity.ClientRequestId = request.ClientRequestId;
            entity.ScheduledStartUtc = dto.Start;
            entity.ScheduledEndUtc = dto.End;
            entity.Status = BookingStatus.PendingTechnicianConfirmation;

            if (entity.Items.Count == 0 && dto.ServiceIssueId.HasValue)
            {
                entity.Items.Add(new BookingItem
                {
                    ServiceIssueId = dto.ServiceIssueId.Value,
                    Quantity = 1
                });
            }

            decimal total = 0m;
            foreach (var item in entity.Items)
            {
                var issue = await _uow.ServiceIssues.GetByIdAsync(item.ServiceIssueId, ct)
                            ?? throw new InvalidOperationException($"Service issue {item.ServiceIssueId} not found.");

                item.ServiceName = issue.Name;
                item.ServiceDescription = issue.Description;
                if (item.UnitPrice <= 0)
                {
                    item.UnitPrice = issue.BasePrice;
                }
                item.LineTotal = item.UnitPrice * item.Quantity;
                total += item.LineTotal;
            }

            entity.EstimatedTotal = total;
            entity.FinalTotal = total;

            if (string.IsNullOrWhiteSpace(entity.CustomerFullName) || string.IsNullOrWhiteSpace(entity.CustomerEmail))
            {
                var user = await _uow.Users.GetByIdAsync(entity.UserId, ct);
                if (user != null)
                {
                    if (string.IsNullOrWhiteSpace(entity.CustomerFullName))
                        entity.CustomerFullName = user.FullName;
                    if (string.IsNullOrWhiteSpace(entity.CustomerEmail))
                        entity.CustomerEmail = user.Email;
                }
            }

            if (string.IsNullOrWhiteSpace(entity.CustomerEmail) && !string.IsNullOrWhiteSpace(dto.GuestEmail))
            {
                entity.CustomerEmail = dto.GuestEmail!;
            }

            if (string.IsNullOrWhiteSpace(entity.CustomerFullName) && !string.IsNullOrWhiteSpace(dto.GuestFullName))
            {
                entity.CustomerFullName = dto.GuestFullName!;
            }

            if (string.IsNullOrWhiteSpace(entity.CustomerPhone) && !string.IsNullOrWhiteSpace(dto.GuestPhone))
            {
                entity.CustomerPhone = dto.GuestPhone;
            }

            if (string.IsNullOrWhiteSpace(entity.Address))
            {
                entity.Address = entity.AddressLine1;
            }

            if (entity.Items.Count > 0)
            {
                var firstItemIssue = await _uow.ServiceIssues.GetByIdAsync(entity.Items.First().ServiceIssueId, ct);
                if (firstItemIssue != null)
                {
                    entity.ServiceIssueId = firstItemIssue.Id;
                    entity.ServiceCategoryId = firstItemIssue.ServiceCategoryId;
                }
            }

            await _uow.Bookings.AddAsync(entity, ct);
            await _uow.SaveChangesAsync(ct);

            // Schedule reminder and store job id
            var remindAt = entity.ScheduledStartUtc.AddHours(-2);
            if (remindAt > _clock.UtcNow)
            {
                entity.ReminderJobId = await _jobs.ScheduleBookingReminderAsync(entity.Id, remindAt, ct);
                _uow.Bookings.Update(entity);
                await _uow.SaveChangesAsync(ct);
            }

            // Publish event
            await _mediator.Publish(new BookingCreatedDomainEvent(entity.Id, entity.UserId, entity.TechnicianId, entity.ScheduledStartUtc), ct);

            var saved = await _uow.Bookings.GetByIdAsync(entity.Id, ct) ?? entity;
            return _mapper.Map<BookingResponseDto>(saved);
        }
    }
}

