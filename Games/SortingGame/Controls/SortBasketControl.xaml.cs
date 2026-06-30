using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HoroshieIgry.Games.SortingGame.Models;

namespace HoroshieIgry.Games.SortingGame.Controls;

public partial class SortBasketControl : UserControl
{
    public static readonly DependencyProperty BasketPlanProperty =
        DependencyProperty.Register(nameof(BasketPlan), typeof(SortBasketPlan), typeof(SortBasketControl),
            new PropertyMetadata(null, OnBasketPlanChanged));

    public SortBasketPlan? BasketPlan
    {
        get => (SortBasketPlan?)GetValue(BasketPlanProperty);
        set => SetValue(BasketPlanProperty, value);
    }

    public string CategoryId => BasketPlan?.CategoryId ?? string.Empty;

    public SortBasketControl()
    {
        InitializeComponent();
    }

    public Point GetDropCenter()
    {
        var transform = BasketBody.TransformToAncestor(this);
        var point = transform.Transform(new Point(BasketBody.ActualWidth / 2, BasketBody.ActualHeight / 2));
        return new Point(ActualWidth / 2, point.Y);
    }

    public double DropRadius => Math.Max(ActualWidth, ActualHeight) * 0.55;

    public async Task PlaySuccessGlowAsync()
    {
        GlowBorder.Visibility = Visibility.Visible;

        var bounce = new DoubleAnimationUsingKeyFrames();
        bounce.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        bounce.KeyFrames.Add(new LinearDoubleKeyFrame(1.08, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(80))));
        bounce.KeyFrames.Add(new LinearDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))));
        BounceScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, bounce);
        BounceScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, bounce);

        await Task.Delay(200);
        GlowBorder.Visibility = Visibility.Collapsed;
    }

    public void PlayWrongWiggle()
    {
        var wiggle = new DoubleAnimationUsingKeyFrames
        {
            Duration = TimeSpan.FromMilliseconds(360)
        };
        wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
        wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(-8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(60))));
        wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(8, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(120))));
        wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(180))));
        wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(240))));
        wiggle.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(360))));
        WiggleTransform.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, wiggle);
    }

    private static void OnBasketPlanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SortBasketControl control && e.NewValue is SortBasketPlan plan)
        {
            control.IconText.Text = plan.IconEmoji;
            control.TitleText.Text = plan.Title;
        }
    }
}
