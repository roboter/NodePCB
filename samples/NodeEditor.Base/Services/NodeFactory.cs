using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.ViewModels.Nodes;

namespace NodeEditorDemo.Services;

public class NodeFactory : INodeFactory
{
    internal static INode CreateRectangle(double x, double y, double width, double height, string? label, double pinSize = 10)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = label }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreateChip(double x, double y, double width, double height, string? label, double pinWidth, double pinHight)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = label }
        };
        var start = 12.5;
        var pinsOnSide = 16;
        var nextPin = 5;
        for (int i = 0; i != pinsOnSide; i++)
        {
            node.AddPin(0, start + i * nextPin, pinHight, pinWidth, PinAlignment.Left, $"P{i}");
        }
        for (int i = 0; i != pinsOnSide; i++)
        {
            node.AddPin(start+i * nextPin, width, pinWidth, pinHight, PinAlignment.Bottom, $"P{pinsOnSide *1 + i}");
        }

        for (int i = 0; i != pinsOnSide; i++)
        {
            node.AddPin(width, start + i * nextPin, pinHight, pinWidth, PinAlignment.Right, $"P{pinsOnSide * 2 + i}");
        }

        for (int i = 0; i != pinsOnSide; i++)
        {
            node.AddPin(start + i * nextPin, 0, pinWidth, pinHight, PinAlignment.Top, $"P{pinsOnSide * 3 + i}");
        }

        return node;
    }

    internal static INode CreateVia(double x, double y, double width, double height, string? label, double pinSize = 10)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = label }
        };

        node.AddPin(0, 0, pinSize, pinSize, PinAlignment.Right, "V1");

        //node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        //node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        //node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreatePin(double x, double y, double width, double height, string? label = "", double pinSize = 10)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new RoundedRectangleViewModel { Label = label }
        };

        node.AddPin(25.4 / 2  , 25.4 / 2 , pinSize, pinSize, PinAlignment.Right, label);

        //node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        //node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        //node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreateEllipse(double x, double y, double width, double height, string? label, double pinSize = 10)
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new EllipseViewModel { Label = label }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreateSignal(double x, double y, double width = 180, double height = 30, string? label = null, bool? state = false, double pinSize = 10, string name = "SIGNAL")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new SignalViewModel { Label = label, State = state }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "IN");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "OUT");

        return node;
    }

    internal static INode CreateAndGate(double x, double y, double width = 60, double height = 60, double pinSize = 10, string name = "AND")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new AndGateViewModel { Label = "&" }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static INode CreateOrGate(double x, double y, double width = 60, double height = 60, int count = 1, double pinSize = 10, string name = "OR")
    {
        var node = new NodeViewModel
        {
            Name = name,
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Pins = new ObservableCollection<IPin>(),
            Content = new OrGateViewModel { Label = "≥", Count = count }
        };

        node.AddPin(0, height / 2, pinSize, pinSize, PinAlignment.Left, "L");
        node.AddPin(width, height / 2, pinSize, pinSize, PinAlignment.Right, "R");
        node.AddPin(width / 2, 0, pinSize, pinSize, PinAlignment.Top, "T");
        node.AddPin(width / 2, height, pinSize, pinSize, PinAlignment.Bottom, "B");

        return node;
    }

    internal static ICommonConnector CreateConnector(IPin? start, IPin? end, double offset)
    {
        return new OffsetConnectorViewModel
        {
            Start = start,
            End = end,
            Offset = offset
        };
    }

    internal static ICommonConnector CreateBezierConnector(IPin? start, IPin? end)
    {
        return new OffsetConnectorViewModel
        {
            Start = start,
            End = end,

        };
    }

    public IDrawingNode CreateDrawing(string? name = null)
    {
        var drawing = new DrawingNodeViewModel
        {
            Name = name,
            X = 0,
            Y = 0,
            Width = 900,
            Height = 600,
            Nodes = new ObservableCollection<INode>(),
            Connectors = new ObservableCollection<ICommonConnector>(),
            EnableMultiplePinConnections = false,
            EnableSnap = true,
            SnapX = 15.0,
            SnapY = 15.0,
            EnableGrid = true,
            GridCellWidth = 15.0,
            GridCellHeight = 15.0,
        };

        return drawing;
    }

    public IList<INodeTemplate> CreateTemplates()
    {
        return new ObservableCollection<INodeTemplate>
        {
            new NodeTemplateViewModel
            {
                Title = "Rectangle",
                Template = CreateRectangle(0, 0, 60, 60, "rect"),
                Preview = CreateRectangle(0, 0, 60, 60, "rect")
            },
            new NodeTemplateViewModel
            {
                Title = "Ellipse",
                Template = CreateEllipse(0, 0, 60, 60, "ellipse"),
                Preview = CreateEllipse(0, 0, 60, 60, "ellipse")
            },
            new NodeTemplateViewModel
            {
                Title = "Signal",
                Template = CreateSignal(0, 0, label: "signal", state: false),
                Preview = CreateSignal(0, 0, label: "signal", state: false)
            },
            new NodeTemplateViewModel
            {
                Title = "AND Gate",
                Template = CreateAndGate(0, 0, 60, 60),
                Preview = CreateAndGate(0, 0, 60, 60)
            },
            new NodeTemplateViewModel
            {
                Title = "OR Gate",
                Template = CreateOrGate(0, 0, 60, 60),
                Preview = CreateOrGate(0, 0, 60, 60)
            },
            new NodeTemplateViewModel
            {
                Title = "Resistor",
                Template = CreateResistor(0, 0),
                Preview = CreateResistor(0, 0)
            },
            new NodeTemplateViewModel
            {
                Title = "Capacitor",
                Template = CreateCapacitor(0, 0),
                Preview = CreateCapacitor(0, 0)
            },
            new NodeTemplateViewModel
            {
                Title = "LED",
                Template = CreateLED(0, 0),
                Preview = CreateLED(0, 0)
            },
            new NodeTemplateViewModel
            {
                Title = "Transistor",
                Template = CreateTransistor(0, 0),
                Preview = CreateTransistor(0, 0)
            },
            new NodeTemplateViewModel
            {
                Title = "Microcontroller",
                Template = CreateMicrocontroller(0, 0),
                Preview = CreateMicrocontroller(0, 0)
            }
        };
    }

    // PCB Electronic Components Factory Methods
    internal static INode CreateResistor(double x, double y, string? value = "10kΩ", string? tolerance = "5%")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 120,
            Height = 40,
            Pins = new ObservableCollection<IPin>(),
            Content = new ResistorViewModel { Value = value, Tolerance = tolerance }
        };

        node.AddPin(0, 20, 8, 8, PinAlignment.Left, "1");
        node.AddPin(120, 20, 8, 8, PinAlignment.Right, "2");

        return node;
    }

    internal static INode CreateCapacitor(double x, double y, string? value = "100µF", string? voltage = "25V")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 100,
            Height = 60,
            Pins = new ObservableCollection<IPin>(),
            Content = new CapacitorViewModel { Value = value, Voltage = voltage }
        };

        node.AddPin(0, 30, 8, 8, PinAlignment.Left, "A");
        node.AddPin(100, 30, 8, 8, PinAlignment.Right, "K");

        return node;
    }

    internal static INode CreateLED(double x, double y, string? color = "Red", string? type = "Standard")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 80,
            Height = 80,
            Pins = new ObservableCollection<IPin>(),
            Content = new LEDViewModel { Color = color }
        };

        node.AddPin(0, 40, 8, 8, PinAlignment.Left, "A");
        node.AddPin(80, 40, 8, 8, PinAlignment.Right, "K");

        return node;
    }

    internal static INode CreateTransistor(double x, double y, string? partNumber = "2N2222", string? type = "NPN")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 100,
            Height = 80,
            Pins = new ObservableCollection<IPin>(),
            Content = new TransistorViewModel { PartNumber = partNumber }
        };

        node.AddPin(0, 40, 6, 6, PinAlignment.Left, "B");
        node.AddPin(100, 15, 6, 6, PinAlignment.Right, "C");
        node.AddPin(100, 65, 6, 6, PinAlignment.Right, "E");

        return node;
    }

    internal static INode CreateMicrocontroller(double x, double y, string? partNumber = "Arduino Nano", string? type = "Arduino")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 160,
            Height = 120,
            Pins = new ObservableCollection<IPin>(),
            Content = new MicrocontrollerViewModel { PartNumber = partNumber }
        };

        // Add digital pins (left side)
        for (int i = 0; i < 13; i++)
        {
            node.AddPin(0, 15 + i * 7, 6, 6, PinAlignment.Left, $"D{i}");
        }

        // Add analog pins (right side)
        for (int i = 0; i < 8; i++)
        {
            node.AddPin(160, 15 + i * 7, 6, 6, PinAlignment.Right, $"A{i}");
        }

        // Add power pins
        node.AddPin(0, 100, 6, 6, PinAlignment.Left, "VCC");
        node.AddPin(160, 100, 6, 6, PinAlignment.Right, "GND");

        return node;
    }

    internal static INode CreateInductor(double x, double y, string? value = "100µH", string? current = "1A")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 100,
            Height = 40,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = value }
        };

        node.AddPin(0, 20, 8, 8, PinAlignment.Left, "1");
        node.AddPin(100, 20, 8, 8, PinAlignment.Right, "2");

        return node;
    }

    internal static INode CreateDiode(double x, double y, string? partNumber = "1N4007", string? type = "Standard")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 80,
            Height = 40,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = partNumber }
        };

        node.AddPin(0, 20, 8, 8, PinAlignment.Left, "A");
        node.AddPin(80, 20, 8, 8, PinAlignment.Right, "K");

        return node;
    }

    internal static INode CreateConnector(double x, double y, int pins = 2, string? type = "Header")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 40,
            Height = pins * 20,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = $"{pins}P {type}" }
        };

        for (int i = 0; i < pins; i++)
        {
            node.AddPin(0, 10 + i * 20, 8, 8, PinAlignment.Left, $"{i + 1}");
        }

        return node;
    }

    internal static INode CreateOpAmp(double x, double y, string? partNumber = "LM358", string? type = "Dual")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 80,
            Height = 60,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = partNumber }
        };

        node.AddPin(0, 15, 6, 6, PinAlignment.Left, "V+");
        node.AddPin(0, 45, 6, 6, PinAlignment.Left, "V-");
        node.AddPin(80, 30, 6, 6, PinAlignment.Right, "OUT");
        node.AddPin(40, 0, 6, 6, PinAlignment.Top, "VCC");
        node.AddPin(40, 60, 6, 6, PinAlignment.Bottom, "GND");

        return node;
    }

    internal static INode CreateCrystal(double x, double y, string? frequency = "16MHz", string? type = "HC-49")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 60,
            Height = 30,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = frequency }
        };

        node.AddPin(0, 15, 6, 6, PinAlignment.Left, "1");
        node.AddPin(60, 15, 6, 6, PinAlignment.Right, "2");

        return node;
    }

    internal static INode CreateBattery(double x, double y, string? voltage = "3.7V", string? type = "Li-Ion")
    {
        var node = new NodeViewModel
        {
            X = x,
            Y = y,
            Width = 80,
            Height = 50,
            Pins = new ObservableCollection<IPin>(),
            Content = new RectangleViewModel { Label = $"{voltage} {type}" }
        };

        node.AddPin(0, 25, 8, 8, PinAlignment.Left, "+");
        node.AddPin(80, 25, 8, 8, PinAlignment.Right, "-");

        return node;
    }
}
