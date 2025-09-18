// src/ITServicesApp.Application/MappingProfiles/AutoMapperProfile.cs
using System.Linq;
using AutoMapper;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Domain.Entities;

namespace ITServicesApp.Application.MappingProfiles
{
    public sealed class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ========= Users =========
            CreateMap<User, UserDto>();

            CreateMap<CreateUserDto, User>(MemberList.Source)
                .ForMember(d => d.Email, m => m.MapFrom(s => s.Email))
                .ForMember(d => d.FullName, m => m.MapFrom(s => s.FullName))
                .ForMember(d => d.Role, m => m.MapFrom(s => s.Role))
                // handled by service (no direct mapping to entity)
                .ForSourceMember(s => s.TempPassword, opt => opt.DoNotValidate());

            CreateMap<UpdateUserDto, User>(MemberList.Source)
                .ForMember(d => d.FullName, m => m.MapFrom(s => s.FullName))
                .ForMember(d => d.Role, m => m.MapFrom(s => s.Role));

            CreateMap<RegisterDto, User>(MemberList.Source)
                .ForMember(d => d.Email, m => m.MapFrom(s => s.Email))
                .ForMember(d => d.FullName, m => m.MapFrom(s => s.FullName))
                // password is hashed in service
                .ForSourceMember(s => s.Password, opt => opt.DoNotValidate());

            // ======== Catalog ========
            CreateMap<ServiceCategory, ServiceCategoryDto>();

