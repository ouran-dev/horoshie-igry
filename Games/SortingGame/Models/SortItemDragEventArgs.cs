using System.Windows.Input;
using HoroshieIgry.Games.SortingGame.Controls;

namespace HoroshieIgry.Games.SortingGame.Models;

public sealed class SortItemDragEventArgs : EventArgs
{
    public required SortItemControl Item { get; init; }
    public TouchDevice? TouchDevice { get; init; }
}
