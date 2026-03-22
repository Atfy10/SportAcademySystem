using AutoMapper;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Mappings.TraineeProfile;

namespace SportAcademy.Tests.Application.Mappings;

public class AutoMapperFullTests
{
    private readonly IConfigurationProvider _config;
    private readonly IMapper _mapper;

    public AutoMapperFullTests()
    {
        var loggerFactory = LoggerFactory.Create(builder => { });
        _config = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(TraineeProfile).Assembly);
        }, loggerFactory);

        _mapper = _config.CreateMapper();
    }

    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        _config.AssertConfigurationIsValid();
    }

}