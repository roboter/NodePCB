using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class CapacitorViewModel : ViewModelBase
{
    [ObservableProperty] private string? _value = "100µF";
    [ObservableProperty] private string? _voltage = "25V";
    [ObservableProperty] private string? _tolerance = "20%";
    [ObservableProperty] private string? _package = "0805";
    [ObservableProperty] private CapacitorType _capacitorType = CapacitorType.Ceramic;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isPolarized;

    public string DisplayValue => !string.IsNullOrEmpty(Value) ? Value : "C";

    public string TypeSymbol => CapacitorType switch
    {
        CapacitorType.Electrolytic => "C+",
        CapacitorType.Tantalum => "C+",
        CapacitorType.Ceramic => "C",
        CapacitorType.Film => "C",
        _ => "C"
    };

    public CapacitorViewModel()
    {
        Value = "100µF";
        Voltage = "25V";
        Tolerance = "20%";
        Package = "0805";
        CapacitorType = CapacitorType.Ceramic;
        UpdatePolarization();
    }

    partial void OnCapacitorTypeChanged(CapacitorType value)
    {
        UpdatePolarization();
        OnPropertyChanged(nameof(TypeSymbol));
    }

    partial void OnValueChanged(string? value)
    {
        OnPropertyChanged(nameof(DisplayValue));
    }

    private void UpdatePolarization()
    {
        IsPolarized = CapacitorType == CapacitorType.Electrolytic || CapacitorType == CapacitorType.Tantalum;
    }
}

public enum CapacitorType
{
    Ceramic,
    Electrolytic,
    Tantalum,
    Film
}
