using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using NodeEditorDemo.Services;

namespace NodeEditorDemo.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (Environment.GetEnvironmentVariable("TAKE_SCREENSHOT") == "1")
        {
            Dispatcher.UIThread.Post(async () =>
            {
                await System.Threading.Tasks.Task.Delay(1500);
                try
                {
                    var mainView = this.FindControl<MainView>("MainView");
                    var target = (Control?)mainView ?? this;
                    var w = Bounds.Width > 0 ? Bounds.Width : 1230;
                    var h = Bounds.Height > 0 ? Bounds.Height : 740;
                    var imgDir = Directory.Exists("images") ? Path.GetFullPath("images") : @"e:\GitHub\NodePCB\images";
                    Directory.CreateDirectory(imgDir);
                    var outputPath = Path.Combine(imgDir, "BezierControlPoints.png");
                    using var stream = File.Create(outputPath);
                    var size = new Size(w, h);
                    ExportRenderer.RenderPng(target, size, stream);
                }
                catch (Exception ex)
                {
                    var errPath = Directory.Exists("images") ? Path.GetFullPath("images/screenshot_error.txt") : @"e:\GitHub\NodePCB\images\screenshot_error.txt";
                    File.WriteAllText(errPath, ex.ToString());
                }
                finally
                {
                    Close();
                }
            });
        }
    }
}
