using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NodeEditorDemo.Models;
using NodeEditorDemo.Services;
using NodeEditorDemo.ViewModels;
using Xunit;

namespace NodeEditor.UnitTests;

public class ArduinoUnoAndGerberExportTests
{
    [Fact]
    public void Test_ArduinoUno_NodeCreation_And_Pins()
    {
        var node = NodeFactory.CreateArduinoUno(100, 150);

        Assert.NotNull(node);
        Assert.Equal(100, node.X);
        Assert.Equal(150, node.Y);
        Assert.Equal(280, node.Width);
        Assert.Equal(215, node.Height);

        Assert.IsType<ComponentViewModel>(node.Content);
        var vm = (ComponentViewModel)node.Content!;
        Assert.Equal("Arduino Uno", vm.Definition.Name);
        Assert.Equal("ATmega328P", vm.Value);

        Assert.NotNull(node.Pins);
        Assert.Equal(32, node.Pins.Count);

        // Verify key digital header pins
        var pinScl = node.Pins.FirstOrDefault(p => p.Name == "SCL");
        var pinSda = node.Pins.FirstOrDefault(p => p.Name == "SDA");
        var pin13 = node.Pins.FirstOrDefault(p => p.Name == "13");
        var pin7 = node.Pins.FirstOrDefault(p => p.Name == "7");
        var pinRx = node.Pins.FirstOrDefault(p => p.Name == "RX0" || p.Name == "RX<0");

        Assert.NotNull(pinScl);
        Assert.NotNull(pinSda);
        Assert.NotNull(pin13);
        Assert.NotNull(pin7);
        Assert.NotNull(pinRx);

        // Verify power and analog header pins
        var pin5v = node.Pins.FirstOrDefault(p => p.Name == "5V");
        var pin3v3 = node.Pins.FirstOrDefault(p => p.Name == "3.3V");
        var pinGnd = node.Pins.FirstOrDefault(p => p.Name == "GND1");
        var pinA0 = node.Pins.FirstOrDefault(p => p.Name == "A0");
        var pinA5 = node.Pins.FirstOrDefault(p => p.Name == "A5");

        Assert.NotNull(pin5v);
        Assert.NotNull(pin3v3);
        Assert.NotNull(pinGnd);
        Assert.NotNull(pinA0);
        Assert.NotNull(pinA5);
    }

    [Fact]
    public void Test_ThroughHole_Components_Loaded_From_Json()
    {
        var components = ComponentLoader.LoadAllComponents().ToList();
        Assert.NotNull(components);
        Assert.True(components.Count >= 7, $"Expected at least 7 components, got {components.Count}");

        var names = components.Select(c => c.Name).ToList();
        Assert.Contains("Resistor", names);
        Assert.Contains("LED", names);
        Assert.Contains("Capacitor", names);
        Assert.Contains("Ceramic Capacitor", names);
        Assert.Contains("Inductor", names);
        Assert.Contains("Diode", names);
        Assert.Contains("Arduino Uno", names);
    }

    [Theory]
    [InlineData("Resistor", 2)]
    [InlineData("LED", 2)]
    [InlineData("Capacitor", 2)]
    [InlineData("Ceramic Capacitor", 2)]
    [InlineData("Inductor", 2)]
    [InlineData("Diode", 2)]
    public void Test_ThroughHole_Components_Pins_And_Pitch(string componentName, int expectedPins)
    {
        var node = NodeFactory.CreateComponent(componentName, 50.8, 50.8);
        Assert.NotNull(node);
        Assert.Equal(expectedPins, node.Pins?.Count);

        var vm = Assert.IsType<ComponentViewModel>(node.Content);
        Assert.Equal(componentName, vm.Definition.Name);

        // Through-hole pins should span integer multiples of breadboard pitch (25.4 canvas units = 2.54mm)
        var pin1 = node.Pins![0];
        var pin2 = node.Pins![1];
        var dx = Math.Abs(pin2.X - pin1.X);
        var dy = Math.Abs(pin2.Y - pin1.Y);
        var distance = Math.Sqrt(dx * dx + dy * dy);

        // Check that pin spacing is a multiple of 25.4 (e.g. 50.8 for 2 holes, 101.6 for 4 holes)
        var remainder = Math.Round(distance % NodeFactory.BreadboardPitch, 2);
        Assert.True(remainder == 0.0 || Math.Abs(remainder - NodeFactory.BreadboardPitch) < 0.01,
            $"Pin spacing {distance} for {componentName} must be multiple of breadboard pitch {NodeFactory.BreadboardPitch}");

        // Assert that EVERY pin's X and Y coordinates are exact multiples of breadboard pitch (25.4 canvas units)
        foreach (var pin in node.Pins!)
        {
            var remX = Math.Round(pin.X % NodeFactory.BreadboardPitch, 2);
            var remY = Math.Round(pin.Y % NodeFactory.BreadboardPitch, 2);
            Assert.True(remX == 0.0 || Math.Abs(remX - NodeFactory.BreadboardPitch) < 0.01,
                $"Pin {pin.Name} X coordinate {pin.X} for {componentName} must be a multiple of breadboard pitch {NodeFactory.BreadboardPitch}");
            Assert.True(remY == 0.0 || Math.Abs(remY - NodeFactory.BreadboardPitch) < 0.01,
                $"Pin {pin.Name} Y coordinate {pin.Y} for {componentName} must be a multiple of breadboard pitch {NodeFactory.BreadboardPitch}");
        }
    }

