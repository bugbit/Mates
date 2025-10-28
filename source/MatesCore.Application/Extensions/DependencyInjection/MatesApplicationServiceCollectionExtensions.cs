
using MatesCore.Application.Explain;
using Mates.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class MatesApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMatesApplication(this IServiceCollection services)
        => services.AddSingleton<INaturalNumberExplain, NaturalNumberExplain>();
}
