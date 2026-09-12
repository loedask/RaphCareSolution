using RaphCare.Application.Common.DTOs;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Commands.CompleteAdminClinicVisit;
using RaphCare.Application.Features.Organization.Commands.MarkAdminClinicVisitInvoicePaid;
using RaphCare.Application.Features.Organization.DTOs;
using RaphCare.Domain.Billing;
using RaphCare.Domain.Clinical;
using RaphCare.Domain.Identity;
using RaphCare.Domain.Organization;
using RaphCare.Domain.Patients;
using Xunit;

namespace RaphCare.Application.Tests.Features.Organization.Commands.CompleteAdminClinicVisit;

public sealed class CompleteAdminClinicVisitBillingHandlerTests
{
    [Fact]
    public async Task HandleCreatesVisitInvoiceWhenBillAmountProvided()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111201");
        var visitId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var appointmentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var now = new DateTime(2026, 9, 12, 14, 0, 0, DateTimeKind.Utc);

        var visit = new Visit
        {
            ClinicId = clinicId,
            AppointmentId = appointmentId,
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            VisitStart = now.AddMinutes(-30),
            VisitType = "InPerson",
            Status = "InProgress"
        };
        EntityId.SetId(visit, visitId);

        var appointment = new Appointment
        {
            ClinicId = clinicId,
            PatientId = patientId,
            ProviderId = visit.ProviderId,
            ScheduledStart = now.AddHours(-1),
            ScheduledEnd = now,
            Type = "InPerson",
            Status = "InProgress"
        };
        EntityId.SetId(appointment, appointmentId);

        var patient = new Patient { FirstName = "Amina", LastName = "K." };
        EntityId.SetId(patient, patientId);

        var invoices = new FakeRepository<Invoice>();
        var lines = new FakeRepository<InvoiceLineItem>();

        var handler = new CompleteAdminClinicVisitHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Doctor]),
            new FakeRepository<Visit>([visit]),
            new FakeRepository<Appointment>([appointment]),
            new FakeRepository<VitalSignRecord>(),
            new FakeRepository<Patient>([patient]),
            new FakeRepository<Provider>(),
            invoices,
            lines,
            new FakeRepository<AppointmentConsent>(),
            new FakeProfessionalUsers([]),
            new FakePatientQuery(),
            new FakeClock(now),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new CompleteAdminClinicVisitCommand
            {
                ClinicId = clinicId,
                VisitId = visitId,
                Summary = "Seen today",
                BillAmount = 250,
                BillDescription = "Consultation",
                MarkPaid = false,
                Currency = "zar"
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Completed", visit.Status);
        var invoice = Assert.Single(invoices.Items);
        Assert.Equal(visitId, invoice.VisitId);
        Assert.Equal(250, invoice.Amount);
        Assert.Equal("ZAR", invoice.Currency);
        Assert.Equal("Pending", invoice.Status);
        Assert.Null(invoice.PaymentMethod);
        Assert.Null(invoice.PaidAt);
        Assert.Equal(invoice.Id, result!.InvoiceId);
        Assert.Equal(250, result.InvoiceAmount);
        Assert.Equal("Pending", result.InvoiceStatus);
        var line = Assert.Single(lines.Items);
        Assert.Equal("Consultation", line.ServiceType);
        Assert.Equal("Consultation", line.Description);
        Assert.Equal(250, line.TotalPrice);
    }

    [Fact]
    public async Task HandleMarkPaidSetsCashPaidOnVisitInvoice()
    {
        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111202");
        var visitId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var patientId = Guid.Parse("33333333-3333-3333-3333-333333333302");
        var now = new DateTime(2026, 9, 12, 15, 0, 0, DateTimeKind.Utc);

        var invoice = new Invoice
        {
            ClinicId = clinicId,
            PatientId = patientId,
            VisitId = visitId,
            Amount = 180,
            Currency = "ZAR",
            DueDate = now.Date,
            Status = "Pending"
        };
        var invoices = new FakeRepository<Invoice>([invoice]);
        var lines = new FakeRepository<InvoiceLineItem>(
        [
            new InvoiceLineItem
            {
                InvoiceId = invoice.Id,
                ServiceType = "Consultation",
                Description = "Consultation",
                Quantity = 1,
                UnitPrice = 180,
                TotalPrice = 180
            }
        ]);
        var patient = new Patient { FirstName = "Jean", LastName = "M." };
        EntityId.SetId(patient, patientId);

        var handler = new MarkAdminClinicVisitInvoicePaidHandler(
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeMembership(true),
            new FakeRoles([RaphCareRoles.Administrator]),
            invoices,
            lines,
            new FakeRepository<Patient>([patient]),
            new FakeClock(now),
            new FakeUnitOfWork());

        var result = await handler.Handle(
            new MarkAdminClinicVisitInvoicePaidCommand
            {
                ClinicId = clinicId,
                InvoiceId = invoice.Id
            },
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Paid", invoice.Status);
        Assert.Equal("Cash", invoice.PaymentMethod);
        Assert.Equal(now, invoice.PaidAt);
        Assert.Equal("Paid", result!.Status);
        Assert.Equal(now, result.PaidAt);
        Assert.Equal(visitId, result.VisitId);
    }
}

