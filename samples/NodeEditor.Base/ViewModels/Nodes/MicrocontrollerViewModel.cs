using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace NodeEditorDemo.ViewModels.Nodes;

public partial class MicrocontrollerViewModel : ViewModelBase
{
    [ObservableProperty] private string? _partNumber = "Arduino Nano";
    [ObservableProperty] private MicrocontrollerType _microcontrollerType = MicrocontrollerType.Arduino;
    [ObservableProperty] private string? _clockSpeed = "16MHz";
    [ObservableProperty] private string? _voltage = "5V";
    [ObservableProperty] private string? _flashMemory = "32KB";
    [ObservableProperty] private string? _ramMemory = "2KB";
    [ObservableProperty] private string? _eepromMemory = "1KB";
    [ObservableProperty] private string? _package = "DIP-30";
    [ObservableProperty] private int _digitalPins = 14;
    [ObservableProperty] private int _analogPins = 8;
    [ObservableProperty] private int _pwmPins = 6;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isProgrammed;
    [ObservableProperty] private string? _firmwareVersion = "1.0.0";

    public ObservableCollection<PinDefinition> PinDefinitions { get; } = new();

    public string DisplaySymbol => MicrocontrollerType switch
    {
        MicrocontrollerType.Arduino => "ARD",
        MicrocontrollerType.ESP32 => "ESP32",
        MicrocontrollerType.ESP8266 => "ESP8266",
        MicrocontrollerType.STM32 => "STM32",
        MicrocontrollerType.PIC => "PIC",
        MicrocontrollerType.AVR => "AVR",
        MicrocontrollerType.ARM => "ARM",
        MicrocontrollerType.RaspberryPi => "RPi",
        _ => "MCU"
    };

    public string StatusIndicator => IsProgrammed ? "●" : "○";

    public MicrocontrollerViewModel()
    {
        PartNumber = "Arduino Nano";
        MicrocontrollerType = MicrocontrollerType.Arduino;
        ClockSpeed = "16MHz";
        Voltage = "5V";
        FlashMemory = "32KB";
        RamMemory = "2KB";
        EepromMemory = "1KB";
        Package = "DIP-30";
        DigitalPins = 14;
        AnalogPins = 8;
        PwmPins = 6;
        IsProgrammed = false;
        FirmwareVersion = "1.0.0";

        InitializePinDefinitions();
    }

    partial void OnMicrocontrollerTypeChanged(MicrocontrollerType value)
    {
        OnPropertyChanged(nameof(DisplaySymbol));
        UpdateDefaultValues();
        InitializePinDefinitions();
    }

    partial void OnIsProgrammedChanged(bool value)
    {
        OnPropertyChanged(nameof(StatusIndicator));
    }

    private void UpdateDefaultValues()
    {
        switch (MicrocontrollerType)
        {
            case MicrocontrollerType.Arduino:
                PartNumber = "Arduino Nano";
                ClockSpeed = "16MHz";
                Voltage = "5V";
                FlashMemory = "32KB";
                RamMemory = "2KB";
                EepromMemory = "1KB";
                Package = "DIP-30";
                DigitalPins = 14;
                AnalogPins = 8;
                PwmPins = 6;
                break;

            case MicrocontrollerType.ESP32:
                PartNumber = "ESP32-WROOM-32";
                ClockSpeed = "240MHz";
                Voltage = "3.3V";
                FlashMemory = "4MB";
                RamMemory = "520KB";
                EepromMemory = "N/A";
                Package = "QFN-48";
                DigitalPins = 34;
                AnalogPins = 18;
                PwmPins = 16;
                break;

            case MicrocontrollerType.ESP8266:
                PartNumber = "ESP8266-12E";
                ClockSpeed = "80MHz";
                Voltage = "3.3V";
                FlashMemory = "4MB";
                RamMemory = "80KB";
                EepromMemory = "N/A";
                Package = "QFN-32";
                DigitalPins = 17;
                AnalogPins = 1;
                PwmPins = 10;
                break;

            case MicrocontrollerType.STM32:
                PartNumber = "STM32F103C8";
                ClockSpeed = "72MHz";
                Voltage = "3.3V";
                FlashMemory = "64KB";
                RamMemory = "20KB";
                EepromMemory = "N/A";
                Package = "LQFP-48";
                DigitalPins = 37;
                AnalogPins = 10;
                PwmPins = 12;
                break;

            case MicrocontrollerType.PIC:
                PartNumber = "PIC18F4550";
                ClockSpeed = "48MHz";
                Voltage = "5V";
                FlashMemory = "32KB";
                RamMemory = "2KB";
                EepromMemory = "256B";
                Package = "DIP-40";
                DigitalPins = 35;
                AnalogPins = 13;
                PwmPins = 4;
                break;

            case MicrocontrollerType.RaspberryPi:
                PartNumber = "Raspberry Pi 4B";
                ClockSpeed = "1.5GHz";
                Voltage = "5V";
                FlashMemory = "N/A";
                RamMemory = "4GB";
                EepromMemory = "N/A";
                Package = "SBC";
                DigitalPins = 40;
                AnalogPins = 0;
                PwmPins = 2;
                break;
        }
    }

