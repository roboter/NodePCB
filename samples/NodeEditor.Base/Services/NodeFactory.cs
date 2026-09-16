using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NodeEditor.Model;
using NodeEditor.Mvvm;
using NodeEditorDemo.Models;
using NodeEditorDemo.ViewModels;

namespace NodeEditorDemo.Services;

public class NodeFactory : INodeFactory
{
    public const double BreadboardPitch = 25.4; // 2.54mm breadboard pitch (10x scale)

    public static INode CreateComponent(ComponentDefinition def, double x, double y)
    {
        var vm = new ComponentViewModel(def);

        var node = new NodeViewModel
        {
            Name = def.Name,
            X = x,
            Y = y,
            Width = def.Width,
            Height = def.Height,
            Pins = new ObservableCollection<IPin>(),
            Content = vm
        };

        foreach (var pin in def.Pins)
        {
            double pWidth = Math.Max(8.0, pin.PadWidth * 4.0);
            double pHeight = Math.Max(8.0, pin.PadHeight * 4.0);
            node.AddPin(pin.X, pin.Y, pWidth, pHeight, pin.GetPinAlignment(), pin.Name);
        }

        return node;
    }

    public static INode CreateComponent(string name, double x, double y)
    {
        var def = ComponentLoader.GetComponent(name);
        if (def == null)
        {
            throw new FileNotFoundException($"Component definition '{name}' not found in components directory.");
        }
        return CreateComponent(def, x, y);
    }

    // Convenience factory methods for standard through-hole components
    public static INode CreateResistor(double x, double y, string? value = null)
    {
        var node = CreateComponent("Resistor", x, y);
        if (value != null && node.Content is ComponentViewModel vm)
        {
            vm.Value = value;
        }
        return node;
    }

    public static INode CreateLED(double x, double y, string? color = null)
    {
        var node = CreateComponent("LED", x, y);
        if (color != null && node.Content is ComponentViewModel vm)
        {
            vm.Value = color;
        }
        return node;
    }

    public static INode CreateCapacitor(double x, double y, string? value = null)
    {
        var node = CreateComponent("Capacitor", x, y);
        if (value != null && node.Content is ComponentViewModel vm)
        {
            vm.Value = value;
        }
        return node;
    }

    public static INode CreateCeramicCapacitor(double x, double y, string? value = null)
    {
        var node = CreateComponent("Ceramic Capacitor", x, y);
        if (value != null && node.Content is ComponentViewModel vm)
        {
            vm.Value = value;
        }
        return node;
    }

    public static INode CreateInductor(double x, double y, string? value = null)
    {
        var node = CreateComponent("Inductor", x, y);
        if (value != null && node.Content is ComponentViewModel vm)
        {
            vm.Value = value;
        }
        return node;
    }

    public static INode CreateDiode(double x, double y, string? partNumber = null)
    {
        var node = CreateComponent("Diode", x, y);
        if (partNumber != null && node.Content is ComponentViewModel vm)
        {
            vm.Value = partNumber;
        }
        return node;
    }

    public static INode CreateArduinoUno(double x, double y)
    {
        return CreateComponent("Arduino Uno", x, y);
    }

    public static ICommonConnector CreateConnector(IPin? start, IPin? end, double offset = 50)
    {
        return new BezierConnectorViewModel
        {
            Start = start,
            End = end,
            Offset = offset
        };
    }

    public static ICommonConnector CreateBezierConnector(IPin? start, IPin? end)
    {
        return new BezierConnectorViewModel
        {
            Start = start,
            End = end
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
            SnapX = BreadboardPitch,
            SnapY = BreadboardPitch,
            EnableGrid = true,
            GridCellWidth = BreadboardPitch,
            GridCellHeight = BreadboardPitch,
        };

        return drawing;
    }

    public IList<INodeTemplate> CreateTemplates()
    {
        var templates = new ObservableCollection<INodeTemplate>();

        // Load all generic through-hole components from JSON
        var components = ComponentLoader.LoadAllComponents();
        foreach (var def in components)
        {
            templates.Add(new NodeTemplateViewModel
            {
                Title = def.Name,
                Template = CreateComponent(def, 0, 0),
                Preview = CreateComponent(def, 0, 0)
            });
        }

        return templates;
    }
}
