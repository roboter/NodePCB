﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using NodeEditor.Model;

namespace NodeEditor.Behaviors
{
    public class DrawingMovedBehavior : Behavior<Control>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject is { })
            {
                AssociatedObject.AddHandler(InputElement.PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (AssociatedObject is { })
            {
                AssociatedObject.RemoveHandler(InputElement.PointerMovedEvent, Moved);
            }
        }

        private void Moved(object? sender, PointerEventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (AssociatedObject.DataContext is not IDrawingNode drawingNode)
            {
                return;
            }

            var (x, y) = e.GetPosition(AssociatedObject);

            drawingNode.ConnectorMove(x, y);
        }
    }
}
