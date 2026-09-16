using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditorDemo.Models;

namespace NodeEditorDemo.ViewModels;

public partial class ComponentViewModel : ViewModelBase
{
    [ObservableProperty] private ComponentDefinition _definition;
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _value = "";
    [ObservableProperty] private string _package = "";
    [ObservableProperty] private bool _isSelected;

    public string DisplayTitle => string.IsNullOrWhiteSpace(Value) ? Name : $"{Name} ({Value})";

    public ComponentViewModel(ComponentDefinition definition)
    {
        _definition = definition;
        _name = definition.DesignatorPrefix;
        _value = definition.Value;
        _package = definition.Package;
    }

    public ComponentViewModel()
    {
        _definition = new ComponentDefinition();
    }

    partial void OnValueChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }

    partial void OnNameChanged(string value)
    {
        OnPropertyChanged(nameof(DisplayTitle));
    }
}
