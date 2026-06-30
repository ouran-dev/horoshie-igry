using System.Runtime.CompilerServices;
using HoroshieIgry.Games.LinkDotsGame.Models;

namespace HoroshieIgry.Games.LinkDotsGame.Helpers;

#if DEBUG
internal static class LinkDotsLevelValidation
{
    [ModuleInitializer]
    internal static void ValidateOnStartup()
    {
        for (var level = 1; level <= LinkDotsLevelFactory.MaxLevel; level++)
        {
            var definition = LinkDotsLevelFactory.CreateForLevel(level);
            if (!LinkDotsSolver.IsSolvable(definition))
                throw new InvalidOperationException($"Уровень {definition.Id} «{definition.Title}» нерешаем.");
        }
    }
}
#endif
