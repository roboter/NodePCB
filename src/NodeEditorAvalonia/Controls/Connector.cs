using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using NodeEditor.Model;

namespace NodeEditor.Controls;

[PseudoClasses(":selected")]
public class OffsetConnector
    : Shape
{
    public static readonly StyledProperty<Point> StartPointProperty = AvaloniaProperty.Register<OffsetConnector, Point>(nameof(StartPoint));

    public static readonly StyledProperty<Point> EndPointProperty = AvaloniaProperty.Register<OffsetConnector, Point>(nameof(EndPoint));

    public static readonly StyledProperty<double> OffsetProperty = AvaloniaProperty.Register<OffsetConnector, double>(nameof(Offset));

    static OffsetConnector()
    {
        StrokeThicknessProperty.OverrideDefaultValue<OffsetConnector>(1);
        AffectsGeometry<OffsetConnector>(StartPointProperty, EndPointProperty, OffsetProperty);
    }

    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public double Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    protected override Geometry CreateDefiningGeometry()
    {
        var geometry = new StreamGeometry();

        using var context = geometry.Open();

        context.BeginFigure(StartPoint, false);

        if (DataContext is ICommonConnector connector)
        {
            var p1X = StartPoint.X;
            var p1Y = StartPoint.Y;
            var p2X = EndPoint.X;
            var p2Y = EndPoint.Y;

            connector.GetControlPoints(
                connector.Orientation,
                Offset,
                connector.Start?.Alignment ?? PinAlignment.None,
                connector.End?.Alignment ?? PinAlignment.None,
                ref p1X, ref p1Y,
                ref p2X, ref p2Y);

            context.CubicBezierTo(new Point(p1X, p1Y), new Point(p2X, p2Y), EndPoint);
        }
        else
        {
            context.CubicBezierTo(StartPoint, EndPoint, EndPoint);
        }

        context.EndFigure(false);

        return geometry;
    }
}


[PseudoClasses(":selected")]
public class BezierConnector : Control
{
    public static readonly StyledProperty<Point> StartPointProperty =
        AvaloniaProperty.Register<BezierConnector, Point>(nameof(StartPoint));

    public static readonly StyledProperty<Point> EndPointProperty =
        AvaloniaProperty.Register<BezierConnector, Point>(nameof(EndPoint));

    public static readonly StyledProperty<Point> StartControlPointProperty =
        AvaloniaProperty.Register<BezierConnector, Point>(nameof(StartControlPoint));

    public static readonly StyledProperty<Point> EndControlPointProperty =
        AvaloniaProperty.Register<BezierConnector, Point>(nameof(EndControlPoint));

    public static readonly StyledProperty<IBrush?> StrokeProperty =
        AvaloniaProperty.Register<BezierConnector, IBrush?>(nameof(Stroke), new ImmutableSolidColorBrush(Colors.Red));

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<BezierConnector, double>(nameof(StrokeThickness), 2.0);

    public static readonly StyledProperty<IBrush?> HandleStrokeProperty =
        AvaloniaProperty.Register<BezierConnector, IBrush?>(nameof(HandleStroke));

    public static readonly StyledProperty<IBrush?> HandleFillProperty =
        AvaloniaProperty.Register<BezierConnector, IBrush?>(nameof(HandleFill));

    public static readonly StyledProperty<double> HandleRadiusProperty =
        AvaloniaProperty.Register<BezierConnector, double>(nameof(HandleRadius), 6.0);

    public static readonly StyledProperty<bool> ShowControlPointsProperty =
        AvaloniaProperty.Register<BezierConnector, bool>(nameof(ShowControlPoints), false);

    static BezierConnector()
    {
        AffectsRender<BezierConnector>(
            StartPointProperty,
            EndPointProperty,
            StartControlPointProperty,
            EndControlPointProperty,
            StrokeProperty,
            StrokeThicknessProperty,
            ShowControlPointsProperty,
            HandleStrokeProperty,
            HandleFillProperty,
            HandleRadiusProperty);
    }

    public BezierConnector()
    {
        ClipToBounds = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowControlPointsProperty)
        {
            PseudoClasses.Set(":selected", change.GetNewValue<bool>());
            InvalidateVisual();
        }
    }

    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    public Point EndPoint
    {
        get => GetValue(EndPointProperty);
        set => SetValue(EndPointProperty, value);
    }

    public Point StartControlPoint
    {
        get => GetValue(StartControlPointProperty);
        set => SetValue(StartControlPointProperty, value);
    }

    public Point EndControlPoint
    {
        get => GetValue(EndControlPointProperty);
        set => SetValue(EndControlPointProperty, value);
    }

    public IBrush? Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush? HandleStroke
    {
        get => GetValue(HandleStrokeProperty);
        set => SetValue(HandleStrokeProperty, value);
    }

    public IBrush? HandleFill
    {
        get => GetValue(HandleFillProperty);
        set => SetValue(HandleFillProperty, value);
    }

    public double HandleRadius
    {
        get => GetValue(HandleRadiusProperty);
        set => SetValue(HandleRadiusProperty, value);
    }

    public bool ShowControlPoints
    {
        get => GetValue(ShowControlPointsProperty);
        set => SetValue(ShowControlPointsProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var isSelected = Classes.Contains(":selected") || ShowControlPoints;
        var strokeBrush = isSelected
            ? new ImmutableSolidColorBrush(Color.FromRgb(0x17, 0x9D, 0xE3))
            : (Stroke ?? new ImmutableSolidColorBrush(Colors.Red));
        var thickness = StrokeThickness > 0 ? StrokeThickness : 2.0;

        // 1. Draw the Bezier curve
        var curvePen = new ImmutablePen(strokeBrush.ToImmutable(), thickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(StartPoint, false);
            ctx.CubicBezierTo(StartControlPoint, EndControlPoint, EndPoint);
            ctx.EndFigure(false);
        }
        context.DrawGeometry(null, curvePen, geometry);

        // 2. Draw handles and control points if selected
        if (isSelected)
        {
            var handleLineBrush = new ImmutableSolidColorBrush(Color.FromArgb(200, 23, 157, 227));
            var dashStyle = new ImmutableDashStyle(new double[] { 4, 3 }, 0);
            var handleLinePen = new ImmutablePen(handleLineBrush, 1.5, dashStyle);

            // Line from curve point (StartPoint) to control point (StartControlPoint)
            context.DrawLine(handleLinePen, StartPoint, StartControlPoint);

            // Line from curve point (EndPoint) to control point (EndControlPoint)
            context.DrawLine(handleLinePen, EndPoint, EndControlPoint);

            // Control point handles (circles)
            var radius = HandleRadius > 0 ? HandleRadius : 6.0;
            var handleFill = HandleFill ?? new ImmutableSolidColorBrush(Colors.White);
            var handleStroke = HandleStroke ?? new ImmutableSolidColorBrush(Color.FromArgb(255, 23, 157, 227));
            var handlePen = new ImmutablePen(handleStroke.ToImmutable(), 2.0);

            context.DrawEllipse(handleFill, handlePen, StartControlPoint, radius, radius);
            context.DrawEllipse(handleFill, handlePen, EndControlPoint, radius, radius);
        }
    }
}

