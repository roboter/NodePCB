using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeEditor.Model;

namespace NodeEditorDemo.Services;

public static class GerberExportService
{
    private const double BaseOriginX = 100.0;
    private const double BaseOriginY = -100.0;
    private const double BoardWidth = 68.58;
    private const double BoardHeight = 53.34;

    // Header Pin Definitions: (HeaderName, PinNumber, PinName, NetName, X_mm, Y_mm)
    private static readonly (string Header, int Pin, string Name, string Net, double X, double Y)[] HeaderPins = new[]
    {
        // J1 - Power Header (8 pins)
        ("J1", 1, "Pin_1", "unconnected-(J1-Pin_1-Pad1)", 127.940, -97.460),
        ("J1", 2, "Pin_2", "/IOREF", 130.480, -97.460),
        ("J1", 3, "Pin_3", "/~{RESET}", 133.020, -97.460),
        ("J1", 4, "Pin_4", "+3V3", 135.560, -97.460),
        ("J1", 5, "Pin_5", "+5V", 138.100, -97.460),
        ("J1", 6, "Pin_6", "GND", 140.640, -97.460),
        ("J1", 7, "Pin_7", "GND", 143.180, -97.460),
        ("J1", 8, "Pin_8", "VCC", 145.720, -97.460),

        // J3 - Analog In Header (6 pins)
        ("J3", 1, "Pin_1", "/A0", 150.800, -97.460),
        ("J3", 2, "Pin_2", "/A1", 153.340, -97.460),
        ("J3", 3, "Pin_3", "/A2", 155.880, -97.460),
        ("J3", 4, "Pin_4", "/A3", 158.420, -97.460),
        ("J3", 5, "Pin_5", "/SDA{slash}A4", 160.960, -97.460),
        ("J3", 6, "Pin_6", "/SCL{slash}A5", 163.500, -97.460),

        // J2 - Digital High Header (10 pins)
        ("J2", 1, "Pin_1", "/SCL{slash}A5", 118.796, -49.200),
        ("J2", 2, "Pin_2", "/SDA{slash}A4", 121.336, -49.200),
        ("J2", 3, "Pin_3", "/AREF", 123.876, -49.200),
        ("J2", 4, "Pin_4", "GND", 126.416, -49.200),
        ("J2", 5, "Pin_5", "/13", 128.956, -49.200),
        ("J2", 6, "Pin_6", "/12", 131.496, -49.200),
        ("J2", 7, "Pin_7", "/\\u002A11", 134.036, -49.200),
        ("J2", 8, "Pin_8", "/\\u002A10", 136.576, -49.200),
        ("J2", 9, "Pin_9", "/\\u002A9", 139.116, -49.200),
        ("J2", 10, "Pin_10", "/8", 141.656, -49.200),

        // J4 - Digital Low Header (8 pins)
        ("J4", 1, "Pin_1", "/7", 145.720, -49.200),
        ("J4", 2, "Pin_2", "/\\u002A6", 148.260, -49.200),
        ("J4", 3, "Pin_3", "/\\u002A5", 150.800, -49.200),
        ("J4", 4, "Pin_4", "/4", 153.340, -49.200),
        ("J4", 5, "Pin_5", "/\\u002A3", 155.880, -49.200),
        ("J4", 6, "Pin_6", "/2", 158.420, -49.200),
        ("J4", 7, "Pin_7", "/TX{slash}1", 160.960, -49.200),
        ("J4", 8, "Pin_8", "/RX{slash}0", 163.500, -49.200),
    };

    // Standard Arduino Uno 4 mounting holes
    private static readonly (double X, double Y, double Diameter)[] MountingHoles = new[]
    {
        (113.970, -97.460, 3.200),  // Bottom-Left (near DC jack)
        (115.240, -49.200, 3.200),  // Top-Left (near USB)
        (166.040, -64.440, 3.200),  // Bottom-Right (near analog pins)
        (166.040, -92.380, 3.200)   // Top-Right (near digital pins)
    };

    public static string FormatCoord(double mm)
    {
        var val = (long)Math.Round(mm * 1_000_000);
        return val.ToString(CultureInfo.InvariantCulture);
    }

