namespace HoroshieIgry.Core.UI;

/// <summary>Иллюстрации из <c>vector_objects.svg</c> (Kenney Background Elements).</summary>
public static class KenneyObjectPaths
{
    private const string Root = "Assets/Background Elements/Kenney";
    private const string VectorObjects = $"{Root}/Vector/vector_objects.svg";

    public static readonly IReadOnlyList<KenneyBackgroundScene> CardDecorations =
    [
        Scene(853, 233, 94, 204),   // tree
        Scene(0, 0, 281, 217),      // house2
        Scene(635, 1005, 84, 84),   // sun
        Scene(250, 365, 203, 121),  // cloud1
        Scene(424, 709, 200, 238),  // treePalm
        Scene(732, 0, 106, 254),    // treePine
        Scene(216, 726, 208, 224), // house1
        Scene(241, 509, 204, 200), // pyramid
        Scene(419, 950, 120, 60),  // bush1
        Scene(641, 511, 84, 84)    // moonFull
    ];

    public static KenneyBackgroundScene PickDecoration(int seed)
    {
        var index = Math.Abs(seed) % CardDecorations.Count;
        return CardDecorations[index];
    }

    private static KenneyBackgroundScene Scene(double x, double y, double width, double height)
        => new(VectorObjects, x, y, width, height);
}