file static class EntityId
{
    public static void SetId<T>(T entity, Guid id) where T : class
    {
        var property = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException($"{typeof(T).Name} has no Id.");
        property.SetValue(entity, id);
    }
}

file sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
{
    public DateTime UtcNow { get; } = utcNow;
}

file sealed class FakeUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
}

file sealed class FakeCurrentUser(Guid userId) : ICurrentUserService
{
    public string? UserId { get; } = userId.ToString();
    public Guid? CurrentUserId { get; } = userId;
    public string? UserName => "tester";
    public Guid? CurrentPatientId => null;
    public bool IsAuthenticated => true;
    public string? IpAddress => null;
    public string? UserAgent => null;
}

file sealed class FakeMembership(bool hasMembership) : IClinicStaffMembershipService
{
    public Task EnsureMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task<IReadOnlyList<Guid>> GetClinicIdsForUserAsync(Guid applicationUserId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Guid>>([]);

    public Task<bool> HasMembershipAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(hasMembership);

    public Task<int> GetActiveStaffCountAsync(Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.FromResult(1);

    public Task<IReadOnlyList<ClinicStaffMembershipEntry>> GetStaffMembershipsAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<ClinicStaffMembershipEntry>>([]);

    public Task<bool> DeactivateMembershipAsync(
        Guid applicationUserId,
        Guid clinicId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task RecordInvitationSentAsync(Guid applicationUserId, Guid clinicId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class FakeRoles(IReadOnlyList<string> roles) : IUserRoleAssignmentService
{
    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(roles);

    public Task AssignRoleIfMissingAsync(Guid userId, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task RemoveRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task SetStaffJobRoleAsync(Guid userId, string jobRole, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}

file sealed class FakeProfessionalUsers(IReadOnlyList<ApplicationUser> users) : IProfessionalUserLookupService
{
    public Task<IReadOnlyList<ApplicationUser>> GetUsersByIdsAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(users);

    public Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult<ApplicationUser?>(null);

    public Task<bool> IsProfessionalAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);

    public Task<bool> HasSuccessfulLoginAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(true);
}

file sealed class FakePatientQuery : IAdminClinicPatientQueryService
{
    public Task<PagedResult<AdminClinicPatientListItemDto>> GetPatientsAsync(
        Guid clinicId,
        int pageNumber,
        int pageSize,
        string? search = null,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new PagedResult<AdminClinicPatientListItemDto>
        {
            Items = [],
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        });

    public Task<AdminClinicPatientDetailDto?> GetPatientDetailAsync(
        Guid clinicId,
        Guid patientId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<AdminClinicPatientDetailDto?>(null);

    public Task<AdminClinicVisitClinicalDocumentationDto> GetVisitClinicalDocumentationAsync(
        Guid visitId,
        DateTime visitStart,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(new AdminClinicVisitClinicalDocumentationDto());
}

file sealed class FakeRepository<T> : IRepository<T> where T : class
{
    public List<T> Items { get; }

    public FakeRepository(IEnumerable<T>? items = null)
    {
        Items = items?.ToList() ?? [];
    }

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var match = Items.FirstOrDefault(item =>
        {
            var value = typeof(T).GetProperty("Id")?.GetValue(item);
            return value is Guid guid && guid == id;
        });
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<T>>(Items);

    public Task<PagedResult<T>> SearchAsync(
        Func<IQueryable<T>, IQueryable<T>>? queryShaper,
        int pageNumber,
        int pageSize,
        bool applyDefaultIdOrdering = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> query = Items.AsQueryable();
        if (queryShaper is not null)
            query = queryShaper(query);
        var list = query.ToList();
        return Task.FromResult(new PagedResult<T>
        {
            Items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            TotalCount = list.Count,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        Items.Remove(entity);
        return Task.CompletedTask;
    }
}
