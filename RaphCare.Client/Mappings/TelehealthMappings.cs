using System.Linq;
using AutoMapper;
using RaphCare.Client.Models.Telehealth;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Mappings;

public class TelehealthMappings : Profile
{
    public TelehealthMappings()
    {
        CreateMap<PatientTeleSessionListItemDto, PatientTeleSessionListItemViewModel>();

        CreateMap<PatientTeleSessionListItemDtoPagedResult, PagedPatientTeleSessionsViewModel>()
            .ForMember(d => d.Items, o => o.Ignore())
            .ForMember(d => d.TotalCount, o => o.Ignore())
            .ForMember(d => d.PageNumber, o => o.Ignore())
            .ForMember(d => d.PageSize, o => o.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                dest.TotalCount = src.TotalCount;
                dest.PageNumber = src.PageNumber;
                dest.PageSize = src.PageSize;
                dest.Items = src.Items is null || src.Items.Count == 0
                    ? Array.Empty<PatientTeleSessionListItemViewModel>()
                    : ctx.Mapper.Map<IReadOnlyList<PatientTeleSessionListItemViewModel>>(src.Items.ToList());
            });

        CreateMap<TelehealthJoinInfoDto, TelehealthJoinInfoViewModel>()
            .ForMember(d => d.Uid, o => o.MapFrom(s => unchecked((uint)s.Uid)))
            .ForMember(d => d.ChannelName, o => o.MapFrom(s => s.ChannelName ?? string.Empty))
            .ForMember(d => d.AppId, o => o.MapFrom(s => s.AppId))
            .ForMember(d => d.RtcToken, o => o.MapFrom(s => s.RtcToken))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status ?? string.Empty));
    }
}
