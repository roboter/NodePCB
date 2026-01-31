using CommunityToolkit.Mvvm.ComponentModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class TransistorViewModel : ViewModelBase
{
    [ObservableProperty] private string? _partNumber = "2N2222";
    [ObservableProperty] private TransistorType _transistorType = TransistorType.NPN;
    [ObservableProperty] private string? _maxCollectorCurrent = "800mA";
    [ObservableProperty] private string? _maxCollectorVoltage = "40V";
    [ObservableProperty] private string? _hfe = "100-300";
    [ObservableProperty] private string? _package = "TO-92";
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private TransistorConfiguration _configuration = TransistorConfiguration.BJT;

    public string DisplaySymbol => Configuration switch
    {
        TransistorConfiguration.BJT => TransistorType == TransistorType.NPN ? "NPN" : "PNP",
        TransistorConfiguration.MOSFET => TransistorType == TransistorType.NPN ? "N-MOS" : "P-MOS",
        TransistorConfiguration.JFET => TransistorType == TransistorType.NPN ? "N-JFET" : "P-JFET",
        _ => "Q"
    };

    public string PinLabels => Configuration switch
    {
        TransistorConfiguration.BJT => "E,B,C",
        TransistorConfiguration.MOSFET => "S,G,D",
        TransistorConfiguration.JFET => "S,G,D",
        _ => "1,2,3"
    };

    public string ArrowDirection => TransistorType == TransistorType.NPN ? "↗" : "↙";

    public TransistorViewModel()
    {
        PartNumber = "2N2222";
        TransistorType = TransistorType.NPN;
        MaxCollectorCurrent = "800mA";
        MaxCollectorVoltage = "40V";
        Hfe = "100-300";
        Package = "TO-92";
        Configuration = TransistorConfiguration.BJT;
    }

    partial void OnTransistorTypeChanged(TransistorType value)
    {
        OnPropertyChanged(nameof(DisplaySymbol));
        OnPropertyChanged(nameof(ArrowDirection));
        UpdateDefaultValues();
    }

    partial void OnConfigurationChanged(TransistorConfiguration value)
    {
        OnPropertyChanged(nameof(DisplaySymbol));
        OnPropertyChanged(nameof(PinLabels));
        UpdateDefaultValues();
    }

    private void UpdateDefaultValues()
    {
        switch (Configuration)
        {
            case TransistorConfiguration.BJT:
                if (TransistorType == TransistorType.NPN)
                {
                    PartNumber = "2N2222";
                    MaxCollectorCurrent = "800mA";
                    MaxCollectorVoltage = "40V";
                    Hfe = "100-300";
                }
                else
                {
                    PartNumber = "2N2907";
                    MaxCollectorCurrent = "600mA";
                    MaxCollectorVoltage = "-40V";
                    Hfe = "100-300";
                }
                break;

            case TransistorConfiguration.MOSFET:
                if (TransistorType == TransistorType.NPN)
                {
                    PartNumber = "2N7000";
                    MaxCollectorCurrent = "200mA";
                    MaxCollectorVoltage = "60V";
                    Hfe = "N/A";
                }
                else
                {
                    PartNumber = "BS250";
                    MaxCollectorCurrent = "230mA";
                    MaxCollectorVoltage = "-45V";
                    Hfe = "N/A";
                }
                break;

            case TransistorConfiguration.JFET:
                PartNumber = "2N5457";
                MaxCollectorCurrent = "10mA";
                MaxCollectorVoltage = "25V";
                Hfe = "N/A";
                break;
        }
    }
}

public enum TransistorType
{
    NPN,
    PNP
}

public enum TransistorConfiguration
{
    BJT,
    MOSFET,
    JFET
}
