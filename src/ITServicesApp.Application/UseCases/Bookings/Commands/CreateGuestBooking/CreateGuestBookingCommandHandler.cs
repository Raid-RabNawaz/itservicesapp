using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.UseCases.Bookings.Commands.CreateBooking;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Domain.Enums;
using MediatR;

namespace ITServicesApp.Application.UseCases.Bookings.Commands.CreateGuestBooking
{
    public sealed class CreateGuestBookingCommandHandler : IRequestHandler<CreateGuestBookingCommand, GuestBookingResponseDto>
    {
        private readonly IUserService _users;
        private readonly IMediator _mediator;

        public CreateGuestBookingCommandHandler(IUserService users, IMediator mediator)
        {
            _users = users;
            _mediator = mediator;
        }

        public async Task<GuestBookingResponseDto> Handle(CreateGuestBookingCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var existingUser = await _users.GetByEmailAsync(dto.Email, ct);
            if (existingUser != null)
            {
                return new GuestBookingResponseDto
                {
                    RequiresLogin = true,
                    ExistingUserId = existingUser.Id,
                    Booking = null
                };
            }

            var createdUser = await _users.CreateAsync(new CreateUserDto
            {
                Email = dto.Email,
                FullName = dto.FullName,
                Role = UserRole.Customer
            }, ct);

            var address = dto.Address ?? new BookingAddressDto();
            var bookingDto = new CreateBookingDto
            {
                UserId = createdUser.Id,
                TechnicianId = dto.TechnicianId,
                ServiceCategoryId = dto.ServiceCategoryId,
                ServiceIssueId = dto.ServiceIssueId,
                Start = dto.Start,
                End = dto.End,
                Notes = dto.Notes,
                PreferredPaymentMethod = dto.PreferredPaymentMethod,
                GuestFullName = dto.FullName,
                GuestEmail = dto.Email,
                GuestPhone = dto.Phone,
                ClientRequestId = dto.ClientRequestId,
                Address = new BookingAddressDto
                {
                    AddressId = address.AddressId,
                    Line1 = address.Line1,
                    Line2 = address.Line2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country
                },
                Items = dto.Items.Select(i => new CreateBookingItemDto
                {
                    ServiceIssueId = i.ServiceIssueId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Notes = i.Notes
                }).ToList()
            };

            if (!bookingDto.Items.Any() && bookingDto.ServiceIssueId.HasValue)
            {
                bookingDto.Items.Add(new CreateBookingItemDto
                {
                    ServiceIssueId = bookingDto.ServiceIssueId.Value,
                    Quantity = 1
                });
            }

            var booking = await _mediator.Send(new CreateBookingCommand(bookingDto, bookingDto.ClientRequestId), ct);

            return new GuestBookingResponseDto
            {
                RequiresLogin = false,
                ExistingUserId = createdUser.Id,
                Booking = booking
            };
        }
    }
}
