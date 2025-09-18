using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Moq;
using Xunit;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.MappingProfiles;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Application.Options;
using ITServicesApp.Application.UseCases.Bookings.Commands.CompleteBooking;
using ITServicesApp.Tests.Unit.TestHelpers;
using Microsoft.Extensions.Options;

namespace ITServicesApp.Tests.Unit.Services
{
    public class PaymentServiceTests
    {
        private readonly IMapper _mapper;

        public PaymentServiceTests()
        {
            _mapper = AutoMapperTestConfig.Create();
        }

        [Fact]
        public async Task CreateCashAsync_Sets_Succeeded()
        {
            var uow = new Mock<IUnitOfWork>();
            var bookings = new Mock<IBookingRepository>();
            var payments = new Mock<IPaymentRepository>();
            var opt = new StripeOptions
            {
                SecretKey = "sk_test_123",
                PublishableKey = "pk_test_123",
                WebhookSecret = "whsec_test_123"
            };
            uow.SetupGet(x => x.Bookings).Returns(bookings.Object);
            uow.SetupGet(x => x.Payments).Returns(payments.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            bookings.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Booking { Id = 1, Status = BookingStatus.Confirmed, UserId = 42 });

            Payment? saved = null;
            payments.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                    .Callback<Payment, CancellationToken>((p, _) => saved = p)
                    .Returns(Task.CompletedTask);

            var mediator = new Mock<IMediator>();
            var stripe = new StripeService(payments.Object, uow.Object);
            var current = new Mock<ICurrentUserService>();
            current.SetupGet(x => x.UserIdInt).Returns(42);
            current.SetupGet(x => x.Role).Returns(UserRole.Customer.ToString());
            current.SetupGet(x => x.IsAuthenticated).Returns(true);

            var svc = new PaymentService(uow.Object, _mapper, stripe, Options.Create(opt), current.Object, mediator.Object);

            var dto = new CreatePaymentDto { BookingId = 1, Method = PaymentMethod.Cash, Amount = 100m, Currency = "USD" };
            var result = await svc.CreateCashAsync(dto);

            Assert.NotNull(saved);
            Assert.Equal("Succeeded", saved!.Status);
            Assert.Equal(100m, result.Amount);
            Assert.Equal(PaymentMethod.Cash, result.Method);
            mediator.Verify(m => m.Send(It.Is<CompleteBookingCommand>(c => c.BookingId == 1), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateOnlineAsync_Assigns_ProviderPaymentId()
        {
            var uow = new Mock<IUnitOfWork>();
            var bookings = new Mock<IBookingRepository>();
            var payments = new Mock<IPaymentRepository>();
            var opt = new StripeOptions
            {
                SecretKey = "sk_test_123",
                PublishableKey = "pk_test_123",
                WebhookSecret = "whsec_test_123"
            };

            uow.SetupGet(x => x.Bookings).Returns(bookings.Object);
            uow.SetupGet(x => x.Payments).Returns(payments.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            bookings.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Booking { Id = 2, Status = BookingStatus.Confirmed, UserId = 42 });

            Payment? saved = null;
            payments.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
                    .Callback<Payment, CancellationToken>((p, _) => saved = p)
                    .Returns(Task.CompletedTask);

            var mediator = new Mock<IMediator>();
            var stripe = new StripeService(payments.Object, uow.Object);
            var current = new Mock<ICurrentUserService>();
            current.SetupGet(x => x.UserIdInt).Returns(42);
            current.SetupGet(x => x.Role).Returns(UserRole.Customer.ToString());
            current.SetupGet(x => x.IsAuthenticated).Returns(true);

            var svc = new PaymentService(uow.Object, _mapper, stripe, Options.Create(opt), current.Object, mediator.Object);

            var dto = new CreatePaymentDto { BookingId = 2, Method = PaymentMethod.Card, Amount = 59.99m, Currency = "USD" };
            var result = await svc.CreateOnlineAsync(dto);

            Assert.NotNull(saved);
            Assert.Equal("Pending", saved!.Status);
            Assert.Equal(PaymentMethod.Card, saved.Method);
            // ProviderPaymentId is generated by StripeService fake
            Assert.False(string.IsNullOrWhiteSpace(result.ProviderPaymentId));
            mediator.Verify(m => m.Send(It.IsAny<CompleteBookingCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task HandleStripeEventAsync_CompletesBooking_OnSucceeded()
        {
            var uow = new Mock<IUnitOfWork>();
            var bookings = new Mock<IBookingRepository>();
            var payments = new Mock<IPaymentRepository>();
            var opt = new StripeOptions
            {
                SecretKey = "sk_test_123",
                PublishableKey = "pk_test_123",
                WebhookSecret = "whsec_test_123"
            };

            var booking = new Booking { Id = 3, Status = BookingStatus.Confirmed, UserId = 7, TechnicianId = 2 };
            var payment = new Payment { Id = 10, BookingId = 3, Status = "pending" };

            uow.SetupGet(x => x.Bookings).Returns(bookings.Object);
            uow.SetupGet(x => x.Payments).Returns(payments.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            payments.Setup(p => p.GetByProviderPaymentIdAsync("pi_ok", It.IsAny<CancellationToken>())).ReturnsAsync(payment);
            payments.Setup(p => p.WebhookSeenAsync(payment.Id, "evt_1", It.IsAny<CancellationToken>())).ReturnsAsync(false);
            payments.Setup(p => p.MarkWebhookSeenAsync(payment.Id, "evt_1", It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            bookings.Setup(b => b.GetByIdAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

            var mediator = new Mock<IMediator>();
            var stripe = new StripeService(payments.Object, uow.Object);
            var current = new Mock<ICurrentUserService>();
            current.SetupGet(x => x.UserIdInt).Returns(7);
            current.SetupGet(x => x.Role).Returns(UserRole.Customer.ToString());
            current.SetupGet(x => x.IsAuthenticated).Returns(true);

            var svc = new PaymentService(uow.Object, _mapper, stripe, Options.Create(opt), current.Object, mediator.Object);

            await svc.HandleStripeEventAsync("evt_1", "payment_intent.succeeded", "pi_ok", "ch_1", "succeeded", 120m, "usd", CancellationToken.None);

            Assert.Equal("succeeded", payment.Status);
            mediator.Verify(m => m.Send(It.Is<CompleteBookingCommand>(c => c.BookingId == 3), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
