// In Models/DTOs/MappingProfile.cs
using AutoMapper;

namespace DentalManagementAPI.Models.DTOs
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Booking, BookingResponseDto>();
            CreateMap<BookingDto, Booking>();
            CreateMap<PatientDto, PatientDto>();
            CreateMap<Doctor, DoctorDto>();
            CreateMap<AppointmentDTO, Appointment>();
            CreateMap<Appointment, AppointmentResponseDto>();
            CreateMap<DoctorsEarnings, DoctorEarningDTO>();
            CreateMap<AdminEarnings, AdminEarningsDto>();


        }
    }
}