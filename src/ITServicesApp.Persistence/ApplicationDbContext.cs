using System.Linq;
using System.Reflection;
using ITServicesApp.Domain.Base;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ITServicesApp.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        private readonly AuditableEntityInterceptor? _auditableInterceptor;
        private readonly SoftDeleteInterceptor? _softDeleteInterceptor;
        private readonly AuditTrailSaveChangesInterceptor? _auditTrailInterceptor;

        // Runtime constructor (DI)
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            AuditableEntityInterceptor auditableInterceptor,
            SoftDeleteInterceptor softDeleteInterceptor,
            AuditTrailSaveChangesInterceptor auditTrailInterceptor)
            : base(options)
        {
            _auditableInterceptor = auditableInterceptor;
            _softDeleteInterceptor = softDeleteInterceptor;
            _auditTrailInterceptor = auditTrailInterceptor;
        }

        // Design-time constructor (no interceptors)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Technician> Technicians => Set<Technician>();
        public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
        public DbSet<ServiceIssue> ServiceIssues => Set<ServiceIssue>();
        public DbSet<TechnicianSlot> TechnicianSlots => Set<TechnicianSlot>();
        public DbSet<TechnicianUnavailability> TechnicianUnavailabilities => Set<TechnicianUnavailability>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public DbSet<BookingDraft> BookingDrafts => Set<BookingDraft>();
        public DbSet<BookingDraftItem> BookingDraftItems => Set<BookingDraftItem>();
        public DbSet<TechnicianExpertise> TechnicianExpertises => Set<TechnicianExpertise>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<TechnicianReview> TechnicianReviews => Set<TechnicianReview>();
        public DbSet<Notification> Notifications => Set<Notification>();
        // /Persistence/ApplicationDbContext.cs
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
        public DbSet<Message> Messages => Set<Message>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<TechnicianPayout> TechnicianPayouts => Set<TechnicianPayout>();
        public DbSet<ServiceReport> ServiceReports => Set<ServiceReport>();
        public DbSet<PlatformSettings> PlatformSettings => Set<PlatformSettings>();
        public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (_auditableInterceptor != null || _softDeleteInterceptor != null || _auditTrailInterceptor != null)
            {
                var interceptors = new List<IInterceptor>();
                if (_auditableInterceptor != null) interceptors.Add(_auditableInterceptor);
                if (_softDeleteInterceptor != null) interceptors.Add(_softDeleteInterceptor);
                if (_auditTrailInterceptor != null) interceptors.Add(_auditTrailInterceptor);
                optionsBuilder.AddInterceptors(interceptors.ToArray());
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.Entity<BookingItem>(entity =>
            {
                entity.HasOne(i => i.Booking)
                      .WithMany(b => b.Items)
                      .HasForeignKey(i => i.BookingId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.ServiceIssue)
                      .WithMany(si => si.BookingItems)
                      .HasForeignKey(i => i.ServiceIssueId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.LineTotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<TechnicianExpertise>(entity =>
            {
                entity.HasKey(e => new { e.TechnicianId, e.ServiceIssueId });

                entity.HasOne(e => e.Technician)
                      .WithMany(t => t.Expertise)
                      .HasForeignKey(e => e.TechnicianId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);

                entity.HasOne(e => e.ServiceIssue)
                      .WithMany(i => i.TechnicianExpertises)
                      .HasForeignKey(e => e.ServiceIssueId)
                      .OnDelete(DeleteBehavior.Cascade)
                      .IsRequired(false);
            });

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.Property(b => b.EstimatedTotal).HasPrecision(18, 2);
                entity.Property(b => b.FinalTotal).HasPrecision(18, 2);

                entity.HasOne(b => b.AddressEntity)
                      .WithMany()
                      .HasForeignKey(b => b.AddressId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Money precision convention
            foreach (var p in modelBuilder.Model
                     .GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                p.SetPrecision(18);
                p.SetScale(2);
            }

            // Global soft-delete filter (if entity has IsDeleted)
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType)
                    && entityType.FindProperty(nameof(AuditableEntity.IsDeleted)) != null)
                {
                    var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                    var prop = System.Linq.Expressions.Expression.Property(parameter, nameof(AuditableEntity.IsDeleted));
                    var compare = System.Linq.Expressions.Expression.Equal(prop, System.Linq.Expressions.Expression.Constant(false));
                    var lambda = System.Linq.Expressions.Expression.Lambda(compare, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}








