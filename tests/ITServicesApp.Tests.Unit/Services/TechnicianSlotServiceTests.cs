using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using Xunit;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.MappingProfiles;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Tests.Unit.TestHelpers;

namespace ITServicesApp.Tests.Unit.Services
{
    public class TechnicianSlotServiceTests
    {
        private readonly IMapper _mapper;

        public TechnicianSlotServiceTests()
        {
            _mapper = AutoMapperTestConfig.Create();
        }

        [Fact]
        public async Task CreateAsync_Creates_WhenNoOverlap()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var slotRepo = new Mock<ITechnicianSlotRepository>();
            uow.SetupGet(x => x.TechnicianSlots).Returns(slotRepo.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            slotRepo.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TechnicianSlot, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<TechnicianSlot>());

            TechnicianSlot? saved = null;
            slotRepo.Setup(r => r.AddAsync(It.IsAny<TechnicianSlot>(), It.IsAny<CancellationToken>()))
                    .Callback<TechnicianSlot, CancellationToken>((e, _) => saved = e)
                    .Returns(Task.CompletedTask);

            var svc = new TechnicianSlotService(uow.Object, _mapper);

            var dto = new CreateTechnicianSlotDto
            {
                TechnicianId = 7,
                StartUtc = new DateTime(2025, 8, 25, 9, 0, 0, DateTimeKind.Utc),
                EndUtc = new DateTime(2025, 8, 25, 11, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await svc.CreateAsync(dto);

            // Assert
            Assert.NotNull(saved);
            Assert.Equal(dto.TechnicianId, saved!.TechnicianId);
            Assert.Equal(dto.StartUtc, saved.StartUtc);
            Assert.Equal(dto.EndUtc, saved.EndUtc);
            Assert.Equal(dto.TechnicianId, result.TechnicianId);
            Assert.Equal(dto.StartUtc, result.StartUtc);
            Assert.Equal(dto.EndUtc, result.EndUtc);
        }

        [Fact]
        public async Task CreateAsync_Throws_OnOverlap()
        {
            // Arrange
            var uow = new Mock<IUnitOfWork>();
            var slotRepo = new Mock<ITechnicianSlotRepository>();
            uow.SetupGet(x => x.TechnicianSlots).Returns(slotRepo.Object);

            var overlapSlot = new TechnicianSlot
            {
                TechnicianId = 7,
                StartUtc = new DateTime(2025, 8, 25, 9, 30, 0, DateTimeKind.Utc),
                EndUtc = new DateTime(2025, 8, 25, 10, 30, 0, DateTimeKind.Utc)
            };

            slotRepo.Setup(r => r.ListAsync(It.IsAny<System.Linq.Expressions.Expression<Func<TechnicianSlot, bool>>>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<TechnicianSlot> { overlapSlot });

            var svc = new TechnicianSlotService(uow.Object, _mapper);

            var dto = new CreateTechnicianSlotDto
            {
                TechnicianId = 7,
                StartUtc = new DateTime(2025, 8, 25, 9, 0, 0, DateTimeKind.Utc),
                EndUtc = new DateTime(2025, 8, 25, 11, 0, 0, DateTimeKind.Utc)
            };

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => svc.CreateAsync(dto));
        }

        [Fact]
        public async Task DeleteByStartAsync_Removes_Slot()
        {
            var uow = new Mock<IUnitOfWork>();
            var slotRepo = new Mock<ITechnicianSlotRepository>();
            uow.SetupGet(x => x.TechnicianSlots).Returns(slotRepo.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var start = new DateTime(2025, 8, 25, 9, 0, 0, DateTimeKind.Utc);
            var slot = new TechnicianSlot { TechnicianId = 88, StartUtc = start, EndUtc = start.AddHours(1) };

            slotRepo.Setup(r => r.GetByTechAndStartAsync(88, start, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(slot);

            var svc = new TechnicianSlotService(uow.Object, _mapper);

            await svc.DeleteByStartAsync(88, start);

            slotRepo.Verify(r => r.Delete(It.Is<TechnicianSlot>(s => s == slot)), Times.Once);
            uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
