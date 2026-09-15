using System.Collections.Generic;

namespace NodeEditor.Model;

public interface IDrawingNodeFactory
{
    IPin CreatePin();
    ICommonConnector CreateConnector();
    IBezierConnector CreateBezierConnector();
    public IList<T> CreateList<T>();
}

