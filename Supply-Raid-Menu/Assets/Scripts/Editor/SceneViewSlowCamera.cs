using UnityEditor;
using UnityEngine;

/// <summary>
/// Allows to change rotation axis of the scene view to Z.
/// Based on unity implementation <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/SceneView/SceneViewMotion.cs"/>
/// </summary>
[InitializeOnLoad]
public static class SceneViewSlowCamera
{
    private static float moveSpeed = 1;

    static SceneViewSlowCamera()
    {
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown)
        {
            var cam = SceneView.lastActiveSceneView;

            switch (e.keyCode)
            {
                case KeyCode.I:
                    cam.pivot += cam.camera.transform.forward * Time.deltaTime * moveSpeed;
                    break;

                case KeyCode.K:
                    cam.pivot -= cam.camera.transform.forward * Time.deltaTime * moveSpeed;
                    break;

                case KeyCode.J:
                    cam.pivot -= cam.camera.transform.right * Time.deltaTime * moveSpeed;
                    break;

                case KeyCode.L:
                    cam.pivot += cam.camera.transform.right * Time.deltaTime * moveSpeed;
                    break;
                case KeyCode.None:
                default:
                    break;
            }
        }

    }
}