    [Fact]
    public void Test_GerberExport_TopCopper_MatchesKiCad()
    {
        var gbr = GerberExportService.GenerateTopCopper(null, "arduinouno");

        Assert.Contains("%TF.GenerationSoftware,KiCad,Pcbnew,10.0.6*%", gbr);
        Assert.Contains("%TF.ProjectId,arduinouno,61726475-696e-46f7-956e-6f2e6b696361,rev?*%", gbr);
        Assert.Contains("%TF.FileFunction,Copper,L1,Top*%", gbr);
        Assert.Contains("%FSLAX46Y46*%", gbr);
        Assert.Contains("%ADD10R,1.700000X1.700000*%", gbr);
        Assert.Contains("%ADD11O,1.700000X1.700000*%", gbr);

        // Check exact coordinates from KiCad reference
        Assert.Contains("X127940000Y-97460000D03*", gbr); // J1 Pin 1
        Assert.Contains("X118796000Y-49200000D03*", gbr); // J2 Pin 1
        Assert.Contains("X150800000Y-97460000D03*", gbr); // J3 Pin 1
        Assert.Contains("X145720000Y-49200000D03*", gbr); // J4 Pin 1
    }

    [Fact]
    public void Test_GerberExport_EdgeCuts_Outline()
    {
        var gbr = GerberExportService.GenerateEdgeCuts(null, "arduinouno");

        Assert.Contains("%TF.FileFunction,Profile,NP*%", gbr);
        Assert.Contains("%FSLAX46Y46*%", gbr);
        Assert.Contains("%ADD10C,0.150000*%", gbr);
        Assert.Contains("D10*", gbr);
        Assert.Contains("M02*", gbr);
    }

    [Fact]
    public void Test_GerberExport_JobFile()
    {
        var job = GerberExportService.GenerateJobFile(null, "arduinouno");

        Assert.Contains("\"Vendor\": \"KiCad\"", job);
        Assert.Contains("\"Version\": \"10.0.6\"", job);
        Assert.Contains("\"X\": 68.73", job);
        Assert.Contains("\"Y\": 53.49", job);
        Assert.Contains("\"BoardThickness\": 1.6", job);
        Assert.Contains("arduinouno-F_Cu.gbr", job);
        Assert.Contains("arduinouno-B_Cu.gbr", job);
        Assert.Contains("arduinouno-Edge_Cuts.gbr", job);
    }

    [Fact]
    public void Test_GerberExport_DrillFile()
    {
        var drl = GerberExportService.GenerateDrillFile(null, "arduinouno");

        Assert.Contains("METRIC,TZ", drl);
        Assert.Contains("T1C0.900", drl); // Header pins
        Assert.Contains("T2C3.200", drl); // Mounting holes
        Assert.Contains("M30", drl);
    }

    [Fact]
    public async Task Test_GerberExport_ZipPackage()
    {
        using var ms = new MemoryStream();
        await GerberExportService.ExportGerberPackageZipAsync(null, ms, "arduinouno");

        ms.Position = 0;
        using var zip = new ZipArchive(ms, ZipArchiveMode.Read);

        var names = zip.Entries.Select(e => e.FullName).ToList();
        Assert.Contains("arduinouno-F_Cu.gbr", names);
        Assert.Contains("arduinouno-B_Cu.gbr", names);
        Assert.Contains("arduinouno-F_Silkscreen.gbr", names);
        Assert.Contains("arduinouno-B_Silkscreen.gbr", names);
        Assert.Contains("arduinouno-Edge_Cuts.gbr", names);
        Assert.Contains("arduinouno-job.gbrjob", names);
        Assert.Contains("arduinouno.drl", names);
        Assert.Contains("README.txt", names);
        Assert.Equal(8, zip.Entries.Count);
    }
}
