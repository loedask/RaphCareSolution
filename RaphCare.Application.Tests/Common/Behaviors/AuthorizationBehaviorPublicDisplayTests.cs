using RaphCare.Application.Common.Behaviors;
using RaphCare.Application.Common.Exceptions;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.Organization.Queries.GetCasualtyDisplay;
using RaphCare.Application.Features.Organization.Queries.GetCollectionDisplay;
using RaphCare.Application.Features.Organization.Queries.GetConsultDisplay;
using Xunit;

namespace RaphCare.Application.Tests.Common.Behaviors;

public sealed class AuthorizationBehaviorPublicDisplayTests
{
    [Fact]
    public async Task HandleAllowsAnonymousCollectionDisplayQueryWithoutSignIn()
    {
        var behavior = new AuthorizationBehavior<GetCollectionDisplayQuery, object?>(new AnonymousUser());
        var called = false;

        var result = await behavior.Handle(
            new GetCollectionDisplayQuery { Token = "U9GVH2ILM98P" },
            ct =>
            {
                called = true;
                return Task.FromResult<object?>(null);
            },
            CancellationToken.None);

        Assert.Null(result);
        Assert.True(called);
    }

    [Fact]
    public async Task HandleAllowsAnonymousCasualtyDisplayQueryWithoutSignIn()
    {
        var behavior = new AuthorizationBehavior<GetCasualtyDisplayQuery, object?>(new AnonymousUser());
        var called = false;

        await behavior.Handle(
            new GetCasualtyDisplayQuery { Token = "CASUALTYTOKEN1" },
            ct =>
            {
                called = true;
                return Task.FromResult<object?>(null);
            },
            CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task HandleAllowsAnonymousConsultDisplayQueryWithoutSignIn()
    {
        var behavior = new AuthorizationBehavior<GetConsultDisplayQuery, object?>(new AnonymousUser());
        var called = false;

        await behavior.Handle(
            new GetConsultDisplayQuery { Token = "CONSULTTOKEN12" },
            ct =>
            {
                called = true;
                return Task.FromResult<object?>(null);
            },
            CancellationToken.None);

        Assert.True(called);
    }

    [Fact]
    public async Task HandleRejectsNonAnonymousRequestWhenNotSignedIn()
    {
        var behavior = new AuthorizationBehavior<SecuredProbeRequest, int>(new AnonymousUser());

        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            behavior.Handle(
                new SecuredProbeRequest(),
                ct => Task.FromResult(1),
                CancellationToken.None));
    }

    private sealed class SecuredProbeRequest : MediatR.IRequest<int>;

    private sealed class AnonymousUser : ICurrentUserService
    {
        public string? UserId => null;
        public Guid? CurrentUserId => null;
        public string? UserName => null;
        public Guid? CurrentPatientId => null;
        public bool IsAuthenticated => false;
        public string? IpAddress => null;
        public string? UserAgent => null;
    }
}
