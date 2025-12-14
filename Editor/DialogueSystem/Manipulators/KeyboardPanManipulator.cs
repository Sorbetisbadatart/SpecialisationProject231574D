using DS.Windows;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Allows panning the graph view using keyboard arrow keys
/// </summary>
public class KeyboardPanManipulator : Manipulator
{
    private const float PanSpeed = 50f;
    private bool isPanning;
    Vector2 panDirection = Vector2.zero;

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        target.RegisterCallback<KeyUpEvent>(OnKeyUp);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        target.UnregisterCallback<KeyUpEvent>(OnKeyUp);
    }

    private void OnKeyDown(KeyDownEvent evt)
    {
        if (isPanning) return;

        var graphView = target as DSGraphView;
        if (graphView == null) return;   

        switch (evt.keyCode)
        {
            case KeyCode.UpArrow:
                panDirection.y += PanSpeed;
                break;
            case KeyCode.DownArrow:
                panDirection.y += -PanSpeed;
                break;
            case KeyCode.LeftArrow:
                panDirection.x += PanSpeed;
                break;
            case KeyCode.RightArrow:
                panDirection.x += -PanSpeed;
                break;
            default:
                return; // Not a pan key
        }

        // Start continuous panning
        if (panDirection != Vector2.zero)
        {
            //isPanning = true;
            graphView.schedule.Execute(() => ContinuousPan(graphView, panDirection)).Every(16); // ~60fps
            evt.StopPropagation();
        }
    }

    private void OnKeyUp(KeyUpEvent evt)
    {
        switch (evt.keyCode)
        {
            case KeyCode.UpArrow:
            case KeyCode.DownArrow:
            case KeyCode.LeftArrow:
            case KeyCode.RightArrow:
                isPanning = false;
                evt.StopPropagation();
                break;
        }
    }

    private void ContinuousPan(GraphView graphView, Vector3 direction)
    {
        //if (!isPanning) return;
        graphView.UpdateViewTransform(direction, new Vector3(graphView.scale, graphView.scale, graphView.scale));
    }
}