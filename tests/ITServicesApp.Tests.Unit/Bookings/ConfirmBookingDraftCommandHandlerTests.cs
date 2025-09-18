using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.UseCases.Bookings.Pipeline;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using MediatR;
using Moq;
using Xunit;

namespace ITServicesApp.Tests.Unit.Bookings
{
    public class ConfirmBookingDraftCommandHandlerTests
    {
        [Fact]
        public async Task Handle_Throws_WhenDraftExpired()
        {
            // Arrange
            var draftId = Guid.NewGuid();
            var start = DateTime.UtcNow.AddHours(2);
            var draft = CreateValidDraft(draftId, start);
            draft.ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-5);

            var assignment = new Mock<IBookingAssignmentService>(MockBehavior.Strict);
            var handler = CreateHandler(draft, assignment, out var mediator, out var uow);

            // Act
            var act = () => handler.Handle(new ConfirmBookingDraftCommand(draftId, new BookingPipelineConfirmDto()), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Booking draft has expired.");
            assignment.Verify(a => a.IsTechnicianAvailableAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            mediator.VerifyNoOtherCalls();
            uow.VerifyGet(x => x.BookingDrafts, Times.AtLeastOnce());
        }

        [Fact]
        public async Task Handle_Throws_WhenTechnicianNoLongerAvailable()
        {
            // Arrange
            var draftId = Guid.NewGuid();
            var start = DateTime.UtcNow.AddHours(3);
            var draft = CreateValidDraft(draftId, start);

            var assignment = new Mock<IBookingAssignmentService>();
            assignment.Setup(a => a.IsTechnicianAvailableAsync(
                draft.TechnicianId!.Value,
                start,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var handler = CreateHandler(draft, assignment, out var mediator, out var uow);

            // Act
            var act = () => handler.Handle(new ConfirmBookingDraftCommand(draftId, new BookingPipelineConfirmDto()), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Selected technician is no longer available for the requested time.");
            assignment.Verify(a => a.IsTechnicianAvailableAsync(
                draft.TechnicianId!.Value,
                start,
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()), Times.Once);
            mediator.VerifyNoOtherCalls();
            uow.VerifyGet(x => x.BookingDrafts, Times.AtLeastOnce());
        }

        private static ConfirmBookingDraftCommandHandler CreateHandler(
            BookingDraft draft,
            Mock<IBookingAssignmentService> assignment,
            out Mock<IMediator> mediator,
            out Mock<IUnitOfWork> uow)
        {
            var draftRepo = new Mock<IBookingDraftRepository>();
            draftRepo.Setup(r => r.GetAsync(draft.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(draft);

            uow = new Mock<IUnitOfWork>();
            uow.SetupGet(x => x.BookingDrafts).Returns(draftRepo.Object);

            mediator = new Mock<IMediator>();

            return new ConfirmBookingDraftCommandHandler(uow.Object, mediator.Object, assignment.Object);
        }

        private static BookingDraft CreateValidDraft(Guid id, DateTime startUtc)
        {
            var draft = new BookingDraft
            {
                Id = id,
                Status = BookingDraftStatus.Pending,
                ServiceCategoryId = 10,
                ServiceIssueId = 20,
                AddressLine1 = "123 Service St",
                City = "Metropolis",
                PostalCode = "12345",
                Country = "USA",
                TechnicianId = 5,
                SlotStartUtc = startUtc,
                SlotEndUtc = startUtc.AddHours(1),
                EstimatedDurationMinutes = 60,
                PreferredPaymentMethod = PaymentMethod.Cash,
                UserId = 1,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(2)
            };

            draft.Items.Add(new BookingDraftItem
            {
                ServiceIssueId = draft.ServiceIssueId!.Value,
                Quantity = 1,
                UnitPrice = 100m,
                DurationMinutes = draft.EstimatedDurationMinutes
            });

            return draft;
        }
    }
}
