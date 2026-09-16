using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NodeEditor.Model;

namespace NodeEditorDemo.Models;

public class ComponentDefinition
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "ThroughHole";

    [JsonPropertyName("designatorPrefix")]
    public string DesignatorPrefix { get; set; } = "U";

    [JsonPropertyName("value")]
    public string Value { get; set; } = "";

    [JsonPropertyName("package")]
    public string Package { get; set; } = "";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("width")]
    public double Width { get; set; } = 101.6;

    [JsonPropertyName("height")]
    public double Height { get; set; } = 25.4;

    [JsonPropertyName("outline")]
    public List<OutlineShapeDefinition> Outline { get; set; } = new();

    [JsonPropertyName("pins")]
    public List<PinDefinitionItem> Pins { get; set; } = new();

    [JsonPropertyName("mountingHoles")]
    public List<MountingHoleDefinition> MountingHoles { get; set; } = new();

    [JsonPropertyName("labels")]
    public LabelsDefinition? Labels { get; set; }

    [JsonPropertyName("kicad")]
    public KiCadMetadata? KiCad { get; set; }
}

public class OutlineShapeDefinition
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Rectangle"; // Rectangle, Line, Ellipse, Path, Polyline

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }

    [JsonPropertyName("x1")]
    public double X1 { get; set; }

    [JsonPropertyName("y1")]
    public double Y1 { get; set; }

    [JsonPropertyName("x2")]
    public double X2 { get; set; }

    [JsonPropertyName("y2")]
    public double Y2 { get; set; }

    [JsonPropertyName("points")]
    public string? Points { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("fill")]
    public string? Fill { get; set; }

    [JsonPropertyName("stroke")]
    public string? Stroke { get; set; }

    [JsonPropertyName("strokeWidth")]
    public double StrokeWidth { get; set; } = 1.0;

    [JsonPropertyName("radius")]
    public double Radius { get; set; }
}

public class PinDefinitionItem
{
    [JsonPropertyName("number")]
    public int Number { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("net")]
    public string? Net { get; set; }

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("alignment")]
    public string Alignment { get; set; } = "None"; // None, Left, Right, Top, Bottom

    [JsonPropertyName("padShape")]
    public string PadShape { get; set; } = "Circle"; // Circle, Rectangle, Oval

    [JsonPropertyName("padWidth")]
    public double PadWidth { get; set; } = 1.7;

    [JsonPropertyName("padHeight")]
    public double PadHeight { get; set; } = 1.7;

    [JsonPropertyName("drill")]
    public double Drill { get; set; } = 0.9;

    public PinAlignment GetPinAlignment() => Alignment switch
    {
        "Left" => PinAlignment.Left,
        "Right" => PinAlignment.Right,
        "Top" => PinAlignment.Top,
        "Bottom" => PinAlignment.Bottom,
        _ => PinAlignment.None
    };
}

public class MountingHoleDefinition
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("diameter")]
    public double Diameter { get; set; } = 3.2;

    [JsonPropertyName("collarDiameter")]
    public double CollarDiameter { get; set; } = 6.5;

    [JsonPropertyName("plated")]
    public bool Plated { get; set; } = false;
}

public class LabelsDefinition
{
    [JsonPropertyName("designator")]
    public LabelLocation? Designator { get; set; }

    [JsonPropertyName("value")]
    public LabelLocation? Value { get; set; }
}

public class LabelLocation
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("horizontalAlignment")]
    public string HorizontalAlignment { get; set; } = "Center";

    [JsonPropertyName("fontSize")]
    public double FontSize { get; set; } = 9.0;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#FFFFFF";

    [JsonPropertyName("fontWeight")]
    public string FontWeight { get; set; } = "Normal";
}

public class KiCadMetadata
{
    [JsonPropertyName("footprint")]
    public string? Footprint { get; set; }

    [JsonPropertyName("pinPitch")]
    public double PinPitch { get; set; } = 2.54;

    [JsonPropertyName("padSpan")]
    public double PadSpan { get; set; }
}
