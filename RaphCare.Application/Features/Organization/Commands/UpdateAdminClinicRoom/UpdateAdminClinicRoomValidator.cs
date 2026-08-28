using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.UpdateAdminClinicRoom;

public sealed class UpdateAdminClinicRoomValidator : AbstractValidator<UpdateAdminClinicRoomCommand>
{
    public UpdateAdminClinicRoomValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoomType).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.RoomType));
    }
}
