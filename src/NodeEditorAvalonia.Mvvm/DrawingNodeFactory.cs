using System.Collections.Generic;
using System.Collections.ObjectModel;
using NodeEditor.Model;

namespace NodeEditor.Mvvm;

public class DrawingNodeFactory : IDrawingNodeFactory
{
    public static readonly IDrawingNodeFactory Instance = new DrawingNodeFactory();

    public IPin CreatePin() => new PinViewModel();

    public ICommonConnector CreateConnector() => new BezierConnectorViewModel();

    public IBezierConnector CreateBezierConnector() => new BezierConnectorViewModel();

    public IList<T> CreateList<T>() => new ObservableCollection<T>();

}
