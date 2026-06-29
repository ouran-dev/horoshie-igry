using System.Reflection;

namespace HoroshieIgry.Core.Games;

/// <summary>
/// Автоматически находит все реализации <see cref="IGameModule"/> в сборке
/// и регистрирует их в каталоге.
/// </summary>
public static class GameCatalogRegistrar
{
    public static void RegisterAll(GameCatalog catalog)
    {
        var modules = DiscoverModules();

        foreach (var module in modules)
            catalog.Register(module);
    }

    private static IReadOnlyList<IGameModule> DiscoverModules()
    {
        var moduleType = typeof(IGameModule);
        var assembly = moduleType.Assembly;

        var modules = new List<IGameModule>();

        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface || !moduleType.IsAssignableFrom(type))
                continue;

            if (Activator.CreateInstance(type) is not IGameModule module)
            {
                throw new InvalidOperationException(
                    $"Тип {type.FullName} реализует {nameof(IGameModule)}, но не удалось создать экземпляр. " +
                    "Нужен публичный конструктор без параметров.");
            }

            modules.Add(module);
        }

        if (modules.Count == 0)
        {
            throw new InvalidOperationException(
                $"В сборке {assembly.GetName().Name} не найдено ни одной игры ({nameof(IGameModule)}).");
        }

        return modules
            .OrderBy(m => m.CatalogOrder)
            .ThenBy(m => m.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
