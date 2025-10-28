using AutoMapper;
using Mess_Management_System_Backend.Models;
using Mess_Management_System_Backend.Dtos;

namespace Mess_Management_System_Backend.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // -------------------------------
            // Entity → DTO mappings
            // -------------------------------
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));

            // -------------------------------
            // DTO → Entity mappings
            // -------------------------------
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => MapRole(src.Role)))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLowerInvariant()))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Trim()));

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Role));
                    opt.MapFrom(src => MapRole(src.Role));
                })
                .ForMember(dest => dest.Email, opt =>
                {
                    opt.PreCondition(src => !string.IsNullOrWhiteSpace(src.Email));
                    opt.MapFrom(src => src.Email!.Trim().ToLowerInvariant());
                })
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName.Trim()))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName.Trim()));
        }

        // -------------------------------
        // Helpers
        // -------------------------------
        private static UserRole MapRole(string? roleString)
        {
            if (Enum.TryParse<UserRole>(roleString, true, out var parsed))
                return parsed;
            return UserRole.Student;
        }
    }
}