    public static string GenerateTopCopper(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var sb = new StringBuilder();
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        sb.AppendLine($"%TF.GenerationSoftware,KiCad,Pcbnew,10.0.6*%");
        sb.AppendLine($"%TF.CreationDate,{now}*%");
        sb.AppendLine($"%TF.ProjectId,{projectName},61726475-696e-46f7-956e-6f2e6b696361,rev?*%");
        sb.AppendLine($"%TF.SameCoordinates,Original*%");
        sb.AppendLine($"%TF.FileFunction,Copper,L1,Top*%");
        sb.AppendLine($"%TF.FilePolarity,Positive*%");
        sb.AppendLine($"%FSLAX46Y46*%");
        sb.AppendLine($"G04 Gerber Fmt 4.6, Leading zero omitted, Abs format (unit mm)*");
        sb.AppendLine($"G04 Created by KiCad (PCBNEW 10.0.6) date {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        sb.AppendLine($"%MOMM*%");
        sb.AppendLine($"%LPD*%");
        sb.AppendLine($"G01*");
        sb.AppendLine($"G04 APERTURE LIST*");
        sb.AppendLine($"%TA.AperFunction,ComponentPad*%");
        sb.AppendLine($"%ADD10R,1.700000X1.700000*%");
        sb.AppendLine($"%TD*%");
        sb.AppendLine($"%TA.AperFunction,ComponentPad*%");
        sb.AppendLine($"%ADD11O,1.700000X1.700000*%");
        sb.AppendLine($"%TD*%");
        sb.AppendLine($"%TA.AperFunction,Conductor*%");
        sb.AppendLine($"%ADD12C,0.400000*%");
        sb.AppendLine($"%TD*%");
        sb.AppendLine($"G04 APERTURE END LIST*");

        // Flash Header Pads (Pin 1 square = D10, Pins 2..N round = D11)
        string currentHeader = "";
        foreach (var pin in HeaderPins)
        {
            if (pin.Header != currentHeader)
            {
                if (!string.IsNullOrEmpty(currentHeader))
                {
                    sb.AppendLine("%TD*%");
                }
                currentHeader = pin.Header;
                sb.AppendLine(pin.Pin == 1 ? "D10*" : "D11*");
            }
            else if (pin.Pin == 2)
            {
                sb.AppendLine("D11*");
            }

            sb.AppendLine($"%TO.P,{pin.Header},{pin.Pin},{pin.Name}*%");
            sb.AppendLine($"%TO.N,{pin.Net}*%");
            sb.AppendLine($"X{FormatCoord(pin.X)}Y{FormatCoord(pin.Y)}D03*");
        }
        sb.AppendLine("%TD*%");

        // Render Canvas Routed Connectors as Copper Traces (D12)
        if (drawing?.Connectors != null && drawing.Connectors.Count > 0)
        {
            sb.AppendLine("G04 === Copper Traces from Canvas Connectors ===*");
            sb.AppendLine("D12*");

            foreach (var conn in drawing.Connectors)
            {
                var points = GetConnectorGerberPoints(conn, drawing);
                if (points.Count > 1)
                {
                    sb.AppendLine($"X{FormatCoord(points[0].X)}Y{FormatCoord(points[0].Y)}D02*");
                    for (int i = 1; i < points.Count; i++)
                    {
                        sb.AppendLine($"X{FormatCoord(points[i].X)}Y{FormatCoord(points[i].Y)}D01*");
                    }
                }
            }
        }

        sb.AppendLine("M02*");
        return sb.ToString();
    }

    public static string GenerateBottomCopper(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var sb = new StringBuilder();
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        sb.AppendLine($"%TF.GenerationSoftware,KiCad,Pcbnew,10.0.6*%");
        sb.AppendLine($"%TF.CreationDate,{now}*%");
        sb.AppendLine($"%TF.ProjectId,{projectName},61726475-696e-46f7-956e-6f2e6b696361,rev?*%");
        sb.AppendLine($"%TF.SameCoordinates,Original*%");
        sb.AppendLine($"%TF.FileFunction,Copper,L2,Bot*%");
        sb.AppendLine($"%TF.FilePolarity,Positive*%");
        sb.AppendLine($"%FSLAX46Y46*%");
        sb.AppendLine($"G04 Gerber Fmt 4.6, Leading zero omitted, Abs format (unit mm)*");
        sb.AppendLine($"G04 Created by KiCad (PCBNEW 10.0.6) date {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        sb.AppendLine($"%MOMM*%");
        sb.AppendLine($"%LPD*%");
        sb.AppendLine($"G01*");
        sb.AppendLine($"G04 APERTURE LIST*");
        sb.AppendLine($"%TA.AperFunction,ComponentPad*%");
        sb.AppendLine($"%ADD10R,1.700000X1.700000*%");
        sb.AppendLine($"%TD*%");
        sb.AppendLine($"%TA.AperFunction,ComponentPad*%");
        sb.AppendLine($"%ADD11O,1.700000X1.700000*%");
        sb.AppendLine($"%TD*%");
        sb.AppendLine($"G04 APERTURE END LIST*");

        string currentHeader = "";
        foreach (var pin in HeaderPins)
        {
            if (pin.Header != currentHeader)
            {
                if (!string.IsNullOrEmpty(currentHeader))
                {
                    sb.AppendLine("%TD*%");
                }
                currentHeader = pin.Header;
                sb.AppendLine(pin.Pin == 1 ? "D10*" : "D11*");
            }
            else if (pin.Pin == 2)
            {
                sb.AppendLine("D11*");
            }

            sb.AppendLine($"%TO.P,{pin.Header},{pin.Pin},{pin.Name}*%");
            sb.AppendLine($"%TO.N,{pin.Net}*%");
            sb.AppendLine($"X{FormatCoord(pin.X)}Y{FormatCoord(pin.Y)}D03*");
        }
        sb.AppendLine("%TD*%");
        sb.AppendLine("M02*");
        return sb.ToString();
    }

    public static string GenerateTopSilkscreen(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var sb = new StringBuilder();
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        sb.AppendLine($"%TF.GenerationSoftware,KiCad,Pcbnew,10.0.6*%");
        sb.AppendLine($"%TF.CreationDate,{now}*%");
        sb.AppendLine($"%TF.ProjectId,{projectName},61726475-696e-46f7-956e-6f2e6b696361,rev?*%");
        sb.AppendLine($"%TF.SameCoordinates,Original*%");
        sb.AppendLine($"%TF.FileFunction,Legend,Top*%");
        sb.AppendLine($"%TF.FilePolarity,Positive*%");
        sb.AppendLine($"%FSLAX46Y46*%");
        sb.AppendLine($"G04 Gerber Fmt 4.6, Leading zero omitted, Abs format (unit mm)*");
        sb.AppendLine($"G04 Created by KiCad (PCBNEW 10.0.6) date {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        sb.AppendLine($"%MOMM*%");
        sb.AppendLine($"%LPD*%");
        sb.AppendLine($"G01*");
        sb.AppendLine($"G04 APERTURE LIST*");
        sb.AppendLine($"%ADD10C,0.120000*%");
        sb.AppendLine($"G04 APERTURE END LIST*");

        // Header J1 outline
        sb.AppendLine("%TO.C,J1*%");
        sb.AppendLine("D10*");
        sb.AppendLine($"X{FormatCoord(126.61)}Y{FormatCoord(-96.13)}D02*");
        sb.AppendLine($"X{FormatCoord(127.94)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(126.61)}Y{FormatCoord(-97.46)}D02*");
        sb.AppendLine($"X{FormatCoord(126.61)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(129.21)}Y{FormatCoord(-96.13)}D02*");
        sb.AppendLine($"X{FormatCoord(147.05)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(129.21)}Y{FormatCoord(-98.79)}D02*");
        sb.AppendLine($"X{FormatCoord(129.21)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(129.21)}Y{FormatCoord(-98.79)}D02*");
        sb.AppendLine($"X{FormatCoord(147.05)}Y{FormatCoord(-98.79)}D01*");
        sb.AppendLine($"X{FormatCoord(147.05)}Y{FormatCoord(-98.79)}D02*");
        sb.AppendLine($"X{FormatCoord(147.05)}Y{FormatCoord(-96.13)}D01*");

        // Header J3 outline
        sb.AppendLine("%TO.C,J3*%");
        sb.AppendLine($"X{FormatCoord(149.47)}Y{FormatCoord(-96.13)}D02*");
        sb.AppendLine($"X{FormatCoord(150.80)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(149.47)}Y{FormatCoord(-97.46)}D02*");
        sb.AppendLine($"X{FormatCoord(149.47)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(152.07)}Y{FormatCoord(-96.13)}D02*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(152.07)}Y{FormatCoord(-98.79)}D02*");
        sb.AppendLine($"X{FormatCoord(152.07)}Y{FormatCoord(-96.13)}D01*");
        sb.AppendLine($"X{FormatCoord(152.07)}Y{FormatCoord(-98.79)}D02*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-98.79)}D01*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-98.79)}D02*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-96.13)}D01*");

        // Header J2 outline
        sb.AppendLine("%TO.C,J2*%");
        sb.AppendLine($"X{FormatCoord(117.466)}Y{FormatCoord(-47.87)}D02*");
        sb.AppendLine($"X{FormatCoord(118.796)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(117.466)}Y{FormatCoord(-49.20)}D02*");
        sb.AppendLine($"X{FormatCoord(117.466)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(120.066)}Y{FormatCoord(-47.87)}D02*");
        sb.AppendLine($"X{FormatCoord(142.986)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(120.066)}Y{FormatCoord(-50.53)}D02*");
        sb.AppendLine($"X{FormatCoord(120.066)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(120.066)}Y{FormatCoord(-50.53)}D02*");
        sb.AppendLine($"X{FormatCoord(142.986)}Y{FormatCoord(-50.53)}D01*");
        sb.AppendLine($"X{FormatCoord(142.986)}Y{FormatCoord(-50.53)}D02*");
        sb.AppendLine($"X{FormatCoord(142.986)}Y{FormatCoord(-47.87)}D01*");

        // Header J4 outline
        sb.AppendLine("%TO.C,J4*%");
        sb.AppendLine($"X{FormatCoord(144.39)}Y{FormatCoord(-47.87)}D02*");
        sb.AppendLine($"X{FormatCoord(145.72)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(144.39)}Y{FormatCoord(-49.20)}D02*");
        sb.AppendLine($"X{FormatCoord(144.39)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(146.99)}Y{FormatCoord(-47.87)}D02*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(146.99)}Y{FormatCoord(-50.53)}D02*");
        sb.AppendLine($"X{FormatCoord(146.99)}Y{FormatCoord(-47.87)}D01*");
        sb.AppendLine($"X{FormatCoord(146.99)}Y{FormatCoord(-50.53)}D02*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-50.53)}D01*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-50.53)}D02*");
        sb.AppendLine($"X{FormatCoord(164.83)}Y{FormatCoord(-47.87)}D01*");

        sb.AppendLine("%TD*%");
        sb.AppendLine("M02*");
        return sb.ToString();
    }

    public static string GenerateBottomSilkscreen(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var sb = new StringBuilder();
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        sb.AppendLine($"%TF.GenerationSoftware,KiCad,Pcbnew,10.0.6*%");
        sb.AppendLine($"%TF.CreationDate,{now}*%");
        sb.AppendLine($"%TF.ProjectId,{projectName},61726475-696e-46f7-956e-6f2e6b696361,rev?*%");
        sb.AppendLine($"%TF.SameCoordinates,Original*%");
        sb.AppendLine($"%TF.FileFunction,Legend,Bot*%");
        sb.AppendLine($"%TF.FilePolarity,Positive*%");
        sb.AppendLine($"%FSLAX46Y46*%");
        sb.AppendLine($"G04 Gerber Fmt 4.6, Leading zero omitted, Abs format (unit mm)*");
        sb.AppendLine($"G04 Created by KiCad (PCBNEW 10.0.6) date {DateTime.Now:yyyy-MM-dd HH:mm:ss}*");
        sb.AppendLine($"%MOMM*%");
        sb.AppendLine($"%LPD*%");
        sb.AppendLine($"G01*");
        sb.AppendLine($"G04 APERTURE LIST*");
        sb.AppendLine($"G04 APERTURE END LIST*");
        sb.AppendLine("M02*");
        return sb.ToString();
    }

    public static string GenerateEdgeCuts(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var sb = new StringBuilder();
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        sb.AppendLine($"%TF.GenerationSoftware,KiCad,Pcbnew,10.0.6*%");
        sb.AppendLine($"%TF.CreationDate,{now}*%");
        sb.AppendLine($"%TF.ProjectId,{projectName},61726475-696e-46f7-956e-6f2e6b696361,rev?*%");
        sb.AppendLine($"%TF.SameCoordinates,Original*%");
        sb.AppendLine($"%TF.FileFunction,Profile,NP*%");
        sb.AppendLine($"%TF.FilePolarity,Positive*%");
        sb.AppendLine($"%FSLAX46Y46*%");
        sb.AppendLine($"G04 Gerber Fmt 4.6, Leading zero omitted, Abs format (unit mm)*");
        sb.AppendLine($"G04 Created by KiCad (PCBNEW 10.0.6)*");
        sb.AppendLine($"%MOMM*%");
        sb.AppendLine($"%LPD*%");
        sb.AppendLine($"G01*");
        sb.AppendLine($"G04 APERTURE LIST*");
        sb.AppendLine($"%ADD10C,0.150000*%");
        sb.AppendLine($"G04 APERTURE END LIST*");
        sb.AppendLine("D10*");

        // Exact Arduino Uno mechanical outline points translated to BaseOrigin (100.0, -100.0)
        var outline = new (double X, double Y)[]
        {
            (0.762, 0.0),
            (63.500, 0.0),
            (66.040, 2.540),
            (66.040, 12.700),
            (68.580, 15.240),
            (68.580, 49.530),
            (66.040, 52.070),
            (66.040, 52.578),
            (65.278, 53.340),
            (0.762, 53.340),
            (0.000, 52.578),
            (0.000, 0.762),
            (0.762, 0.0)
        };

        // Output outline path
        double startX = BaseOriginX + outline[0].X;
        double startY = BaseOriginY + outline[0].Y;
        sb.AppendLine($"X{FormatCoord(startX)}Y{FormatCoord(startY)}D02*");

        for (int i = 1; i < outline.Length; i++)
        {
            double ptX = BaseOriginX + outline[i].X;
            double ptY = BaseOriginY + outline[i].Y;
            sb.AppendLine($"X{FormatCoord(ptX)}Y{FormatCoord(ptY)}D01*");
        }

        sb.AppendLine("M02*");
        return sb.ToString();
    }

    public static string GenerateJobFile(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture);

        return $$"""
{
  "Header": {
    "GenerationSoftware": {
      "Vendor": "KiCad",
      "Application": "Pcbnew",
      "Version": "10.0.6"
    },
    "CreationDate": "{{now}}"
  },
  "GeneralSpecs": {
    "ProjectId": {
      "Name": "{{projectName}}",
      "GUID": "61726475-696e-46f7-956e-6f2e6b696361",
      "Revision": "rev?"
    },
    "Size": {
      "X": 68.73,
      "Y": 53.49
    },
    "LayerNumber": 2,
    "BoardThickness": 1.6,
    "Finish": "None"
  },
  "DesignRules": [
    {
      "Layers": "Outer",
      "PadToPad": 0.0,
      "PadToTrack": 0.0,
      "TrackToTrack": 0.2
    }
  ],
  "FilesAttributes": [
    {
      "Path": "{{projectName}}-F_Cu.gbr",
      "FileFunction": "Copper,L1,Top",
      "FilePolarity": "Positive"
    },
    {
      "Path": "{{projectName}}-B_Cu.gbr",
      "FileFunction": "Copper,L2,Bot",
      "FilePolarity": "Positive"
    },
    {
      "Path": "{{projectName}}-F_Silkscreen.gbr",
      "FileFunction": "Legend,Top",
      "FilePolarity": "Positive"
    },
    {
      "Path": "{{projectName}}-B_Silkscreen.gbr",
      "FileFunction": "Legend,Bot",
      "FilePolarity": "Positive"
    },
    {
      "Path": "{{projectName}}-Edge_Cuts.gbr",
      "FileFunction": "Profile,NP",
      "FilePolarity": "Positive"
    }
  ],
  "MaterialStackup": [
    {
      "Type": "Legend",
      "Name": "Top Silk Screen"
    },
    {
      "Type": "SolderPaste",
      "Name": "Top Solder Paste"
    },
    {
      "Type": "SolderMask",
      "Color": "Green",
      "Thickness": 0.01,
      "Name": "Top Solder Mask"
    },
    {
      "Type": "Copper",
      "Thickness": 0.035,
      "Name": "F.Cu"
    },
    {
      "Type": "Dielectric",
      "Thickness": 1.51,
      "Material": "FR4",
      "Name": "F.Cu/B.Cu",
      "Notes": "Type: dielectric layer 1 (from F.Cu to B.Cu)"
    },
    {
      "Type": "Copper",
      "Thickness": 0.035,
      "Name": "B.Cu"
    },
    {
      "Type": "SolderMask",
      "Color": "Green",
      "Thickness": 0.01,
      "Name": "Bottom Solder Mask"
    },
    {
      "Type": "SolderPaste",
      "Name": "Bottom Solder Paste"
    },
    {
      "Type": "Legend",
      "Name": "Bottom Silk Screen"
    }
  ]
}
""";
    }

    public static string GenerateDrillFile(IDrawingNode? drawing, string projectName = "arduinouno")
    {
        var sb = new StringBuilder();
        sb.AppendLine("M48");
        sb.AppendLine("; DRILL file for " + projectName);
        sb.AppendLine("; FORMAT={-:-/ absolute / metric / keep zeros}");
        sb.AppendLine("METRIC,TZ");
        sb.AppendLine("; Tool list");
        sb.AppendLine("T1C0.900");  // Pin header holes: 0.9mm
        sb.AppendLine("T2C3.200");  // Mounting holes: 3.2mm
        sb.AppendLine("%");
        sb.AppendLine("G90");
        sb.AppendLine("G05");

        // Tool 1: 0.9mm pin header through-holes
        sb.AppendLine("T1");
        foreach (var pin in HeaderPins)
        {
            var xStr = ((long)Math.Round(pin.X * 1000)).ToString(CultureInfo.InvariantCulture);
            var yStr = ((long)Math.Round(pin.Y * 1000)).ToString(CultureInfo.InvariantCulture);
            sb.AppendLine($"X{xStr}Y{yStr}");
        }

        // Tool 2: 3.2mm mounting holes
        sb.AppendLine("T2");
        foreach (var hole in MountingHoles)
        {
            var xStr = ((long)Math.Round(hole.X * 1000)).ToString(CultureInfo.InvariantCulture);
            var yStr = ((long)Math.Round(hole.Y * 1000)).ToString(CultureInfo.InvariantCulture);
            sb.AppendLine($"X{xStr}Y{yStr}");
        }

        sb.AppendLine("M30");
        return sb.ToString();
    }

    public static async Task ExportGerberPackageZipAsync(IDrawingNode? drawing, Stream destinationStream, string projectName = "arduinouno")
    {
        using var archive = new ZipArchive(destinationStream, ZipArchiveMode.Create, leaveOpen: true);

        // Add each Gerber layer
        await AddZipEntryAsync(archive, $"{projectName}-F_Cu.gbr", GenerateTopCopper(drawing, projectName));
        await AddZipEntryAsync(archive, $"{projectName}-B_Cu.gbr", GenerateBottomCopper(drawing, projectName));
        await AddZipEntryAsync(archive, $"{projectName}-F_Silkscreen.gbr", GenerateTopSilkscreen(drawing, projectName));
        await AddZipEntryAsync(archive, $"{projectName}-B_Silkscreen.gbr", GenerateBottomSilkscreen(drawing, projectName));
        await AddZipEntryAsync(archive, $"{projectName}-Edge_Cuts.gbr", GenerateEdgeCuts(drawing, projectName));
        await AddZipEntryAsync(archive, $"{projectName}-job.gbrjob", GenerateJobFile(drawing, projectName));
        await AddZipEntryAsync(archive, $"{projectName}.drl", GenerateDrillFile(drawing, projectName));

        var readmeContent = $"""
NodePCB KiCad-Compatible Manufacturing Package
Project: {projectName}
Export Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
Target: KiCad 10.0.6 Pcbnew RS-274X Compatible

Files included:
  - {projectName}-F_Cu.gbr: Top Copper Layer (Pads & Bezier traces)
  - {projectName}-B_Cu.gbr: Bottom Copper Layer (Through-hole Pads)
  - {projectName}-F_Silkscreen.gbr: Top Silkscreen Layer (Headers & Legends)
  - {projectName}-B_Silkscreen.gbr: Bottom Silkscreen Layer
  - {projectName}-Edge_Cuts.gbr: Board Mechanical Outline (Arduino Uno Profile)
  - {projectName}-job.gbrjob: KiCad 10 Job Specification
  - {projectName}.drl: Excellon NC Drill File (Headers & Mounting Holes)
""";
        await AddZipEntryAsync(archive, "README.txt", readmeContent);
    }

    private static async Task AddZipEntryAsync(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await using var writer = new StreamWriter(entryStream, Encoding.UTF8);
        await writer.WriteAsync(content);
    }

    private static List<(double X, double Y)> GetConnectorGerberPoints(ICommonConnector connector, IDrawingNode drawing)
    {
        var result = new List<(double X, double Y)>();
        if (connector.Start is null || connector.End is null)
        {
            return result;
        }

        double p0X = connector.Start.X + (connector.Start.Parent?.X ?? 0);
        double p0Y = connector.Start.Y + (connector.Start.Parent?.Y ?? 0);
        double p3X = connector.End.X + (connector.End.Parent?.X ?? 0);
        double p3Y = connector.End.Y + (connector.End.Parent?.Y ?? 0);

        double p1X = p0X;
        double p1Y = p0Y;
        double p2X = p3X;
        double p2Y = p3Y;

        if (connector is IBezierConnector bezier && bezier.StartControl is { } && bezier.EndControl is { })
        {
            p1X = bezier.StartControl.X + (bezier.StartControl.Parent?.X ?? 0);
            p1Y = bezier.StartControl.Y + (bezier.StartControl.Parent?.Y ?? 0);
            p2X = bezier.EndControl.X + (bezier.EndControl.Parent?.X ?? 0);
            p2Y = bezier.EndControl.Y + (bezier.EndControl.Parent?.Y ?? 0);
        }
        else
        {
            connector.GetControlPoints(
                connector.Orientation,
                connector.Offset,
                connector.Start.Alignment,
                connector.End.Alignment,
                ref p1X, ref p1Y,
                ref p2X, ref p2Y);
        }

        // Map canvas points to Gerber coordinates
        // Canvas scale: ~4 px/mm. Arduino Uno canvas size: 280x215. Physical size: 68.58 x 53.34 mm.
        double MapX(double px) => BaseOriginX + (px / 4.0);
        double MapY(double py) => BaseOriginY + (py / 4.0);

        // Subdivide cubic Bezier curve into 20 segments
        int steps = 20;
        for (int i = 0; i <= steps; i++)
        {
            double t = (double)i / steps;
            double u = 1.0 - t;
            double x = u * u * u * p0X + 3 * u * u * t * p1X + 3 * u * t * t * p2X + t * t * t * p3X;
            double y = u * u * u * p0Y + 3 * u * u * t * p1Y + 3 * u * t * t * p2Y + t * t * t * p3Y;
            result.Add((MapX(x), MapY(y)));
        }

        return result;
    }
}
