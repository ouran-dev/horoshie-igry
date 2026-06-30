using System.Windows.Media;

namespace HoroshieIgry.Games.LinkDotsGame.Models;

public sealed class LinkDotsPair
{
    public required int ColorId { get; init; }
    public required int StartRow { get; init; }
    public required int StartCol { get; init; }
    public required int EndRow { get; init; }
    public required int EndCol { get; init; }
    public required Color PathColor { get; init; }
    public required Color DotColor { get; init; }

    public bool IsEndpoint(int row, int col)
        => (StartRow == row && StartCol == col) || (EndRow == row && EndCol == col);
}
