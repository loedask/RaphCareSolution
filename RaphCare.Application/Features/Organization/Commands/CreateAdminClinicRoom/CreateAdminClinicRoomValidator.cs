using FluentValidation;

namespace RaphCare.Application.Features.Organization.Commands.CreateAdminClinicRoom;

public sealed class CreateAdminClinicRoomValidator : AbstractValidator<CreateAdminClinicRoomCommand>
{
    public CreateAdminClinicRoomValidator()
    {
        RuleFor(x => x.ClinicId).NotEmpty();
        RuleFor(x => x.WardId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoomType).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.RoomType));
    }
}
