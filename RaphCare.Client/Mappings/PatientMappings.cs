using AutoMapper;
using RaphCare.Client.Models;
using RaphCare.Client.Services.Base;

namespace RaphCare.Client.Mappings;

public class PatientMappings : Profile
{
    public PatientMappings()
    {
        CreateMap<PatientDto, PatientViewModel>();
        CreateMap<PagedResultOfPatientDto, PagedResultViewModel<PatientViewModel>>()
            .ForMember(d => d.Items, o => o.MapFrom(s => s.Items));
        CreateMap<CreatePatientRequest, CreatePatientCommand>();
        CreateMap<UpdatePatientRequest, UpdatePatientCommand>();
    }
}
