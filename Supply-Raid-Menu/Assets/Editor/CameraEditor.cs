using UnityEditor;
using UnityEngine;

/// <summary>
/// Allows to change rotation axis of the scene view to Z.
/// Based on unity implementation <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/SceneView/SceneViewMotion.cs"/>
/// </summary>
[InitializeOnLoad]
public static class SceneViewMotion2D
{
    // there is the main difference from unity implementation.
    // rotation around Y axis applies with Vector3.right and Vector3.up in next 2 lines respectively
    private static readonly Vector3 pitchDir = Vector3.right;
    private static readonly Vector3 yawDir = Vector3.back;

    private static bool isOrbit;
    private static bool useZAxis = true;

    static SceneViewMotion2D()
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
                    cam.pivot += cam.camera.transform.forward * Time.deltaTime;
                    break;

                case KeyCode.K:
                    cam.pivot -= cam.camera.transform.forward * Time.deltaTime;
                    break;

                case KeyCode.J:
                    cam.pivot -= cam.camera.transform.right * Time.deltaTime;
                    break;

                case KeyCode.L:
                    cam.pivot += cam.camera.transform.right * Time.deltaTime;
                    break;
                case KeyCode.None:
                default:
                    break;
            }
        }

    }
}

