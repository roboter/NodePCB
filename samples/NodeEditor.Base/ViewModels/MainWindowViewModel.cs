﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using NodeEditor.Controls;
using NodeEditor.Export.Renderers;
using NodeEditor.Model;
using NodeEditor.Serializer;
using NodeEditor.ViewModels;
using ReactiveUI;

namespace NodeEditorDemo.ViewModels;

public class MainWindowViewModel : ViewModelBase, INodeTemplatesHost
{
    private readonly INodeSerializer _serializer;
    private readonly NodeFactory _factory;
    private IList<INodeTemplate>? _templates;
    private IDrawingNode? _drawing;
    private bool _isEditMode;
    private bool _isToolboxVisible;

    public MainWindowViewModel()
    {
        _serializer = new NodeSerializer(typeof(ObservableCollection<>));
        _factory = new();

        _templates = _factory.CreateTemplates();

        Drawing = _factory.CreateDemoDrawing();
        Drawing.SetSerializer(_serializer);

        _isEditMode = true;
        _isToolboxVisible = true;

        ToggleEditModeCommand = ReactiveCommand.Create(() =>
        {
            IsEditMode = !IsEditMode;
        });

        ToggleToolboxVisibleCommand = ReactiveCommand.Create(() =>
        {
            IsToolboxVisible = !IsToolboxVisible;
        });

        NewCommand = ReactiveCommand.Create(New);

        OpenCommand = ReactiveCommand.CreateFromTask(async () => await Open());

        SaveCommand = ReactiveCommand.CreateFromTask(async () => await Save());

        ExportCommand = ReactiveCommand.CreateFromTask(async () => await Export());

        ExitCommand = ReactiveCommand.Create(() =>
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Shutdown();
            }
        });

        AboutCommand = ReactiveCommand.Create(() =>
        {
            // TODO: Show about dialog window.
        });
    }

    public IList<INodeTemplate>? Templates
    {
        get => _templates;
        set => this.RaiseAndSetIfChanged(ref _templates, value);
    }

    public IDrawingNode? Drawing
    {
        get => _drawing;
        set => this.RaiseAndSetIfChanged(ref _drawing, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => this.RaiseAndSetIfChanged(ref _isEditMode, value);
    }

    public bool IsToolboxVisible
    {
        get => _isToolboxVisible;
        set => this.RaiseAndSetIfChanged(ref _isToolboxVisible, value);
    }

    public ICommand ToggleEditModeCommand { get; }

    public ICommand ToggleToolboxVisibleCommand { get; }

    public ICommand NewCommand { get; }

    public ICommand OpenCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand ExportCommand { get; }

    public ICommand ExitCommand { get; }

    public ICommand AboutCommand { get; }

    private void New()
    {
        Drawing = _factory.CreateDrawing();
        Drawing.SetSerializer(_serializer);
    }

    private List<FilePickerFileType> GetOpenFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetSaveFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.Json,
            StorageService.All
        };
    }

    private static List<FilePickerFileType> GetExportFileTypes()
    {
        return new List<FilePickerFileType>
        {
            StorageService.ImagePng,
            StorageService.ImageSvg,
            StorageService.Pdf,
            StorageService.Xps,
            StorageService.ImageSkp,
            StorageService.All
        };
    }

    private async Task Open()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var result = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open drawing",
            FileTypeFilter = GetOpenFileTypes(),
            AllowMultiple = false
        });

        var file = result.FirstOrDefault();

        if (file is not null && file.CanOpenRead)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                using var reader = new StreamReader(stream);
                var json = await reader.ReadToEndAsync();
                var drawing = _serializer.Deserialize<DrawingNodeViewModel?>(json);
                if (drawing is { })
                {
                    Drawing = drawing;
                    Drawing.SetSerializer(_serializer);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    private async Task Save()
    {
        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save drawing",
            FileTypeChoices = GetSaveFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension("drawing"),
            DefaultExtension = "json",
            ShowOverwritePrompt = true
        });

        if (file is not null && file.CanOpenWrite)
        {
            try
            {
                var json = _serializer.Serialize(_drawing);
                await using var stream = await file.OpenWriteAsync();
                await using var writer = new StreamWriter(stream);
                await writer.WriteAsync(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    public async Task Export()
    {
        if (Drawing is null)
        {
            return;
        }

        var storageProvider = StorageService.GetStorageProvider();
        if (storageProvider is null)
        {
            return;
        }

        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export drawing",
            FileTypeChoices = GetExportFileTypes(),
            SuggestedFileName = Path.GetFileNameWithoutExtension("drawing"),
            DefaultExtension = "png",
            ShowOverwritePrompt = true
        });

        if (file is not null && file.CanOpenWrite)
        {
            try
            {
                var control = new DrawingNode
                {
                    DataContext = Drawing
                };

                var preview = new Window
                {
                    Width = Drawing.Width,
                    Height = Drawing.Height,
                    Content = control,
                    ShowInTaskbar = false,
                    WindowState = WindowState.Minimized
                };

                preview.Show();

                var size = new Size(Drawing.Width, Drawing.Height);

                if (file.Name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = await file.OpenWriteAsync();
                    PngRenderer.Render(preview, size, stream);
                }

                if (file.Name.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = await file.OpenWriteAsync();
                    SvgRenderer.Render(preview, size, stream);
                }

                if (file.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = await file.OpenWriteAsync();
                    PdfRenderer.Render(preview, size, stream, 96);
                }

                if (file.Name.EndsWith("xps", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = await file.OpenWriteAsync();
                    XpsRenderer.Render(control, size, stream, 96);
                }

                if (file.Name.EndsWith("skp", StringComparison.OrdinalIgnoreCase))
                {
                    await using var stream = await file.OpenWriteAsync();
                    SkpRenderer.Render(control, size, stream);
                }

                preview.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    public void PrintNetList(IDrawingNode? drawing)
    {
        if (drawing?.Connectors is null || drawing.Nodes is null)
        {
            return;
        }

        foreach (var connector in drawing.Connectors)
        {
            if (connector.Start is { } start && connector.End is { } end)
            {
                Debug.WriteLine($"{start.Parent?.Name}:{start.Name} -> {end.Parent?.Name}:{end.Name}");
            }
        }
    }
}
