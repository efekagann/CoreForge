using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CoreForge.Application.Tests.Helpers;

internal static class MapperFactory
{
    public static IMapper Create(params Assembly[] assemblies)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg =>
        {
            foreach (var asm in assemblies)
                cfg.AddMaps(asm);
        });
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }
}
