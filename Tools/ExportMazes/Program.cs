using HoroshieIgry.Core.Mazes;

var target = args.Length > 0
    ? args[0]
    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Assets", "Mazes"));

MazeGenerator.ExportToDirectory(target);
Console.WriteLine($"Экспортировано 20 уровней в: {target}");
