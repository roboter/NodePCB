using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;

namespace NodeEditorDemo.Services;

internal static class Demo
{
    private const double Inch = 2.54;
    public static IDrawingNode CreateDemoDrawing()
    {

        var drawing = new DrawingNodeViewModel
        {
            X = 0,
            Y = 0,
            Width = 900,
            Height = 600,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<ICommonConnector>(),
            EnableMultiplePinConnections = false,
            EnableSnap = true,
            SnapX = Inch * 10,
            SnapY = Inch * 10,
            EnableGrid = true,
            GridCellWidth = Inch * 10,
            GridCellHeight = Inch * 10,
        };

        // Create some basic components first
        var rectangle0 = NodeFactory.CreateRectangle(30, 30, 60, 60, "rect0");
        rectangle0.Parent = drawing;
        drawing.Nodes.Add(rectangle0);

        var rectangle1 = NodeFactory.CreateRectangle(240, 30, 60, 60, "rect1");
        rectangle1.Parent = drawing;
        drawing.Nodes.Add(rectangle1);

        if (rectangle0.Pins?[1] is { } && rectangle1.Pins?[0] is { })
        {
            var connector0 = NodeFactory.CreateConnector(rectangle0.Pins[1], rectangle1.Pins[0], 20);
            connector0.Parent = drawing;
            drawing.Connectors.Add(connector0);
        }

        // Add PCB electronic components
        var resistor1 = NodeFactory.CreateResistor(30, 120, "10kΩ", "5%");
        resistor1.Parent = drawing;
        drawing.Nodes.Add(resistor1);

        var capacitor1 = NodeFactory.CreateCapacitor(200, 120, "100µF", "25V");
        capacitor1.Parent = drawing;
        drawing.Nodes.Add(capacitor1);

        var led1 = NodeFactory.CreateLED(350, 100, "Red");
        led1.Parent = drawing;
        drawing.Nodes.Add(led1);

        var transistor1 = NodeFactory.CreateTransistor(30, 220, "2N2222");
        transistor1.Parent = drawing;
        drawing.Nodes.Add(transistor1);

        var arduino1 = NodeFactory.CreateMicrocontroller(200, 220, "Arduino Nano");
        arduino1.Parent = drawing;
        drawing.Nodes.Add(arduino1);

        // Connect some components
        if (resistor1.Pins?[1] is { } && capacitor1.Pins?[0] is { })
        {
            var connector1 = NodeFactory.CreateConnector(resistor1.Pins[1], capacitor1.Pins[0], 20);
            connector1.Parent = drawing;
            drawing.Connectors.Add(connector1);
        }

        if (capacitor1.Pins?[1] is { } && led1.Pins?[0] is { })
        {
            var connector2 = NodeFactory.CreateConnector(capacitor1.Pins[1], led1.Pins[0], 20);
            connector2.Parent = drawing;
            drawing.Connectors.Add(connector2);
        }

        //var rectangle2 = NodeFactory.CreateRectangle(30, 150, 60, 60, "rect2");
        //rectangle2.Parent = drawing;
        //drawing.Nodes.Add(rectangle2);

        //var ellipse0 = NodeFactory.CreateEllipse(240, 150, 60, 60, "ellipse0");
        //ellipse0.Parent = drawing;
        //drawing.Nodes.Add(ellipse0);

        //var signal0 = NodeFactory.CreateSignal(x: 30, y: 270, label: "in0", state: true);
        //signal0.Parent = drawing;
        //drawing.Nodes.Add(signal0);

        //var signal1 = NodeFactory.CreateSignal(x: 30, y: 390, label: "in1", state: false);
        //signal1.Parent = drawing;
        //drawing.Nodes.Add(signal1);

        //var signal2 = NodeFactory.CreateSignal(x: 420, y: 375, label: "out0", state: true);
        //signal2.Parent = drawing;
        //drawing.Nodes.Add(signal2);

        //var orGate0 = NodeFactory.CreateOrGate(300, 360);
        //orGate0.Parent = drawing;
        //drawing.Nodes.Add(orGate0);

        //if (signal0.Pins?[1] is { } && orGate0.Pins?[2] is { })
        //{
        //    var connector0 = NodeFactory.CreateConnector(signal0.Pins[1], orGate0.Pins[2]);
        //    connector0.Parent = drawing;
        //    drawing.Connectors.Add(connector0);
        //}

        //if (signal1.Pins?[1] is { } && orGate0.Pins?[0] is { })
        //{
        //    var connector0 = NodeFactory.CreateConnector(signal1.Pins[1], orGate0.Pins[0]);
        //    connector0.Parent = drawing;
        //    drawing.Connectors.Add(connector0);
        //}

        //if (orGate0.Pins?[1] is { } && signal2.Pins?[0] is { })
        //{
        //    var connector1 = NodeFactory.CreateConnector(orGate0.Pins[1], signal2.Pins[0]);
        //    connector1.Parent = drawing;
        //    drawing.Connectors.Add(connector1);
        //}

        var start = 25;
        var pinsOnSide = 16;
        var nextPin = 15;
        var pinWidth = 0.35 * 10;
        var pinHeight = 1.35 * 10;

        var side = 9.85 * 10;// pinsOnSide * nextPin + pinWidth + start * 2;
        var rectangleStm = NodeFactory.CreateChip(12, 12, side, side, "LQFP64", pinWidth, pinHeight);
        rectangleStm.Parent = drawing;
        drawing.Nodes.Add(rectangleStm);

        // TODO: How to move VIA?
        var via1 = NodeFactory.CreateVia(120, 12, 60, 60, "v1");
        via1.Parent = drawing;
        drawing.Nodes.Add(via1);

        var pin1 = NodeFactory.CreatePin(Inch * 10 - Inch * 5, Inch * 10 - Inch * 5, Inch * 10, Inch * 10, "p1");
        pin1.Parent = drawing;
        drawing.Nodes.Add(pin1);


        return drawing;
    }
}
