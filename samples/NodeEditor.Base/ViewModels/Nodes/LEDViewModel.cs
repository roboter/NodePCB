using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class LEDViewModel : ViewModelBase
{
    [ObservableProperty] private string? _color = "Red";
    [ObservableProperty] private string? _forwardVoltage = "1.8V";
    [ObservableProperty] private string? _forwardCurrent = "20mA";
    [ObservableProperty] private string? _package = "5mm";
    [ObservableProperty] private LEDType _ledType = LEDType.Standard;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isOn;

    public string DisplaySymbol => LedType switch
    {
        LEDType.Standard => "LED",
        LEDType.RGB => "RGB",
        LEDType.IR => "IR",
        LEDType.UV => "UV",
        LEDType.Laser => "LD",
        _ => "LED"
    };

    public string ColorCode => Color?.ToUpper() switch
    {
        "RED" => "#FF0000",
        "GREEN" => "#00FF00",
        "BLUE" => "#0000FF",
        "YELLOW" => "#FFFF00",
        "ORANGE" => "#FF8000",
        "WHITE" => "#FFFFFF",
        "PINK" => "#FF69B4",
        "PURPLE" => "#8A2BE2",
        _ => "#FF0000"
    };

    public LEDViewModel()
    {
        Color = "Red";
        ForwardVoltage = "1.8V";
        ForwardCurrent = "20mA";
        Package = "5mm";
        LedType = LEDType.Standard;
        IsOn = false;
    }

    partial void OnColorChanged(string? value)
    {
        OnPropertyChanged(nameof(ColorCode));
    }

    partial void OnLedTypeChanged(LEDType value)
    {
        OnPropertyChanged(nameof(DisplaySymbol));
        UpdateDefaultValues();
    }

    private void UpdateDefaultValues()
    {
        switch (LedType)
        {
            case LEDType.IR:
                ForwardVoltage = "1.2V";
                Color = "Infrared";
                break;
            case LEDType.UV:
                ForwardVoltage = "3.4V";
                Color = "Ultraviolet";
                break;
            case LEDType.RGB:
                ForwardVoltage = "3.3V";
                Color = "RGB";
                break;
            case LEDType.Laser:
                ForwardVoltage = "2.1V";
                Color = "Red";
                break;
            default:
                ForwardVoltage = "1.8V";
                break;
        }
    }
}

public enum LEDType
{
    Standard,
    RGB,
    IR,
    UV,
    Laser
}
