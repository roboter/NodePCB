using System.Collections.Generic;
using Avalonia;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using Xunit;

namespace NodeEditor.UnitTests;

public class BezierConnectorTests
{
    [Fact]
    public void BezierConnectorViewModel_InitializesControlPoints_WhenPinsSet()
    {
        var startPin = new PinViewModel { X = 10, Y = 20, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 100, Y = 200, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Start = startPin,
            End = endPin,
            Offset = 50
        };

        Assert.NotNull(connector.StartControl);
        Assert.NotNull(connector.EndControl);
        Assert.Equal(10 + 50, connector.StartControl.X);
        Assert.Equal(20, connector.StartControl.Y);
        Assert.Equal(100 - 50, connector.EndControl.X);
        Assert.Equal(200, connector.EndControl.Y);
    }

    [Fact]
    public void BezierConnectorViewModel_ResetControlPoints_RecalculatesPoints()
    {
        var startPin = new PinViewModel { X = 0, Y = 0, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 200, Y = 100, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Start = startPin,
            End = endPin,
            Offset = 30
        };

        // User manually moved control points
        connector.StartControl!.X = 80;
        connector.StartControl.Y = 80;
        Assert.Equal(80, connector.StartControl.X);
        Assert.Equal(80, connector.StartControl.Y);

        // Reset
        connector.ResetControlPoints();

        Assert.Equal(30, connector.StartControl.X);
        Assert.Equal(0, connector.StartControl.Y);
    }

    [Fact]
    public void HitTestHelper_HitTestControlPoint_FindsHandle()
    {
        var drawing = new DrawingNodeViewModel();
        var startPin = new PinViewModel { X = 10, Y = 20, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 100, Y = 200, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Parent = drawing,
            Start = startPin,
            End = endPin,
            Offset = 50
        };

        drawing.Connectors = new List<ICommonConnector> { connector };

        // Start control is at (60, 20)
        var (hitConnector, hitPin) = HitTestHelper.HitTestControlPoint(drawing, new Point(62, 21), radius: 10);

        Assert.Same(connector, hitConnector);
        Assert.Same(connector.StartControl, hitPin);
    }

    [Fact]
    public void HitTestHelper_HitTestConnector_HitsBezierCurve()
    {
        var startPin = new PinViewModel { X = 0, Y = 0, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 100, Y = 0, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Start = startPin,
            End = endPin,
            Offset = 50
        };

        // Middle of curve around (50, 0)
        var hit = HitTestHelper.HitTestConnector(connector, new Rect(48, -2, 4, 4));
        Assert.True(hit);

        // Clicking on handle directly
        var hitHandle = HitTestHelper.HitTestConnector(connector, new Rect(49, -1, 2, 2));
        Assert.True(hitHandle);
    }

    [Fact]
    public void BezierConnector_ControlPoint_TracksParentNode()
    {
        var node = new NodeViewModel { X = 100, Y = 50 };
        var startPin = new PinViewModel { Parent = node, X = 10, Y = 10, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 300, Y = 100, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Start = startPin,
            End = endPin,
            Offset = 40
        };

        Assert.NotNull(connector.StartControl);
        Assert.Same(node, connector.StartControl.Parent);
        Assert.Equal(50, connector.StartControl.X); // 10 + 40
        Assert.Equal(10, connector.StartControl.Y);

        var converter = new NodeEditor.Converters.PinToPointConverter();
        var canvasPoint = (Point)converter.Convert(connector.StartControl, typeof(Point), null!, System.Globalization.CultureInfo.InvariantCulture)!;
        Assert.Equal(150, canvasPoint.X); // 100 (node X) + 50 (control X)
        Assert.Equal(60, canvasPoint.Y);  // 50 (node Y) + 10 (control Y)

        // Move node
        node.X += 20;
        node.Y += 30;

        var canvasPointAfterMove = (Point)converter.Convert(connector.StartControl, typeof(Point), null!, System.Globalization.CultureInfo.InvariantCulture)!;
        Assert.Equal(170, canvasPointAfterMove.X);
        Assert.Equal(90, canvasPointAfterMove.Y);
    }

    [Fact]
    public void DrawingNode_SelectConnector_SetsIsSelected_AndCalculateSelectedRectReturnsDefault()
    {
        var drawing = new DrawingNodeViewModel();
        var startPin = new PinViewModel { X = 0, Y = 0, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 100, Y = 100, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Parent = drawing,
            Start = startPin,
            End = endPin,
            Offset = 50
        };

        drawing.Connectors = new List<ICommonConnector> { connector };

        Assert.False(connector.IsSelected);

        // Select the connector
        drawing.SetSelectedConnectors(new HashSet<ICommonConnector> { connector });

        Assert.True(connector.IsSelected);

        // CalculateSelectedRect must NOT include connector bounding box (must return default Rect)
        var rect = HitTestHelper.CalculateSelectedRect(null);
        Assert.Equal(default, rect);

        // Deselect connector
        drawing.SetSelectedConnectors(null);
        Assert.False(connector.IsSelected);
    }

    [Fact]
    public void BezierConnectorViewModel_MovingControlPoint_NotifiesPropertyChanged()
    {
        var startPin = new PinViewModel { X = 0, Y = 0, Alignment = PinAlignment.Right };
        var endPin = new PinViewModel { X = 100, Y = 100, Alignment = PinAlignment.Left };

        var connector = new BezierConnectorViewModel
        {
            Start = startPin,
            End = endPin,
            Offset = 50
        };

        var notifiedProps = new List<string?>();
        connector.PropertyChanged += (s, e) => notifiedProps.Add(e.PropertyName);

        connector.StartControl!.X = 75;
        connector.StartControl.OnMoved();

        Assert.Contains(nameof(connector.StartControl), notifiedProps);
    }
}

