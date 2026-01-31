# NodePCB

## About

Simple idea to edit PCBs with curves  

![image](/images/CurvePCB.gif)

## Requirements

* **.NET SDK**: 10.0.102 or later
* **Target Frameworks**: 
  - `netstandard2.0` and `net6.0` for core libraries
  - `net8.0` for sample applications

## Project Structure

```
NodePCB/
├── src/                          # Source libraries
│   ├── NodeEditorAvalonia/      # Main Avalonia controls package
│   ├── NodeEditorAvalonia.Model/ # Base interfaces and models
│   └── NodeEditorAvalonia.Mvvm/  # MVVM view models implementation
├── samples/                      # Sample applications
│   ├── NodeEditor.Base/         # Base sample application
│   ├── NodeEditor.Desktop/      # Desktop sample application
│   └── NodeEditor.Web/          # Web/WASM sample application
├── tests/                        # Unit tests
│   └── NodeEditorAvalonia.UnitTests/
├── build/                        # Build scripts and props files
└── images/                       # Project images and assets
```

## Building NodePCB

First, clone the repository or download the latest zip.
```
git clone https://github.com/roboter/NodePCB.git
```

### Build on Windows using script

* [.NET SDK 10.0](https://www.microsoft.com/net/download?initial-os=windows).

Open up a command-prompt and execute the commands:
```
.\build.ps1
```

### Build on Linux using script

* [.NET SDK 10.0](https://www.microsoft.com/net/download?initial-os=linux).

Open up a terminal prompt and execute the commands:
```
./build.sh
```

### Build on OSX using script

* [.NET SDK 10.0](https://www.microsoft.com/net/download?initial-os=macos).

Open up a terminal prompt and execute the commands:
```
./build.sh
```

```sh
cd NodePCB && dotnet run --project samples/NodeEditor.Desktop/NodeEditor.Desktop.csproj --no-build
```

## Web

```
git clone https://github.com/roboter/NodePCB.git
dotnet workload install wasm-tools
dotnet run --project ./samples/NodeEditor.Web/NodeEditor.Web.csproj -c Release
```

## NuGet

NodePCB is delivered as a NuGet package.

You can find the packages here [NuGet](https://www.nuget.org/packages/NodeEditorAvalonia/) and install the package like this:

`Install-Package NodeEditorAvalonia`

## Available Packages

* [NodeEditorAvalonia](https://www.nuget.org/packages/NodeEditorAvalonia) - The main package with Avalonia controls and default theme.
* [NodeEditorAvalonia.Model](https://www.nuget.org/packages/NodeEditorAvalonia.Model) - The base interfaces used in controls and view models.
* [NodeEditorAvalonia.MVVM](https://www.nuget.org/packages/NodeEditorAvalonia.MVVM) - The MVVM view models with default implementation.

### Package Sources

* https://api.nuget.org/v3/index.json
* https://www.myget.org/F/avalonia-ci/api/v2

## Resources

* [GitHub source code repository.](https://github.com/roboter/NodePCB)

## License

NodePCB is licensed under the [MIT license](LICENSE.TXT).
