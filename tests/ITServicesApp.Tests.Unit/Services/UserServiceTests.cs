using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using ITServicesApp.Application.Abstractions;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Application.MappingProfiles;
using ITServicesApp.Application.Options;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Infrastructure.Services;
using ITServicesApp.Persistence;
using ITServicesApp.Tests.Unit.TestHelpers;
using Microsoft.Extensions.Options;

namespace ITServicesApp.Tests.Unit.Services
{
    public class UserServiceTests
    {
        private readonly IMapper _mapper;

        public UserServiceTests()
        {
            _mapper = AutoMapperTestConfig.Create();
        }

        [Fact]
        public async Task CreateAsync_Sends_FirstLogin_Email()
        {
            var uow = new Mock<IUnitOfWork>();
            var users = new Mock<IUserRepository>();
            var hasher = new Mock<IPasswordHasher>();
            var reset = new Mock<IPasswordResetService>();
            var email = new Mock<IEmailService>();
            var jwt = new Mock<IJwtTokenService>();
            var current = new Mock<ICurrentUserService>();
            var opts = InMemoryDb.CreateOptions();
            var db = new ApplicationDbContext(opts);

            uow.SetupGet(x => x.Users).Returns(users.Object);
            uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            hasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
            reset.Setup(r => r.GenerateAndStoreTokenAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync("token-123");

            User? saved = null;
            users.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Callback<User, CancellationToken>((u, _) => saved = u)
                 .Returns(Task.CompletedTask);

            var frontend = Options.Create(new FrontendOptions
            {
                BaseUrl = "https://app.test",
                FirstLoginPath = "/first-login"
            });

            var svc = new UserService(uow.Object, _mapper, hasher.Object, email.Object, jwt.Object, current.Object, db, reset.Object, frontend);

            var dto = new CreateUserDto { Email = "a@b.com", FullName = "Alice" };

            var result = await svc.CreateAsync(dto);

            Assert.NotNull(saved);
            Assert.Equal("Alice", result.FullName);
            email.Verify(e => e.SendAsync("a@b.com", It.IsAny<string>(), It.Is<string>(body => body.Contains("Set your password")), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CompleteFirstLoginAsync_ClearsMustChangeAndHashesPassword()
        {
            var opts = InMemoryDb.CreateOptions();
            using var db = new ApplicationDbContext(opts);

            var user = new User
            {
                Email = "first@login.test",
                FullName = "First Login",
                PasswordHash = "old-hash",
                MustChangePassword = true
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var uow = TestUowFactory.Create(db);
            var passwordHasher = new Mock<IPasswordHasher>();
            passwordHasher.Setup(h => h.Hash("NewPass123!")).Returns("new-hash");

            var email = new Mock<IEmailService>();
            var jwt = new Mock<IJwtTokenService>();
            var current = new Mock<ICurrentUserService>();
            var reset = new Mock<IPasswordResetService>();
            reset.Setup(r => r.VerifyAndConsumeAsync(It.Is<User>(u => u.Email == user.Email), "token-123", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);

            var frontend = Options.Create(new FrontendOptions { BaseUrl = "https://app.test", FirstLoginPath = "/first-login" });

            var svc = new UserService(uow, _mapper, passwordHasher.Object, email.Object, jwt.Object, current.Object, db, reset.Object, frontend);

            var dto = new FirstLoginPasswordSetupDto
            {
                Email = user.Email,
                Token = "token-123",
                NewPassword = "NewPass123!"
            };

            await svc.CompleteFirstLoginAsync(dto, CancellationToken.None);

            user.MustChangePassword.Should().BeFalse();
            user.PasswordHash.Should().Be("new-hash");
            user.PasswordChangedAt.Should().NotBeNull();
            reset.Verify(r => r.VerifyAndConsumeAsync(It.IsAny<User>(), "token-123", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
