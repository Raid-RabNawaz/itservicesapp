using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.MappingProfiles;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Enums;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Tests.Unit.TestHelpers;

namespace ITServicesApp.Tests.Unit.Services
{
    public class ReviewServiceTests
    {
        private readonly IMapper _mapper;

        public ReviewServiceTests()
        { 
            _mapper = AutoMapperTestConfig.Create();
        }

        [Fact]
        public async Task CreateAsync_Succeeds_ForValidBooking()
        {
            var uow = new Mock<IUnitOfWork>();
            var bookings = new Mock<IBookingRepository>();
            var reviews = new Mock<ITechnicianReviewRepository>();

            uow.SetupGet(x => x.Bookings).Returns(bookings.Object);
            uow.SetupGet(x => x.TechnicianReviews).Returns(reviews.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            bookings.Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Booking { Id = 123, UserId = 9, TechnicianId = 5, Status = BookingStatus.Completed });

            reviews.Setup(r => r.ExistsForBookingAsync(123, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            TechnicianReview? saved = null;
            reviews.Setup(r => r.AddAsync(It.IsAny<TechnicianReview>(), It.IsAny<CancellationToken>()))
                   .Callback<TechnicianReview, CancellationToken>((e, _) => saved = e)
                   .Returns(Task.CompletedTask);

            var svc = new ReviewService(uow.Object, _mapper);

            var dto = new CreateReviewDto { BookingId = 123, TechnicianId = 5, Rating = 5, Comment = "Great!" };

            var result = await svc.CreateAsync(userId: 9, dto);

            Assert.NotNull(saved);
            Assert.Equal(9, saved!.UserId);
            Assert.Equal(5, saved.TechnicianId);
            Assert.Equal(123, saved.BookingId);
            Assert.Equal(5, result.Rating);
            reviews.Verify(r => r.AddAsync(It.IsAny<TechnicianReview>(), It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task CreateAsync_Throws_WhenBookingNotCompleted()
        {
            var uow = new Mock<IUnitOfWork>();
            var bookings = new Mock<IBookingRepository>();
            var reviews = new Mock<ITechnicianReviewRepository>();

            uow.SetupGet(x => x.Bookings).Returns(bookings.Object);
            uow.SetupGet(x => x.TechnicianReviews).Returns(reviews.Object);

            bookings.Setup(r => r.GetByIdAsync(321, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Booking { Id = 321, UserId = 9, TechnicianId = 11, Status = BookingStatus.Confirmed });

            reviews.Setup(r => r.ExistsForBookingAsync(321, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var svc = new ReviewService(uow.Object, _mapper);
            var dto = new CreateReviewDto { BookingId = 321, TechnicianId = 11, Rating = 4 };

            await Assert.ThrowsAsync<System.InvalidOperationException>(() => svc.CreateAsync(9, dto));
        }
        [Fact]
        public async Task CreateAsync_Throws_WhenDuplicateForBooking()
        {
            var uow = new Mock<IUnitOfWork>();
            var bookings = new Mock<IBookingRepository>();
            var reviews = new Mock<ITechnicianReviewRepository>();

            uow.SetupGet(x => x.Bookings).Returns(bookings.Object);
            uow.SetupGet(x => x.TechnicianReviews).Returns(reviews.Object);

            bookings.Setup(r => r.GetByIdAsync(123, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new Booking { Id = 123, UserId = 9, TechnicianId = 5, Status = BookingStatus.Completed });

            reviews.Setup(r => r.ExistsForBookingAsync(123, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var svc = new ReviewService(uow.Object, _mapper);
            var dto = new CreateReviewDto { BookingId = 123, TechnicianId = 5, Rating = 4 };

            await Assert.ThrowsAsync<System.InvalidOperationException>(() => svc.CreateAsync(9, dto));
        }
    }
}
