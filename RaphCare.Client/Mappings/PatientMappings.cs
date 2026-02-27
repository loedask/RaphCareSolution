using AutoMapper;
using RaphCare.Client.Generated;
using RaphCare.Client.Models;

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
