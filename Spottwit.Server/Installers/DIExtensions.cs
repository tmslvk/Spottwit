using Spottwit.Server.Installers;
using System.Reflection;

namespace MuslinkReact.Server.Extensions
{
    public static class DIExtensions
    {
        public static IServiceCollection InstallServicesFromAssembly(this IServiceCollection services, IConfiguration configuration)
        {
            var installers = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => typeof(IServiceInstaller).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<IServiceInstaller>();

            foreach (var installer in installers)
                installer.Install(services, configuration);

            return services;
        }
    }
}