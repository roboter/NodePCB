using System;

namespace NodeEditor.Model;

public sealed class ConnectorCreatedEventArgs : EventArgs
{
    public ICommonConnector? Connector { get; }

    public ConnectorCreatedEventArgs(ICommonConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorRemovedEventArgs : EventArgs
{
    public ICommonConnector? Connector { get; }

    public ConnectorRemovedEventArgs(ICommonConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorSelectedEventArgs : EventArgs
{
    public ICommonConnector? Connector { get; }

    public ConnectorSelectedEventArgs(ICommonConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorDeselectedEventArgs : EventArgs
{
    public ICommonConnector? Connector { get; }

    public ConnectorDeselectedEventArgs(ICommonConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorStartChangedEventArgs : EventArgs
{
    public ICommonConnector? Connector { get; }

    public ConnectorStartChangedEventArgs(ICommonConnector? connector)
    {
        Connector = connector;
    }
}

public sealed class ConnectorEndChangedEventArgs : EventArgs
{
    public ICommonConnector? Connector { get; }

    public ConnectorEndChangedEventArgs(ICommonConnector? connector)
    {
        Connector = connector;
    }
}

public interface ICommonConnector
{
    string? Name { get; set; }
    IDrawingNode? Parent { get; set; }
    ConnectorOrientation Orientation { get; set; }
    IPin? Start { get; set; }
    IPin? End { get; set; }

    double Offset { get; set; }
    bool IsSelected { get; set; }

    bool CanSelect();

    bool CanRemove();
    event EventHandler<ConnectorCreatedEventArgs>? Created;
    event EventHandler<ConnectorRemovedEventArgs>? Removed;
    event EventHandler<ConnectorSelectedEventArgs>? Selected;
    event EventHandler<ConnectorDeselectedEventArgs>? Deselected;
    event EventHandler<ConnectorStartChangedEventArgs>? StartChanged;
    event EventHandler<ConnectorEndChangedEventArgs>? EndChanged;
    void OnCreated();
    void OnRemoved();
    void OnSelected();
    void OnDeselected();
    void OnStartChanged();
    void OnEndChanged();
}


public interface IBezierConnector : ICommonConnector
{
    IPin? StartControl { get; set; }
    IPin? EndControl { get; set; }
    void ResetControlPoints();
}

