using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace cs2_rockthevote
{
    public class DependencyManager<TPlugin, TConfig>
    {
        private List<IPluginDependency<TPlugin, TConfig>> Dependencies { get; set; } = new();

        private List<Type> TypesToAdd { get; set; } = new();

        Type dependencyType = typeof(IPluginDependency<TPlugin, TConfig>);

        private readonly ILogger<DependencyManager<TPlugin, TConfig>> Logger;

        public DependencyManager(ILogger<DependencyManager<TPlugin, TConfig>> logger)
        {
            Logger = logger;
        }

        public void LoadDependencies(Assembly assembly)
        {

            var typesToAdd = assembly.GetTypes()
                .Where(x => x.IsClass)
                .Where(dependencyType.IsAssignableFrom);

            TypesToAdd.AddRange(typesToAdd);
        }

        public void AddIt(IServiceCollection collection)
        {
            foreach (var type in TypesToAdd)
            {
                collection.AddSingleton(type);
            }

            collection.AddSingleton(p =>
            {
                Dependencies = TypesToAdd
                    .Where(x => dependencyType.IsAssignableFrom(x))
                    .Select(type => (IPluginDependency<TPlugin, TConfig>)p.GetService(type)!)
                    .ToList();

                return this;
            });
        }

        public void OnMapStart(string mapName)
        {
            Logger.LogInformation($"DependencyManager: Map started, initializing {Dependencies.Count} dependencies");
            foreach (var service in Dependencies)
            {
                service.OnMapStart(mapName);
            }
            Logger.LogInformation("DependencyManager: All dependencies initialized for map start");
        }

        public void OnPluginLoad(TPlugin plugin)
        {
            Logger.LogInformation($"DependencyManager: Plugin loading, initializing {Dependencies.Count} dependencies");
            foreach (var service in Dependencies)
            {
                service.OnLoad(plugin);
            }
            Logger.LogInformation("DependencyManager: All dependencies initialized for plugin load");
        }

        public void OnConfigParsed(TConfig config)
        {
            Logger.LogInformation($"DependencyManager: Config parsed, updating {Dependencies.Count} dependencies");
            foreach (var service in Dependencies)
            {
                service.OnConfigParsed(config);
            }
            Logger.LogInformation("DependencyManager: All dependencies updated with new config");
        }
    }
}
