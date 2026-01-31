using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class ResistorViewModel : ViewModelBase
{
    [ObservableProperty] private string? _value = "10kΩ";
    [ObservableProperty] private string? _tolerance = "5%";
    [ObservableProperty] private string? _power = "1/4W";
    [ObservableProperty] private string? _package = "0805";
    [ObservableProperty] private bool _isSelected;

    public string DisplayValue => !string.IsNullOrEmpty(Value) ? Value : "R";

    public ResistorViewModel()
    {
        Value = "10kΩ";
        Tolerance = "5%";
        Power = "1/4W";
        Package = "0805";
    }

    partial void OnValueChanged(string? value)
    {
        OnPropertyChanged(nameof(DisplayValue));
    }
}
