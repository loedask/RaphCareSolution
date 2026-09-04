using MediatR;
using Microsoft.EntityFrameworkCore;
using RaphCare.Application.Common.Interfaces;
using RaphCare.Application.Features.MentalHealth.DTOs;
using RaphCare.Application.Features.Organization;
using RaphCare.Domain.MentalHealth;

namespace RaphCare.Application.Features.MentalHealth.Queries.GetMentalHealthAssessmentById;

public sealed class GetMentalHealthAssessmentByIdHandler(
    ICurrentUserService currentUserService,
    IClinicStaffMembershipService clinicStaffMembershipService,
    IRepository<MentalHealthAssessment> assessments)
    : IRequestHandler<GetMentalHealthAssessmentByIdQuery, MentalHealthAssessmentDetailDto?>
{
    public async Task<MentalHealthAssessmentDetailDto?> Handle(
        GetMentalHealthAssessmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        await AdminClinicAuthorization.EnsureClinicStaffAsync(
                currentUserService,
                clinicStaffMembershipService,
                request.ClinicId,
                "Only hospital staff can view mental health assessments.",
                cancellationToken)
            .ConfigureAwait(false);

        var page = await assessments.SearchAsync(
                q => q
                    .Where(a => a.Id == request.AssessmentId && a.ClinicId == request.ClinicId)
                    .Include(a => a.Questions)
                    .Include(a => a.Responses),
                pageNumber: 1,
                pageSize: 1,
                applyDefaultIdOrdering: false,
                cancellationToken)
            .ConfigureAwait(false);

        var assessment = page.Items.Count > 0 ? page.Items[0] : null;
        return assessment is null ? null : MentalHealthAssessmentMapper.ToDetailDto(assessment);
    }
}
