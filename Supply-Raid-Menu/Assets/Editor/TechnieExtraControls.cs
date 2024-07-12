using Technie.PhysicsCreator;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Allows to change rotation axis of the scene view to Z.
/// Based on unity implementation <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/SceneView/SceneViewMotion.cs"/>
/// </summary>
[InitializeOnLoad]
public static class TechnieExtraControls
{
    static TechnieExtraControls()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            var cam = SceneView.lastActiveSceneView;

            ///change KeyCode."whatever" to change the bind for each function
            switch (e.keyCode)
            {
                ///Add new hull
                case KeyCode.O:
                    HullPainterWindow.instance.AddHull();
                    HullPainterWindow.instance.Repaint();
                    break;
                ///Toggle hide all
                case KeyCode.H:
                    bool allHullsVisible = HullPainterWindow.instance.AreAllHullsVisible();
                    if (allHullsVisible)
                        HullPainterWindow.instance.SetAllHullsVisible(false); // Hide all
                    else
                        HullPainterWindow.instance.SetAllHullsVisible(true); // Show all
                    HullPainterWindow.instance.Repaint();
                    break;
                ///reset switch
                case KeyCode.None:
                default:
                    break;
            }
        }

    }
}