            CreateMap<CreateServiceCategoryDto, ServiceCategory>(MemberList.Source)
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description));

            CreateMap<UpdateServiceCategoryDto, ServiceCategory>(MemberList.Source)
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description));

            CreateMap<ServiceIssue, ServiceIssueDto>();

            CreateMap<CreateServiceIssueDto, ServiceIssue>(MemberList.Source)
                .ForMember(d => d.ServiceCategoryId, m => m.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description))
                .ForMember(d => d.EstimatedDurationMinutes, m => m.MapFrom(s => s.EstimatedDurationMinutes))
                .ForMember(d => d.BasePrice, m => m.MapFrom(s => s.BasePrice));

            CreateMap<UpdateServiceIssueDto, ServiceIssue>(MemberList.Source)
                .ForMember(d => d.Name, m => m.MapFrom(s => s.Name))
                .ForMember(d => d.Description, m => m.MapFrom(s => s.Description))
                .ForMember(d => d.EstimatedDurationMinutes, m => m.MapFrom(s => s.EstimatedDurationMinutes))
                .ForMember(d => d.BasePrice, m => m.MapFrom(s => s.BasePrice));

            // ======== Technicians ========
            CreateMap<Technician, TechnicianDto>();

            CreateMap<Technician, TechnicianProfileDto>()
                .ForMember(d => d.UserFullName, m => m.MapFrom(s => s.User != null ? s.User.FullName : string.Empty))
                .ForMember(d => d.UserEmail, m => m.MapFrom(s => s.User != null ? s.User.Email : string.Empty))
                .ForMember(d => d.ServiceCategoryName, m => m.MapFrom(s => s.ServiceCategory != null ? s.ServiceCategory.Name : string.Empty))
                .ForMember(d => d.AverageRating, m => m.MapFrom(s => (s.Reviews != null && s.Reviews.Count > 0) ? s.Reviews.Average(r => r.Rating) : 0d))
                .ForMember(d => d.ReviewsCount, m => m.MapFrom(s => s.Reviews != null ? s.Reviews.Count : 0));

            CreateMap<CreateTechnicianDto, Technician>(MemberList.Source)
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.UserId))
                .ForMember(d => d.ServiceCategoryId, m => m.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.Bio, m => m.MapFrom(s => s.Bio))
                .ForMember(d => d.HourlyRate, m => m.MapFrom(s => s.HourlyRate))
                .ForMember(d => d.IsActive, m => m.MapFrom(s => s.IsActive));

            CreateMap<UpdateTechnicianProfileDto, Technician>(MemberList.Source)
                .ForMember(d => d.ServiceCategoryId, m => m.MapFrom(s => s.ServiceCategoryId))
                .ForMember(d => d.Bio, m => m.MapFrom(s => s.Bio))
                .ForMember(d => d.HourlyRate, m => m.MapFrom(s => s.HourlyRate))
                .ForMember(d => d.IsActive, m => m.MapFrom(s => s.IsActive));

            // ======== Slots & Unavailability ========
            CreateMap<TechnicianSlot, TechnicianSlotDto>()
                .ForMember(d => d.Start, m => m.MapFrom(s => (System.DateTime?)s.StartUtc))
                .ForMember(d => d.End, m => m.MapFrom(s => (System.DateTime?)s.EndUtc))
                .ForMember(d => d.DurationMinutes, m => m.MapFrom(s => (int?)(s.EndUtc - s.StartUtc).TotalMinutes))
                .ForMember(d => d.IsAvailable, m => m.Ignore()); // computed elsewhere

            CreateMap<CreateTechnicianSlotDto, TechnicianSlot>(MemberList.Source)
                .ForMember(d => d.TechnicianId, m => m.MapFrom(s => s.TechnicianId))
                .ForMember(d => d.StartUtc, m => m.MapFrom(s => s.StartUtc))
                .ForMember(d => d.EndUtc, m => m.MapFrom(s => s.EndUtc));

            CreateMap<TechnicianUnavailability, TechnicianUnavailabilityDto>();

            CreateMap<CreateUnavailabilityDto, TechnicianUnavailability>(MemberList.Source)
                .ForMember(d => d.TechnicianId, m => m.MapFrom(s => s.TechnicianId))
                .ForMember(d => d.StartUtc, m => m.MapFrom(s => s.StartUtc))
                .ForMember(d => d.EndUtc, m => m.MapFrom(s => s.EndUtc))
                .ForMember(d => d.Reason, m => m.MapFrom(s => s.Reason));

            // ========= Bookings =========
            CreateMap<Booking, BookingDto>()
                .ForMember(d => d.Items, m => m.MapFrom(s => s.Items))
                .ForMember(d => d.Address, m => m.MapFrom(s => new BookingAddressDto
                {
                    AddressId = s.AddressId,
                    Line1 = s.AddressLine1 ?? s.Address,
                    Line2 = s.AddressLine2,
                    City = s.City,
                    State = s.State,
                    PostalCode = s.PostalCode,
                    Country = s.Country
                }));

            CreateMap<Booking, BookingResponseDto>()
                .IncludeBase<Booking, BookingDto>()
                .ForMember(d => d.TechnicianName, m => m.MapFrom(s => s.Technician != null && s.Technician.User != null ? s.Technician.User.FullName : null))
                .ForMember(d => d.UserFullName, m => m.MapFrom(s => s.User != null ? s.User.FullName : null))
                .ForMember(d => d.Payment, m => m.MapFrom(s => s.Payment));
            CreateMap<CreateBookingDto, Booking>(MemberList.Source)
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.UserId))
                .ForMember(d => d.TechnicianId, m => m.MapFrom(s => s.TechnicianId))
                .ForMember(d => d.ServiceCategoryId, m => m.MapFrom(s => s.ServiceCategoryId ?? 0))
                .ForMember(d => d.ServiceIssueId, m => m.MapFrom(s => s.ServiceIssueId ?? 0))
                .ForMember(d => d.ScheduledStartUtc, m => m.MapFrom(s => s.Start))
                .ForMember(d => d.ScheduledEndUtc, m => m.MapFrom(s => s.End))
                .ForMember(d => d.AddressId, m => m.MapFrom(s => s.Address != null ? s.Address.AddressId : (int?)null))
                .ForMember(d => d.AddressLine1, m => m.MapFrom(s => s.Address != null ? s.Address.Line1 : null))
                .ForMember(d => d.AddressLine2, m => m.MapFrom(s => s.Address != null ? s.Address.Line2 : null))
                .ForMember(d => d.City, m => m.MapFrom(s => s.Address != null ? s.Address.City : null))
                .ForMember(d => d.State, m => m.MapFrom(s => s.Address != null ? s.Address.State : null))
                .ForMember(d => d.PostalCode, m => m.MapFrom(s => s.Address != null ? s.Address.PostalCode : null))
                .ForMember(d => d.Country, m => m.MapFrom(s => s.Address != null ? s.Address.Country : null))
                .ForMember(d => d.Address, m => m.MapFrom(s => s.Address != null ? s.Address.Line1 : null))
                .ForMember(d => d.Notes, m => m.MapFrom(s => s.Notes))
                .ForMember(d => d.ClientRequestId, m => m.MapFrom(s => s.ClientRequestId))
                .ForMember(d => d.Items, m => m.MapFrom(s => s.Items))
                .ForMember(d => d.PreferredPaymentMethod, m => m.MapFrom(s => s.PreferredPaymentMethod))
                .ForMember(d => d.CustomerFullName, m => m.MapFrom(s => s.GuestFullName ?? string.Empty))
                .ForMember(d => d.CustomerEmail, m => m.MapFrom(s => s.GuestEmail ?? string.Empty))
                                .ForMember(d => d.CustomerPhone, m => m.MapFrom(s => s.GuestPhone))
                .ForSourceMember(s => s.ServiceCategoryId, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.ServiceIssueId, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.Address, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.GuestFullName, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.GuestEmail, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.GuestPhone, opt => opt.DoNotValidate());


            CreateMap<UpdateBookingNotesDto, Booking>(MemberList.Source)
                .ForMember(d => d.Id, m => m.MapFrom(s => s.BookingId))
                .ForMember(d => d.Notes, m => m.MapFrom(s => s.Notes));

            CreateMap<BookingDraftItem, BookingPipelineItemDto>()
                .ForMember(d => d.DurationMinutes, m => m.MapFrom(s => s.DurationMinutes));

            CreateMap<BookingDraft, BookingPipelineStateDto>()
                .ForMember(d => d.Id, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.Status, m => m.MapFrom(s => s.Status))
                .ForMember(d => d.IsAuthenticatedUser, m => m.MapFrom(s => s.UserId.HasValue))
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.UserId))
                .ForMember(d => d.GuestFullName, m => m.MapFrom(s => s.GuestFullName))
                .ForMember(d => d.GuestEmail, m => m.MapFrom(s => s.GuestEmail))
                .ForMember(d => d.GuestPhone, m => m.MapFrom(s => s.GuestPhone))
                .ForMember(d => d.PreferredPaymentMethod, m => m.MapFrom(s => s.PreferredPaymentMethod))
                .ForMember(d => d.EstimatedDurationMinutes, m => m.MapFrom(s => s.EstimatedDurationMinutes))
                .ForMember(d => d.CreatedAtUtc, m => m.MapFrom(s => s.CreatedAtUtc))
                .ForMember(d => d.ExpiresAtUtc, m => m.MapFrom(s => s.ExpiresAtUtc))
                .ForMember(d => d.Service, m => m.MapFrom(s =>
                    s.Items.Any()
                        ? new BookingPipelineServiceSelectionDto
                        {
                            ServiceCategoryId = s.ServiceCategoryId,
                            ServiceIssueId = s.ServiceIssueId,
                            Items = s.Items.Select(i => new BookingPipelineItemDto
                            {
                                ServiceIssueId = i.ServiceIssueId,
                                Quantity = i.Quantity,
                                UnitPrice = i.UnitPrice,
                                DurationMinutes = i.DurationMinutes,
                                Notes = i.Notes
                            }).ToList()
                        }
                        : null))
                .ForMember(d => d.Address, m => m.MapFrom(s =>
                    !string.IsNullOrWhiteSpace(s.AddressLine1)
                        ? new BookingPipelineAddressDto
                        {
                            Line1 = s.AddressLine1 ?? string.Empty,
                            Line2 = s.AddressLine2,
                            City = s.City ?? string.Empty,
                            State = s.State ?? string.Empty,
                            PostalCode = s.PostalCode ?? string.Empty,
                            Country = s.Country ?? string.Empty,
                            Notes = s.Notes
                        }
                        : null))
                .ForMember(d => d.Slot, m => m.MapFrom(s =>
                    s.TechnicianId.HasValue && s.SlotStartUtc.HasValue && s.SlotEndUtc.HasValue
                        ? new BookingPipelineSlotDto
                        {
                            TechnicianId = s.TechnicianId.Value,
                            SlotId = s.SlotId,
                            StartUtc = s.SlotStartUtc.Value,
                            EndUtc = s.SlotEndUtc.Value,
                            DurationMinutes = s.EstimatedDurationMinutes
                        }
                        : null));
            CreateMap<BookingItem, BookingItemDto>();
            CreateMap<CreateBookingItemDto, BookingItem>(MemberList.Source)
                .ForMember(d => d.ServiceIssueId, m => m.MapFrom(s => s.ServiceIssueId))
                .ForMember(d => d.Quantity, m => m.MapFrom(s => s.Quantity))
                .ForMember(d => d.Notes, m => m.MapFrom(s => s.Notes))
                .ForMember(d => d.UnitPrice, m => m.MapFrom(s => s.UnitPrice ?? 0m))
                .ForMember(d => d.LineTotal, m => m.Ignore())
                .ForMember(d => d.ServiceName, m => m.Ignore())
                .ForMember(d => d.ServiceDescription, m => m.Ignore())
                .ForSourceMember(s => s.UnitPrice, opt => opt.DoNotValidate());

            // ========= Payments =========
            CreateMap<Payment, PaymentDto>();
            CreateMap<Payment, PaymentSummaryDto>();

            CreateMap<CreatePaymentDto, Payment>(MemberList.Source)
                .ForMember(d => d.BookingId, m => m.MapFrom(s => s.BookingId))
                .ForMember(d => d.Method, m => m.MapFrom(s => s.Method))
                .ForMember(d => d.Amount, m => m.MapFrom(s => s.Amount))
                .ForMember(d => d.Currency, m => m.MapFrom(s => s.Currency));

            // ===== Reviews & Notifications =====
            CreateMap<TechnicianReview, TechnicianReviewDto>()
                .ForMember(d => d.SubmittedAtUtc, m => m.MapFrom(s => s.CreatedAtUtc));

            CreateMap<CreateReviewDto, TechnicianReview>(MemberList.Source)
                .ForMember(d => d.BookingId, m => m.MapFrom(s => s.BookingId))
                .ForMember(d => d.TechnicianId, m => m.MapFrom(s => s.TechnicianId))
                .ForMember(d => d.Rating, m => m.MapFrom(s => s.Rating))
                .ForMember(d => d.Comment, m => m.MapFrom(s => s.Comment));
            // UserId set by service.

            CreateMap<Notification, NotificationDto>();
        }
    }
}













