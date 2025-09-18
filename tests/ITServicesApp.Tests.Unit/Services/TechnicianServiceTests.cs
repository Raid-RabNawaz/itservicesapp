using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.MappingProfiles;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Tests.Unit.TestHelpers;

namespace ITServicesApp.Tests.Unit.Services
{
    public class TechnicianServiceTests
    {
        private readonly IMapper _mapper;

        public TechnicianServiceTests()
        {
            _mapper = AutoMapperTestConfig.Create();
        }

        [Fact]
        public async Task GetCalendarAsync_Merges_Slots_And_Unavailability()
        {
            var uow = new Mock<IUnitOfWork>();
            var slotsRepo = new Mock<ITechnicianSlotRepository>();
            var unavRepo = new Mock<ITechnicianUnavailabilityRepository>();
            var techRepo = new Mock<ITechnicianRepository>();
            var catRepo = new Mock<IServiceCategoryRepository>();
            var revRepo = new Mock<ITechnicianReviewRepository>();
            var current = new Mock<ICurrentUserService>();

            uow.SetupGet(x => x.TechnicianSlots).Returns(slotsRepo.Object);
            uow.SetupGet(x => x.TechnicianUnavailabilities).Returns(unavRepo.Object);
            uow.SetupGet(x => x.TechnicianReviews).Returns(revRepo.Object);
            uow.SetupGet(x => x.ServiceCategories).Returns(catRepo.Object);
            uow.SetupGet(x => x.Technicians).Returns(techRepo.Object);

            var from = new DateTime(2025, 8, 20, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddDays(1);

            slotsRepo.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TechnicianSlot, bool>>>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new List<TechnicianSlot>
                     {
                         new TechnicianSlot{ TechnicianId = 77, StartUtc = from.AddHours(9), EndUtc = from.AddHours(11) },
                         new TechnicianSlot{ TechnicianId = 77, StartUtc = from.AddHours(13), EndUtc = from.AddHours(15) }
                     });

            unavRepo.Setup(r => r.ListForTechnicianAsync(77, from, to, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<TechnicianUnavailability>
                    {
                        new TechnicianUnavailability{ TechnicianId = 77, StartUtc = from.AddHours(12), EndUtc = from.AddHours(13) }
                    });

            var svc = new TechnicianService(uow.Object, _mapper, current.Object);

            var list = await svc.GetCalendarAsync(77, from, to);

            Assert.Equal(3, list.Count);
            Assert.Contains(list, x => x.IsAvailable == false && x.StartUtc == from.AddHours(12));
        }

        [Fact]
        public async Task UpdateProfile_Updates_Entity()
        {
            var uow = new Mock<IUnitOfWork>();
            var techRepo = new Mock<ITechnicianRepository>();
            var current = new Mock<ICurrentUserService>();
            uow.SetupGet(x => x.Technicians).Returns(techRepo.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var tech = new Technician { Id = 66, ServiceCategoryId = 1, IsActive = true };
            techRepo.Setup(r => r.GetByIdAsync(66, It.IsAny<CancellationToken>())).ReturnsAsync(tech);

            var svc = new TechnicianService(uow.Object, _mapper, current.Object);

            await svc.UpdateProfileAsync(66, new UpdateTechnicianProfileDto { ServiceCategoryId = 2, Bio = "hi", HourlyRate = 50, IsActive = false });

            techRepo.Verify(r => r.Update(It.Is<Technician>(t => t.ServiceCategoryId == 2 && t.IsActive == false)), Times.Once);
            uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
