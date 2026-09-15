using System;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using NodeEditor.Model;
using ReactiveMarbles.PropertyChanged;

namespace NodeEditor.Mvvm;

[ObservableObject]
public partial class BezierConnectorViewModel : IBezierConnector
{
    [ObservableProperty] private string? _name;
    [ObservableProperty] private IDrawingNode? _parent;
    [ObservableProperty] private ConnectorOrientation _orientation;
    [ObservableProperty] private IPin? _start;
    [ObservableProperty] private IPin? _end;
    [ObservableProperty] private double _offset = 50;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private IPin? _startControl;
    [ObservableProperty] private IPin? _endControl;


    partial void OnOffsetChanged(double value)
    {
        ResetControlPoints();
    }

    partial void OnOrientationChanged(ConnectorOrientation value)
    {
        ResetControlPoints();
    }

    partial void OnStartControlChanged(IPin? value)
    {
        if (value is System.ComponentModel.INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += StartControl_PropertyChanged;
        }
        if (value is { })
        {
            value.Moved += StartControl_Moved;
        }
    }

    private void StartControl_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IPin.X) or nameof(IPin.Y))
        {
            OnPropertyChanged(nameof(StartControl));
        }
    }

    private void StartControl_Moved(object? sender, PinMovedEventArgs e)
    {
        OnPropertyChanged(nameof(StartControl));
    }

    partial void OnEndControlChanged(IPin? value)
    {
        if (value is System.ComponentModel.INotifyPropertyChanged npc)
        {
            npc.PropertyChanged += EndControl_PropertyChanged;
        }
        if (value is { })
        {
            value.Moved += EndControl_Moved;
        }
    }

    private void EndControl_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(IPin.X) or nameof(IPin.Y))
        {
            OnPropertyChanged(nameof(EndControl));
        }
    }

    private void EndControl_Moved(object? sender, PinMovedEventArgs e)
    {
        OnPropertyChanged(nameof(EndControl));
    }

    public void NotifyStartControlChanged() => OnPropertyChanged(nameof(StartControl));
    public void NotifyEndControlChanged() => OnPropertyChanged(nameof(EndControl));

    public event EventHandler<ConnectorCreatedEventArgs>? Created;
    public event EventHandler<ConnectorRemovedEventArgs>? Removed;
    public event EventHandler<ConnectorSelectedEventArgs>? Selected;
    public event EventHandler<ConnectorDeselectedEventArgs>? Deselected;
    public event EventHandler<ConnectorStartChangedEventArgs>? StartChanged;
    public event EventHandler<ConnectorEndChangedEventArgs>? EndChanged;

    public BezierConnectorViewModel()
    {
        ObservePins();
        EnsureControlPoints();
    }

    private void ObservePins()
    {
        this.WhenChanged(x => x.Start)
            .DistinctUntilChanged()
            .Subscribe(start =>
            {
                if (start?.Parent is { })
                {
                    (start.Parent as NodeViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                    (start.Parent as NodeViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                }
                else
                {
                    if (start is { })
                    {
                        (start as PinViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                        (start as PinViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                    }
                }

                if (start is { })
                {
                    (start as PinViewModel)?.WhenChanged(x => x.Alignment).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(Start)));
                }

                EnsureControlPoints();
            });

        this.WhenChanged(x => x.End)
            .DistinctUntilChanged()
            .Subscribe(end =>
            {
                if (end?.Parent is { })
                {
                    (end.Parent as NodeViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                    (end.Parent as NodeViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                }
                else
                {
                    if (end is { })
                    {
                        (end as PinViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                        (end as PinViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                    }
                }

                if (end is { })
                {
                    (end as PinViewModel)?.WhenChanged(x => x.Alignment).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(End)));
                }

                EnsureControlPoints();
            });

        this.WhenChanged(x => x.StartControl)
            .DistinctUntilChanged()
            .Subscribe(sc =>
            {
                if (sc?.Parent is { })
                {
                    (sc.Parent as NodeViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(StartControl)));
                    (sc.Parent as NodeViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(StartControl)));
                }
                if (sc is { })
                {
                    (sc as PinViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(StartControl)));
                    (sc as PinViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(StartControl)));
                }
            });

        this.WhenChanged(x => x.EndControl)
            .DistinctUntilChanged()
            .Subscribe(ec =>
            {
                if (ec?.Parent is { })
                {
                    (ec.Parent as NodeViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(EndControl)));
                    (ec.Parent as NodeViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(EndControl)));
                }
                if (ec is { })
                {
                    (ec as PinViewModel)?.WhenChanged(x => x.X).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(EndControl)));
                    (ec as PinViewModel)?.WhenChanged(x => x.Y).DistinctUntilChanged().Subscribe(_ => OnPropertyChanged(nameof(EndControl)));
                }
            });
    }

    private void EnsureControlPoints()
    {
        if (Start is null || End is null)
        {
            return;
        }

        if (StartControl is null || EndControl is null)
        {
            ResetControlPoints();
        }
    }

    public void ResetControlPoints()
    {
        if (Start is null || End is null)
        {
            return;
        }

        var p0X = Start.X;
        var p0Y = Start.Y;
        if (Start.Parent is { })
        {
            p0X += Start.Parent.X;
            p0Y += Start.Parent.Y;
        }

        var p3X = End.X;
        var p3Y = End.Y;
        if (End.Parent is { })
        {
            p3X += End.Parent.X;
            p3Y += End.Parent.Y;
        }

        var p1X = p0X;
        var p1Y = p0Y;
        var p2X = p3X;
        var p2Y = p3Y;

        this.GetControlPoints(
            Orientation,
            Offset,
            Start.Alignment,
            End.Alignment,
            ref p1X, ref p1Y,
            ref p2X, ref p2Y);

        if (StartControl is null)
        {
            StartControl = new PinViewModel
            {
                Name = "StartControl",
                Parent = Start.Parent,
                X = Start.Parent is { } sp ? p1X - sp.X : p1X,
                Y = Start.Parent is { } spy ? p1Y - spy.Y : p1Y,
                Width = 10,
                Height = 10,
                Alignment = PinAlignment.None
            };
        }
        else
        {
            StartControl.Parent = Start.Parent;
            StartControl.X = Start.Parent is { } sp ? p1X - sp.X : p1X;
            StartControl.Y = Start.Parent is { } spy ? p1Y - spy.Y : p1Y;
            StartControl.OnMoved();
            OnPropertyChanged(nameof(StartControl));
        }

        if (EndControl is null)
        {
            EndControl = new PinViewModel
            {
                Name = "EndControl",
                Parent = End.Parent,
                X = End.Parent is { } ep ? p2X - ep.X : p2X,
                Y = End.Parent is { } epy ? p2Y - epy.Y : p2Y,
                Width = 10,
                Height = 10,
                Alignment = PinAlignment.None
            };
        }
        else
        {
            EndControl.Parent = End.Parent;
            EndControl.X = End.Parent is { } ep ? p2X - ep.X : p2X;
            EndControl.Y = End.Parent is { } epy ? p2Y - epy.Y : p2Y;
            EndControl.OnMoved();
            OnPropertyChanged(nameof(EndControl));
        }
    }

    public virtual bool CanSelect() => true;
    public virtual bool CanRemove() => true;

    public void OnCreated() => Created?.Invoke(this, new ConnectorCreatedEventArgs(this));
    public void OnRemoved() => Removed?.Invoke(this, new ConnectorRemovedEventArgs(this));
    public void OnSelected()
    {
        IsSelected = true;
        Selected?.Invoke(this, new ConnectorSelectedEventArgs(this));
    }

    public void OnDeselected()
    {
        IsSelected = false;
        Deselected?.Invoke(this, new ConnectorDeselectedEventArgs(this));
    }

    public void OnStartChanged() => StartChanged?.Invoke(this, new ConnectorStartChangedEventArgs(this));
    public void OnEndChanged() => EndChanged?.Invoke(this, new ConnectorEndChangedEventArgs(this));
}

