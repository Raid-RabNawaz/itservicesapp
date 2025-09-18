using System.Collections.Generic;
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
using ITServicesApp.Tests.Unit.TestHelpers;

namespace ITServicesApp.Tests.Unit.Services
{
    public class ServiceCatalogServiceTests
    {
        private readonly IMapper _mapper;

        public ServiceCatalogServiceTests()
        {
            _mapper = AutoMapperTestConfig.Create();
        }

        [Fact]
        public async Task CreateCategory_Update_Delete_Works()
        {
            var uow = new Mock<IUnitOfWork>();
            var catRepo = new Mock<IServiceCategoryRepository>();
            var issueRepo = new Mock<IServiceIssueRepository>();

            uow.SetupGet(x => x.ServiceCategories).Returns(catRepo.Object);
            uow.SetupGet(x => x.ServiceIssues).Returns(issueRepo.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            ServiceCategory? saved = null;
            catRepo.Setup(r => r.AddAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
                   .Callback<ServiceCategory, CancellationToken>((e, _) => saved = e)
                   .Returns(Task.CompletedTask);

            var svc = new ServiceCatalogService(uow.Object, _mapper);

            // Create
            var created = await svc.CreateCategoryAsync(new CreateServiceCategoryDto { Name = "Networking", Description = "WiFi etc." });
            Assert.NotNull(saved);
            Assert.Equal("Networking", saved!.Name);
            Assert.Equal("Networking", created.Name);

            // Update
            catRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ServiceCategory { Id = 1, Name = "Networking" });
            await svc.UpdateCategoryAsync(1, new UpdateServiceCategoryDto { Name = "Network", Description = "LAN/WAN" });
            catRepo.Verify(r => r.Update(It.Is<ServiceCategory>(c => c.Name == "Network")), Times.Once);

            // Delete (no issues)
            catRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new ServiceCategory { Id = 1, Name = "Network" });
            catRepo.Setup(r => r.HasIssuesAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            await svc.DeleteCategoryAsync(1);
            catRepo.Verify(r => r.Delete(It.IsAny<ServiceCategory>()), Times.Once);
        }

        [Fact]
        public async Task DeleteCategory_Throws_WhenHasIssues()
        {
            var uow = new Mock<IUnitOfWork>();
            var catRepo = new Mock<IServiceCategoryRepository>();
            uow.SetupGet(x => x.ServiceCategories).Returns(catRepo.Object);

            catRepo.Setup(r => r.HasIssuesAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var svc = new ServiceCatalogService(uow.Object, AutoMapperTestConfig.Create());

            await Assert.ThrowsAsync<System.InvalidOperationException>(() => svc.DeleteCategoryAsync(99));
        }
    }
}
