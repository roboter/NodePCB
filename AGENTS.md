# 🛠️ AGENTS.md — NodePCB Avalonia UI Engineering Guidelines & Best Practices

> **Target Audience**: AI Agents, LLMs, and Software Engineers developing, extending, or refactoring the **NodePCB** codebase.
> **Technology Stack**: .NET 10.0 • Avalonia UI 11.0+ • C# 12/13 • CommunityToolkit.Mvvm • ReactiveMarbles

---

## 1. Architectural Blueprint & Separation of Concerns

The NodePCB solution enforces a strict multi-tier architecture. Maintaining boundaries between these projects is mandatory:

```
NodePCB/
├── src/
│   ├── NodeEditorAvalonia.Model/   # 1. PURE DOMAIN ABSTRACTION (Zero UI Dependencies)
│   ├── NodeEditorAvalonia.Mvvm/    # 2. MVVM STATE & REACTIVE ENGINE (CommunityToolkit + ReactiveMarbles)
│   └── NodeEditorAvalonia/         # 3. CONTROLS, BEHAVIORS, HIT-TESTING & THEMES (Avalonia UI)
├── samples/
│   ├── NodeEditor.Base/           # 4. COMPONENT LIBRARY, FACTORIES & VIEW LOCATOR
│   ├── NodeEditor.Desktop/        # 5. Desktop Application Host (Win/macOS/Linux)
│   └── NodeEditor.Web/            # 6. WebAssembly Host (Browser)
└── tests/
    └── NodeEditorAvalonia.UnitTests/ # 7. Automated Unit & Geometry Tests
```

### Layer Dependency Rules

| Project | Allowed References | Prohibited References | Primary Responsibility |
| :--- | :--- | :--- | :--- |
| `NodeEditorAvalonia.Model` | *None* (Standard .NET library) | Avalonia UI, GUI packages | Pure domain interfaces (`INode`, `IPin`, `IBezierConnector`, `IDrawingNode`) and enums (`PinAlignment`, `ConnectorOrientation`). |
| `NodeEditorAvalonia.Mvvm` | `NodeEditorAvalonia.Model`, `CommunityToolkit.Mvvm`, `ReactiveMarbles` | `NodeEditorAvalonia` (UI controls) | ViewModels implementing domain interfaces, reactive pin/node change tracking, serialization DTOs. |
| `NodeEditorAvalonia` | `NodeEditorAvalonia.Model`, Avalonia UI, `Avalonia.Xaml.Interactivity` | `NodeEditorAvalonia.Mvvm`, Application-specific views | Custom templated controls (`Editor`, `DrawingNode`), direct-rendering controls (`BezierConnector`), XAML behaviors, and themes. |
| `NodeEditor.Base` | All above projects | Platform-specific APIs (`P/Invoke`, etc.) | Electronic component Views/ViewModels (Resistors, ICs, Transistors), `NodeFactory`, `ViewLocator`, and export renderers. |

> [!IMPORTANT]
> **Never reference Avalonia UI in `NodeEditorAvalonia.Model`.**
> This keeps layout mathematics, serialization, graph routing, and unit tests completely headless and decoupled from windowing or rendering contexts.

---

## 2. Avalonia Control Authoring Standards

NodePCB utilizes distinct control paradigms depending on performance and templating requirements:

### A. When to Use Which Control Base Class

```
                ┌───────────────────────────────┐
                │ What are you building?        │
                └───────────────┬───────────────┘
                                │
        ┌───────────────────────┼────────────────────────┐
        ▼                       ▼                        ▼
[Composite Layout]      [Reusable Canvas Widget]   [High-Speed Trace/Line]
   UserControl              TemplatedControl               Control
  (e.g. ResistorView)     (e.g. Editor, DrawingNode) (e.g. BezierConnector)
```

