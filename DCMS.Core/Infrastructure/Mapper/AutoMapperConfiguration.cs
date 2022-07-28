using AutoMapper;

namespace DCMS.Core.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper 配置
    /// </summary>
    public static class AutoMapperConfiguration
    {
        public static IMapper Mapper { get; private set; }

        public static MapperConfiguration MapperConfiguration { get; private set; }

        public static void Init(MapperConfiguration config)
        {
            MapperConfiguration = config;
            Mapper = config.CreateMapper();
        }
    }

}