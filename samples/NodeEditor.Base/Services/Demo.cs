using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NodeEditor.Model;
using NodeEditor.Mvvm;

namespace NodeEditorDemo.Services;

internal static class Demo
{
    private const double GridUnit = NodeFactory.BreadboardPitch; // 25.4mm / 2.54mm breadboard pitch

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
            SnapX = GridUnit,
            SnapY = GridUnit,
            EnableGrid = true,
            GridCellWidth = GridUnit,
            GridCellHeight = GridUnit,
        };

        // 1. Resistor (10kΩ) at (GridUnit * 1, GridUnit * 2)
        var resistor1 = NodeFactory.CreateResistor(GridUnit * 1, GridUnit * 2, "10kΩ");
        resistor1.Parent = drawing;
        drawing.Nodes.Add(resistor1);

        // 2. Diode (1N4007) at (GridUnit * 1, GridUnit * 5)
        var diode1 = NodeFactory.CreateDiode(GridUnit * 1, GridUnit * 5, "1N4007");
        diode1.Parent = drawing;
        drawing.Nodes.Add(diode1);

        // 3. Inductor (100µH) at (GridUnit * 1, GridUnit * 8)
        var inductor1 = NodeFactory.CreateInductor(GridUnit * 1, GridUnit * 8, "100µH");
        inductor1.Parent = drawing;
        drawing.Nodes.Add(inductor1);

        // 4. Ceramic Capacitor (100nF) at (GridUnit * 6, GridUnit * 2)
        var ceramicCap1 = NodeFactory.CreateCeramicCapacitor(GridUnit * 6, GridUnit * 2, "100nF");
        ceramicCap1.Parent = drawing;
        drawing.Nodes.Add(ceramicCap1);

        // 5. Electrolytic Capacitor (100µF) at (GridUnit * 9, GridUnit * 2)
        var cap1 = NodeFactory.CreateCapacitor(GridUnit * 9, GridUnit * 2, "100µF");
        cap1.Parent = drawing;
        drawing.Nodes.Add(cap1);

        // 6. LED (Red) at (GridUnit * 9, GridUnit * 5)
        var led1 = NodeFactory.CreateLED(GridUnit * 9, GridUnit * 5, "Red");
        led1.Parent = drawing;
        drawing.Nodes.Add(led1);

        // 7. Arduino Uno development board at (GridUnit * 13, GridUnit * 2)
        var uno = NodeFactory.CreateArduinoUno(GridUnit * 13, GridUnit * 2);
        uno.Parent = drawing;
        drawing.Nodes.Add(uno);

        // Connect components using curved Bezier traces
        if (resistor1.Pins?[1] is { } && ceramicCap1.Pins?[0] is { })
        {
            var conn1 = (BezierConnectorViewModel)NodeFactory.CreateBezierConnector(resistor1.Pins[1], ceramicCap1.Pins[0]);
            conn1.Parent = drawing;
            drawing.Connectors.Add(conn1);
            conn1.OnSelected();
            drawing.SetSelectedConnectors(new HashSet<ICommonConnector> { conn1 });
        }

        if (ceramicCap1.Pins?[1] is { } && cap1.Pins?[0] is { })
        {
            var conn2 = NodeFactory.CreateConnector(ceramicCap1.Pins[1], cap1.Pins[0], 20);
            conn2.Parent = drawing;
            drawing.Connectors.Add(conn2);
        }

        if (cap1.Pins?[1] is { } && led1.Pins?[0] is { })
        {
            var conn3 = NodeFactory.CreateConnector(cap1.Pins[1], led1.Pins[0], 20);
            conn3.Parent = drawing;
            drawing.Connectors.Add(conn3);
        }

        // Connect Arduino Uno D13 pin to LED anode via curved Bezier trace
        var d13Pin = uno.Pins?.FirstOrDefault(p => p.Name == "13");
        if (d13Pin != null && led1.Pins?[0] is { })
        {
            var connUno = (BezierConnectorViewModel)NodeFactory.CreateBezierConnector(d13Pin, led1.Pins[0]);
            connUno.Parent = drawing;
            drawing.Connectors.Add(connUno);
        }

        return drawing;
    }
}