1. **`Control` (Direct Rendering)**:
   - Use when you need direct, high-frequency drawing via `DrawingContext.Render()`.
   - Example: [`BezierConnector`](file:///e:/GitHub/NodePCB/src/NodeEditorAvalonia/Controls/Connector.cs).
   - Skips template expansion and child visual tree overhead; optimal for curves, traces, and dynamic vectors.

2. **`TemplatedControl` (Stylable UI Building Blocks)**:
   - Use for controls whose visual appearance must be customizable through `ControlTheme` and `ControlTemplate`.
   - Example: [`Editor`](file:///e:/GitHub/NodePCB/src/NodeEditorAvalonia/Controls/Editor.cs) and [`DrawingNode`](file:///e:/GitHub/NodePCB/src/NodeEditorAvalonia/Controls/DrawingNode.cs).
   - Always declare template parts with `[TemplatePart]` attributes.

3. **`UserControl` (Domain Composite Views)**:
   - Use in `NodeEditor.Base` for domain-specific components (e.g., [`ResistorView`](file:///e:/GitHub/NodePCB/samples/NodeEditor.Base/Views/Nodes/ResistorView.axaml), [`MicrocontrollerView`](file:///e:/GitHub/NodePCB/samples/NodeEditor.Base/Views/Nodes/MicrocontrollerView.axaml)).

---

### B. StyledProperty & Visual Invalidation Rules

Always use Avalonia's dependency property system correctly:

```csharp
public class BezierConnector : Control
{
    // 1. Register StyledProperty with type-safe owner and value
    public static readonly StyledProperty<Point> StartPointProperty =
        AvaloniaProperty.Register<BezierConnector, Point>(nameof(StartPoint));

    public static readonly StyledProperty<bool> ShowControlPointsProperty =
        AvaloniaProperty.Register<BezierConnector, bool>(nameof(ShowControlPoints), false);

    // 2. Register metadata in static constructor
    static BezierConnector()
    {
        // Declarative invalidation: NEVER manually call InvalidateVisual() in property setters!
        AffectsRender<BezierConnector>(
            StartPointProperty,
            EndPointProperty,
            StartControlPointProperty,
            EndControlPointProperty,
            ShowControlPointsProperty);
    }

    // 3. Clean property accessors
    public Point StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }
}
```

#### Core Invalidation Best Practices:
- Use `AffectsRender<T>(...)` for visual property changes (strokes, colors, coordinates).
- Use `AffectsGeometry<T>(...)` when deriving from `Shape` and overriding `CreateDefiningGeometry()`.
- Use `AffectsMeasure<T>(...)` and `AffectsArrange<T>(...)` for properties altering layout dimensions.
- React to property changes cleanly using `OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)`.

---

### C. PseudoClasses Implementation

For visual state tracking (like `:selected`, `:pointerover`, `:pressed`):

```csharp
[PseudoClasses(":selected")]
public class BezierConnector : Control
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ShowControlPointsProperty)
        {
            // Update pseudoclass state
            PseudoClasses.Set(":selected", change.GetNewValue<bool>());
        }
    }
}
```

In AXAML, style pseudoclasses with modern Avalonia 11 nested selector syntax:

```xml
<ControlTheme x:Key="{x:Type controls:BezierConnector}" TargetType="controls:BezierConnector">
  <Setter Property="Stroke" Value="{DynamicResource ConnectorBackgroundBrush}" />
  
  <!-- Avalonia 11 Nested Selector for PseudoClass -->
  <Style Selector="^:selected">
    <Setter Property="Stroke" Value="#179DE3" />
  </Style>
</ControlTheme>
```

---

### D. TemplatedControl Contracts & Template Parts

When building templated controls:
1. Specify part contracts using `[TemplatePart]` attributes.
2. Follow the `PART_` naming convention.
3. Query parts exclusively inside `OnApplyTemplate`.

```csharp
[TemplatePart("PART_ZoomBorder", typeof(NodeZoomBorder))]
[TemplatePart("PART_DrawingNode", typeof(DrawingNode))]
[TemplatePart("PART_AdornerCanvas", typeof(Canvas))]
public class Editor : TemplatedControl
{
    public NodeZoomBorder? ZoomControl { get; private set; }
    public DrawingNode? DrawingNode { get; private set; }
    public Canvas? AdornerCanvas { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ZoomControl = e.NameScope.Find<NodeZoomBorder>("PART_ZoomBorder");
        DrawingNode = e.NameScope.Find<DrawingNode>("PART_DrawingNode");
        AdornerCanvas = e.NameScope.Find<Canvas>("PART_AdornerCanvas");
    }
}
```

---

## 3. High-Performance Direct Rendering (Zero-Allocation Rule)

Direct rendering inside `Render(DrawingContext context)` executes on every frame or property change. **Garbage collection pauses will stutter canvas interactions.**

### ❌ Anti-Pattern (Allocations in Render Loop):
```csharp
public override void Render(DrawingContext context)
{
    // AVOID: Allocates new brushes and pens on every render tick!
    var brush = new SolidColorBrush(Colors.Cyan);
    var pen = new Pen(brush, 2.0);
    context.DrawLine(pen, StartPoint, EndPoint);
}
```

### ✅ Best Practice (Immutable Primitives):
```csharp
public override void Render(DrawingContext context)
{
    base.Render(context);

    var isSelected = Classes.Contains(":selected") || ShowControlPoints;
    var strokeBrush = isSelected
        ? new ImmutableSolidColorBrush(Color.FromRgb(0x17, 0x9D, 0xE3))
        : (Stroke ?? new ImmutableSolidColorBrush(Colors.Red));

    // ImmutablePen and ImmutableDashStyle prevent resource churn
    var curvePen = new ImmutablePen(strokeBrush.ToImmutable(), StrokeThickness, lineCap: PenLineCap.Round);

    var geometry = new StreamGeometry();
    using (var ctx = geometry.Open())
    {
        ctx.BeginFigure(StartPoint, false);
        ctx.CubicBezierTo(StartControlPoint, EndControlPoint, EndPoint);
        ctx.EndFigure(false);
    }
    
    context.DrawGeometry(null, curvePen, geometry);
}
```

### Performance Checklist for Drawing:
1. **Use `ImmutableSolidColorBrush` and `ImmutablePen`**: Immutable objects are thread-safe and avoid Avalonia subscription/observable overhead.
2. **StreamGeometry over PathGeometry**: `StreamGeometry` is lightweight and Skia-optimized.
3. **ClipToBounds**: Always set `ClipToBounds = false` when controls render handles or glow effects outside their assigned bounds.
4. **Adorner Overlay Separation**: Keep interactive drag marquees and selection boxes in a transparent overlay canvas (`PART_AdornerCanvas`) with `IsHitTestVisible="False"`.

---

## 4. Pointer Events, Drag Behaviors & Interactive Hit Testing

Complex interactive canvas mechanics (e.g., node dragging, marquee selection, Bezier handle manipulation) must be encapsulated in **XAML Behaviors**, not hard-coded into control classes.

### A. Behavior Architecture (`Avalonia.Xaml.Interactivity`)

Behaviors inherit from `Behavior<T>` (typically `Behavior<ItemsControl>`):

```csharp
public class DrawingSelectionBehavior : Behavior<ItemsControl>
{
    private Control? _inputSource;

    private void Initialize()
    {
        if (AssociatedObject is null || InputSource is null) return;
        _inputSource = InputSource;

        // Use Tunnel | Bubble to capture gestures cleanly
        _inputSource.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.PointerCaptureLostEvent, CaptureLost, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        _inputSource.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    private void DeInitialize()
    {
        if (_inputSource is null) return;
        _inputSource.RemoveHandler(InputElement.PointerPressedEvent, Pressed);
        _inputSource.RemoveHandler(InputElement.PointerReleasedEvent, Released);
        _inputSource.RemoveHandler(InputElement.PointerCaptureLostEvent, CaptureLost);
        _inputSource.RemoveHandler(InputElement.PointerMovedEvent, Moved);
        _inputSource = null;
    }
}
```

### B. Pointer Capture & Lifecycle Rules
- **Capture Pointer on Drag**: Always call `e.Pointer.Capture(_inputSource)` on `PointerPressed`.
- **Release Pointer on Up**: Always call `e.Pointer.Capture(null)` in `PointerReleased`.
- **Handle Capture Loss**: Always listen for `PointerCaptureLostEvent` to clean up dragging state if the OS or window manager steals focus (e.g., Alt-Tab or gesture dismissal).

### C. Visual Coordinate Transformations
Never assume mouse coordinates match node coordinates. Always transform via Avalonia's visual tree:

```csharp
// Get coordinates relative to the drawing canvas
var position = e.GetPosition(AssociatedObject);

// Check transform relative to visual root if needed
var root = AssociatedObject.GetVisualRoot();
var matrix = AssociatedObject.TransformToVisual(root as Visual);
```

### D. Geometric Hit Testing
Avalonia's standard `VisualTreeHelper.HitTest` only tests rectangular bounding boxes. For curved traces and Bezier paths, use mathematical hit testing via [`HitTestHelper`](file:///e:/GitHub/NodePCB/src/NodeEditorAvalonia/HitTestHelper.cs):

```csharp
// Subdivide cubic curve into discrete segments and check point-to-segment distance
public static Point[] FlattenCubic(Point pt0, Point pt1, Point pt2, Point pt3)
{
    var count = (int)Math.Max(1, Length(pt0, pt1) + Length(pt1, pt2) + Length(pt2, pt3));
    var points = new Point[count];
    for (var i = 0; i < count; i++)
    {
        var t = (i + 1d) / count;
        // Cubic Bezier formula: B(t) = (1-t)³P0 + 3(1-t)²tP1 + 3(1-t)t²P2 + t³P3
        var x = Math.Pow(1 - t, 3) * pt0.X + 3 * t * Math.Pow(1 - t, 2) * pt1.X + 3 * t * t * (1 - t) * pt2.X + Math.Pow(t, 3) * pt3.X;
        var y = Math.Pow(1 - t, 3) * pt0.Y + 3 * t * Math.Pow(1 - t, 2) * pt1.Y + 3 * t * t * (1 - t) * pt2.Y + Math.Pow(t, 3) * pt3.Y;
        points[i] = new Point(x, y);
    }
    return points;
}
```

---

## 5. Modern AXAML, Theming & Compiled Bindings

NodePCB is built with **Avalonia 11**. Legacy Avalonia 0.10 patterns (such as uncompiled bindings or global `<Style>` blocks for control definitions) are strictly forbidden.

### A. Compiled Bindings (`x:CompileBindings="True"`)
Always enforce compile-time binding validation:
1. Declare `x:CompileBindings="True"` on root `<ResourceDictionary>`, `<UserControl>`, or `<Styles>`.
2. Specify `x:DataType` on all templates, flyouts, and user controls.
3. If an attached property binding requires runtime reflection (e.g., complex relative ancestor lookups), scope `x:CompileBindings="False"` to that specific element only.

```xml
<ResourceDictionary xmlns="https://github.com/avaloniaui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:m="clr-namespace:NodeEditor.Model;assembly=NodeEditorAvalonia.Model"
                    xmlns:controls="clr-namespace:NodeEditor.Controls"
                    x:CompileBindings="True">

  <ControlTheme x:Key="{x:Type controls:DrawingNode}" TargetType="controls:DrawingNode">
    <Setter Property="Template">
      <ControlTemplate x:DataType="m:IDrawingNode">
        <Panel Background="{TemplateBinding Background}">
          <!-- Strongly typed bindings validated at compile time -->
          <controls:Connectors ItemsSource="{Binding Connectors}" />
          <controls:Nodes ItemsSource="{Binding Nodes}" />
        </Panel>
      </ControlTemplate>
    </Setter>
  </ControlTheme>
</ResourceDictionary>
```

### B. ControlTheme Paradigm
- Use `<ControlTheme x:Key="{x:Type controls:MyControl}" TargetType="controls:MyControl">` instead of unkeyed styles.
- Allows themes to be referenced, overridden, or based on other themes (`BasedOn="{StaticResource ...}"`).

### C. Theme Dictionaries for Light / Dark Mode
Store color tokens in `ThemeDictionaries` to allow effortless variant switching:

```xml
<Styles xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        x:CompileBindings="True">
  <Styles.Resources>
    <ResourceDictionary>
      <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Default">
          <SolidColorBrush x:Key="EditorBackground">#1E1E1E</SolidColorBrush>
          <SolidColorBrush x:Key="ConnectorBackgroundBrush">#00E5FF</SolidColorBrush>
        </ResourceDictionary>
        <ResourceDictionary x:Key="Dark">
          <SolidColorBrush x:Key="EditorBackground">#121212</SolidColorBrush>
          <SolidColorBrush x:Key="ConnectorBackgroundBrush">#179DE3</SolidColorBrush>
        </ResourceDictionary>
      </ResourceDictionary.ThemeDictionaries>
    </ResourceDictionary>
  </Styles.Resources>
</Styles>
```

### D. Cross-Platform Input Gestures & Icons
NodePCB targets Windows, macOS, Linux, and WebAssembly. Shortcuts must support platform modifiers:

```xml
<MenuItem Header="Copy" 
          Command="{Binding CopyNodesCommand}" 
          InputGesture="{OnPlatform macOS=CMD+C, iOS=CMD+C, Default=Ctrl+C}">
  <MenuItem.Icon>
    <PathIcon Width="16" Height="16" Data="{DynamicResource EditorCopyIcon}" />
  </MenuItem.Icon>
</MenuItem>
```

---

## 6. MVVM & Reactive Property Architecture

NodePCB uses `CommunityToolkit.Mvvm` combined with `ReactiveMarbles.PropertyChanged` for high-speed, observable reactive updates.

### A. ViewModel Best Practices
- Inherit from `ObservableObject` or `ViewModelBase`.
- Use `[ObservableProperty]` source generators.
- Intercept property changes via generated partial methods (`partial void OnPropertyChanged(...)`).

```csharp
[ObservableObject]
public partial class BezierConnectorViewModel : IBezierConnector
{
    [ObservableProperty] private double _offset = 50.0;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private IPin? _start;
    [ObservableProperty] private IPin? _end;

    // Generated change hook
    partial void OnOffsetChanged(double value)
    {
        ResetControlPoints();
    }
}
```

### B. Reactive Pin & Parent Observation
When nodes are moved on the canvas, connected Bezier control points and traces must update reactively without polling:

```csharp
// ReactiveMarbles.PropertyChanged: WhenChanged()
this.WhenChanged(x => x.Start)
    .DistinctUntilChanged()
    .Subscribe(start =>
    {
        if (start?.Parent is NodeViewModel parentNode)
        {
            parentNode.WhenChanged(n => n.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
            parentNode.WhenChanged(n => n.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
        }
    });
```

### C. ViewLocator Convention
Views in `NodeEditor.Base` are automatically resolved by `ViewLocator` based on naming conventions (`*ViewModel` $\to$ `*View`):

```csharp
public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        var name = data?.GetType().FullName?.Replace("ViewModel", "View");
        var type = name is null ? null : Type.GetType(name);
        return type != null ? (Control)Activator.CreateInstance(type)! : new TextBlock { Text = "Not Found: " + name };
    }

    public bool Match(object? data) => data is ViewModelBase;
}
```

---

## 7. Electronic Component Recipe: Adding New Nodes

Follow this standard procedure whenever introducing new electronic components (e.g., Inductor, Op-Amp, Diode) to ensure seamless integration into the toolbox, canvas, serialization, and flyout editors:

### Step 1: Create the ViewModel
In [`samples/NodeEditor.Base/ViewModels/Nodes/`](file:///e:/GitHub/NodePCB/samples/NodeEditor.Base/ViewModels/Nodes/):
```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class InductorViewModel : ViewModelBase
{
    [ObservableProperty] private string? _value = "100µH";
    [ObservableProperty] private string? _currentRating = "1A";
    [ObservableProperty] private string? _package = "0805";
    [ObservableProperty] private bool _isSelected;

    public string DisplayValue => !string.IsNullOrEmpty(Value) ? Value : "L";

    partial void OnValueChanged(string? value) => OnPropertyChanged(nameof(DisplayValue));
}
```

### Step 2: Create the AXAML View
In [`samples/NodeEditor.Base/Views/Nodes/`](file:///e:/GitHub/NodePCB/samples/NodeEditor.Base/Views/Nodes/):
```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="clr-namespace:NodeEditorDemo.ViewModels.Nodes"
             x:Class="NodeEditorDemo.Views.Nodes.InductorView"
             x:CompileBindings="True" 
             x:DataType="vm:InductorViewModel"
             ClipToBounds="False">

  <Panel>
    <!-- Right-click property flyout -->
    <Panel.ContextFlyout>
      <Flyout>
        <StackPanel Width="220" Spacing="8">
          <Label Content="Inductor Settings" FontWeight="Bold" />
          <TextBox Text="{Binding Value}" Watermark="Value (e.g., 100µH)" />
          <TextBox Text="{Binding CurrentRating}" Watermark="Current Rating" />
        </StackPanel>
      </Flyout>
    </Panel.ContextFlyout>

    <!-- Schematic symbol canvas / drawing -->
    <Canvas Width="100" Height="40">
      <!-- Direct vector arcs, labels, and terminal indicators -->
    </Canvas>
  </Panel>
</UserControl>
```

### Step 3: Register in Component Factory
In [`samples/NodeEditor.Base/Services/NodeFactory.cs`](file:///e:/GitHub/NodePCB/samples/NodeEditor.Base/Services/):
- Define default dimensions, pin layout, pin alignments (`PinAlignment.Left`, `PinAlignment.Right`), and pin names.
- Register into the component templates host so it immediately appears in the draggable Toolbox.

---

## 8. Threading & Background Worker Safety

NodePCB operations such as Gerber exporting, layout auto-routing, and file I/O must not block the UI thread:

```csharp
// Background processing
Task.Run(() =>
{
    var routingResult = CalculateAutorouting();

    // ALWAYS marshal UI updates to the Avalonia Dispatcher
    Dispatcher.UIThread.Post(() =>
    {
        drawingNode.Connectors.Add(routingResult);
    }, DispatcherPriority.Render);
});
```

- Never call `.Result` or `.Wait()` on asynchronous tasks on the UI thread.
- Use `DispatcherPriority.Render` or `Normal` depending on whether the update affects layout.

---

## 9. AI Agent Quality & Integrity Checklist

Before committing any change to this codebase, verify each of the following:

- [ ] **Architecture Check**: Does `NodeEditorAvalonia.Model` remain free of any Avalonia UI references?
- [ ] **Binding Check**: Is `x:CompileBindings="True"` enabled on any new or modified AXAML file?
- [ ] **Data Type Check**: Is `x:DataType` specified on all templates, flyouts, and views?
- [ ] **Rendering Performance**: Are all brushes/pens inside `Render()` using `ImmutableSolidColorBrush` / `ImmutablePen`?
- [ ] **Zero-Allocation**: No `new Pen()` or LINQ iterations inside `Render(DrawingContext)` calls.
- [ ] **Invalidation**: Are `AffectsRender`, `AffectsGeometry`, or `AffectsMeasure` used instead of manual setter invalidations?
- [ ] **Behavior Cleanup**: Does your custom behavior properly unsubscribe from all events in `DeInitialize()`?
- [ ] **Platform Agnostic**: Are keyboard gestures wrapped with `{OnPlatform ...}`?
- [ ] **Unit Tests**: Have you run or added unit tests in `NodeEditorAvalonia.UnitTests`?
