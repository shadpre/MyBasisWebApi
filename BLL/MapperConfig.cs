using AutoMapper;
using MyBasisWebApi.Logic.Models.Users;
using MyBasisWebApi.DataAccess;

namespace MyBasisWebApi.Logic
{
    /// <summary>
    /// Configuration for AutoMapper profiles.
    /// </summary>
    public class MapperConfig : Profile
    {
        /// <summary>
        /// Initializes a new instance of the MapperConfig class.
        /// </summary>
        public MapperConfig()
        {
            // Create mappings between ApiUserDto and ApiUser, and vice versa.
            CreateMap<ApiUserDto, ApiUser>().ReverseMap();
        }
    }
}