    private void InitializePinDefinitions()
    {
        PinDefinitions.Clear();

        switch (MicrocontrollerType)
        {
            case MicrocontrollerType.Arduino:
                InitializeArduinoPins();
                break;
            case MicrocontrollerType.ESP32:
                InitializeESP32Pins();
                break;
            case MicrocontrollerType.ESP8266:
                InitializeESP8266Pins();
                break;
            default:
                InitializeGenericPins();
                break;
        }
    }

    private void InitializeArduinoPins()
    {
        // Arduino Nano pin layout
        PinDefinitions.Add(new PinDefinition("D0", "RX", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D1", "TX", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D2", "Digital", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D3", "PWM", PinType.PWM));
        PinDefinitions.Add(new PinDefinition("D4", "Digital", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D5", "PWM", PinType.PWM));
        PinDefinitions.Add(new PinDefinition("D6", "PWM", PinType.PWM));
        PinDefinitions.Add(new PinDefinition("D7", "Digital", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D8", "Digital", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D9", "PWM", PinType.PWM));
        PinDefinitions.Add(new PinDefinition("D10", "PWM/SS", PinType.PWM));
        PinDefinitions.Add(new PinDefinition("D11", "PWM/MOSI", PinType.PWM));
        PinDefinitions.Add(new PinDefinition("D12", "MISO", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("D13", "SCK/LED", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("A0", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A1", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A2", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A3", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A4", "SDA", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A5", "SCL", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A6", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("A7", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("3V3", "Power", PinType.Power));
        PinDefinitions.Add(new PinDefinition("5V", "Power", PinType.Power));
        PinDefinitions.Add(new PinDefinition("GND", "Ground", PinType.Ground));
        PinDefinitions.Add(new PinDefinition("VIN", "Power Input", PinType.Power));
        PinDefinitions.Add(new PinDefinition("RST", "Reset", PinType.Digital));
    }

    private void InitializeESP32Pins()
    {
        // Key ESP32 pins
        for (int i = 0; i <= 39; i++)
        {
            if (i >= 6 && i <= 11) continue; // Skip flash pins

            string pinType = i switch
            {
                >= 32 and <= 39 => "ADC",
                _ => "GPIO"
            };

            PinDefinitions.Add(new PinDefinition($"GPIO{i}", pinType,
                i >= 32 && i <= 39 ? PinType.Analog : PinType.Digital));
        }

        PinDefinitions.Add(new PinDefinition("3V3", "Power", PinType.Power));
        PinDefinitions.Add(new PinDefinition("5V", "Power", PinType.Power));
        PinDefinitions.Add(new PinDefinition("GND", "Ground", PinType.Ground));
        PinDefinitions.Add(new PinDefinition("EN", "Enable", PinType.Digital));
    }

    private void InitializeESP8266Pins()
    {
        PinDefinitions.Add(new PinDefinition("GPIO0", "Boot/Flash", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO1", "TX", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO2", "LED", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO3", "RX", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO4", "SDA", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO5", "SCL", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO12", "MISO", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO13", "MOSI", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO14", "SCK", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO15", "SS", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("GPIO16", "Wake", PinType.Digital));
        PinDefinitions.Add(new PinDefinition("ADC0", "Analog", PinType.Analog));
        PinDefinitions.Add(new PinDefinition("3V3", "Power", PinType.Power));
        PinDefinitions.Add(new PinDefinition("GND", "Ground", PinType.Ground));
        PinDefinitions.Add(new PinDefinition("RST", "Reset", PinType.Digital));
    }

    private void InitializeGenericPins()
    {
        for (int i = 1; i <= DigitalPins; i++)
        {
            PinDefinitions.Add(new PinDefinition($"D{i}", "Digital", PinType.Digital));
        }

        for (int i = 1; i <= AnalogPins; i++)
        {
            PinDefinitions.Add(new PinDefinition($"A{i}", "Analog", PinType.Analog));
        }

        PinDefinitions.Add(new PinDefinition("VCC", "Power", PinType.Power));
        PinDefinitions.Add(new PinDefinition("GND", "Ground", PinType.Ground));
    }
}

public enum MicrocontrollerType
{
    Arduino,
    ESP32,
    ESP8266,
    STM32,
    PIC,
    AVR,
    ARM,
    RaspberryPi
}

public enum PinType
{
    Digital,
    Analog,
    PWM,
    Power,
    Ground,
    Communication
}

public record PinDefinition(string Name, string Function, PinType Type);
