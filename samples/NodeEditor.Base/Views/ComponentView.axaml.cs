using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Media;
using NodeEditorDemo.Models;
using NodeEditorDemo.ViewModels;

namespace NodeEditorDemo.Views;

public partial class ComponentView : UserControl
{
    private Canvas? _canvas;

    public ComponentView()
    {
        InitializeComponent();
        _canvas = this.FindControl<Canvas>("PART_Canvas");
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        RebuildVisuals();
    }

    private void RebuildVisuals()
    {
        if (_canvas == null)
        {
            _canvas = this.FindControl<Canvas>("PART_Canvas");
        }
        if (_canvas == null) return;

        _canvas.Children.Clear();

        if (DataContext is not ComponentViewModel vm || vm.Definition == null)
        {
            return;
        }

        var def = vm.Definition;
        Width = def.Width;
        Height = def.Height;
        _canvas.Width = def.Width;
        _canvas.Height = def.Height;

        // 1. Render Outline Shapes
        foreach (var shape in def.Outline)
        {
            Control? element = null;

            if (shape.Type.Equals("Rectangle", StringComparison.OrdinalIgnoreCase))
            {
                var rect = new Rectangle
                {
                    Width = shape.Width,
                    Height = shape.Height,
                    RadiusX = shape.Radius,
                    RadiusY = shape.Radius,
                    Fill = ParseBrush(shape.Fill),
                    Stroke = ParseBrush(shape.Stroke),
                    StrokeThickness = shape.StrokeWidth
                };
                Canvas.SetLeft(rect, shape.X);
                Canvas.SetTop(rect, shape.Y);
                element = rect;
            }
            else if (shape.Type.Equals("Line", StringComparison.OrdinalIgnoreCase))
            {
                var line = new Line
                {
                    StartPoint = new Point(shape.X1, shape.Y1),
                    EndPoint = new Point(shape.X2, shape.Y2),
                    Stroke = ParseBrush(shape.Stroke) ?? Brushes.Gray,
                    StrokeThickness = shape.StrokeWidth
                };
                element = line;
            }
            else if (shape.Type.Equals("Ellipse", StringComparison.OrdinalIgnoreCase))
            {
                var ellipse = new Ellipse
                {
                    Width = shape.Width,
                    Height = shape.Height,
                    Fill = ParseBrush(shape.Fill),
                    Stroke = ParseBrush(shape.Stroke),
                    StrokeThickness = shape.StrokeWidth
                };
                Canvas.SetLeft(ellipse, shape.X);
                Canvas.SetTop(ellipse, shape.Y);
                element = ellipse;
            }
            else if (shape.Type.Equals("Path", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(shape.Data))
            {
                var path = new Avalonia.Controls.Shapes.Path
                {
                    Data = Geometry.Parse(shape.Data),
                    Fill = ParseBrush(shape.Fill),
                    Stroke = ParseBrush(shape.Stroke),
                    StrokeThickness = shape.StrokeWidth
                };
                element = path;
            }

            if (element != null)
            {
                _canvas.Children.Add(element);
            }
        }

        // 2. Render Mounting Holes
        foreach (var hole in def.MountingHoles)
        {
            // Exposed outer ground/mounting collar
            double collarRad = hole.CollarDiameter * 2.0;
            var collar = new Ellipse
            {
                Width = collarRad,
                Height = collarRad,
                Fill = new SolidColorBrush(Color.Parse("#B8BDBF")),
                Stroke = new SolidColorBrush(Color.Parse("#8A9094")),
                StrokeThickness = 1.0
            };
            Canvas.SetLeft(collar, hole.X - collarRad / 2.0);
            Canvas.SetTop(collar, hole.Y - collarRad / 2.0);
            _canvas.Children.Add(collar);

            // Inner drill through-hole
            double drillRad = hole.Diameter * 2.0;
            var drill = new Ellipse
            {
                Width = drillRad,
                Height = drillRad,
                Fill = new SolidColorBrush(Color.Parse("#1E1E1E"))
            };
            Canvas.SetLeft(drill, hole.X - drillRad / 2.0);
            Canvas.SetTop(drill, hole.Y - drillRad / 2.0);
            _canvas.Children.Add(drill);
        }

        // 3. Render Visual Pin Pads (Square for pin 1, Circle for others)
        foreach (var pin in def.Pins)
        {
            double padW = Math.Max(8.0, pin.PadWidth * 4.0);
            double padH = Math.Max(8.0, pin.PadHeight * 4.0);

            if (pin.PadShape.Equals("Rectangle", StringComparison.OrdinalIgnoreCase) || pin.Number == 1)
            {
                var pad = new Rectangle
                {
                    Width = padW,
                    Height = padH,
                    Fill = new SolidColorBrush(Color.Parse("#D4AF37")),
                    Stroke = new SolidColorBrush(Color.Parse("#8C7320")),
                    StrokeThickness = 1.0,
                    RadiusX = 1.0,
                    RadiusY = 1.0
                };
                Canvas.SetLeft(pad, pin.X - padW / 2.0);
                Canvas.SetTop(pad, pin.Y - padH / 2.0);
                _canvas.Children.Add(pad);
            }
            else
            {
                var pad = new Ellipse
                {
                    Width = padW,
                    Height = padH,
                    Fill = new SolidColorBrush(Color.Parse("#D4AF37")),
                    Stroke = new SolidColorBrush(Color.Parse("#8C7320")),
                    StrokeThickness = 1.0
                };
                Canvas.SetLeft(pad, pin.X - padW / 2.0);
                Canvas.SetTop(pad, pin.Y - padH / 2.0);
                _canvas.Children.Add(pad);
            }

            // Inner drill hole
            var innerHole = new Ellipse
            {
                Width = 3.5,
                Height = 3.5,
                Fill = new SolidColorBrush(Color.Parse("#1E1E1E"))
            };
            Canvas.SetLeft(innerHole, pin.X - 1.75);
            Canvas.SetTop(innerHole, pin.Y - 1.75);
            _canvas.Children.Add(innerHole);
        }

        // 4. Render Labels (Designator & Value)
        if (def.Labels?.Designator is { } des)
        {
            var tbDes = new TextBlock
            {
                FontSize = des.FontSize,
                Foreground = ParseBrush(des.Color) ?? Brushes.White,
                FontWeight = des.FontWeight.Equals("Bold", StringComparison.OrdinalIgnoreCase) ? FontWeight.Bold : FontWeight.Normal,
            };
            tbDes.Bind(TextBlock.TextProperty, new Binding(nameof(ComponentViewModel.Name)));
            Canvas.SetLeft(tbDes, des.X - 20.0);
            Canvas.SetTop(tbDes, des.Y);
            _canvas.Children.Add(tbDes);
        }

        if (def.Labels?.Value is { } val)
        {
            var tbVal = new TextBlock
            {
                FontSize = val.FontSize,
                Foreground = ParseBrush(val.Color) ?? Brushes.White,
                FontWeight = val.FontWeight.Equals("Bold", StringComparison.OrdinalIgnoreCase) ? FontWeight.Bold : FontWeight.Normal,
            };
            tbVal.Bind(TextBlock.TextProperty, new Binding(nameof(ComponentViewModel.Value)));
            Canvas.SetLeft(tbVal, val.X - 20.0);
            Canvas.SetTop(tbVal, val.Y);
            _canvas.Children.Add(tbVal);
        }

        // 5. Selection Highlight Indicator
        var selectionBorder = new Rectangle
        {
            Width = def.Width + 4.0,
            Height = def.Height + 4.0,
            Fill = Brushes.Transparent,
            Stroke = new SolidColorBrush(Color.Parse("#179DE3")),
            StrokeThickness = 2.0,
            StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 4, 2 },
            RadiusX = 2.0,
            RadiusY = 2.0
        };
        Canvas.SetLeft(selectionBorder, -2.0);
        Canvas.SetTop(selectionBorder, -2.0);
        selectionBorder.Bind(Visual.IsVisibleProperty, new Binding(nameof(ComponentViewModel.IsSelected)));
        _canvas.Children.Add(selectionBorder);
    }

    private static IBrush? ParseBrush(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return null;
        try
        {
            return new SolidColorBrush(Color.Parse(color));
        }
        catch
        {
            return null;
        }
    }
}
