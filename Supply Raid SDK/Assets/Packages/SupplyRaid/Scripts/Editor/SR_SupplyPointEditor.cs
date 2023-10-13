using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SupplyRaid;

#if UNITY_EDITOR
[CustomEditor(typeof(SR_SupplyPoint))]
public class SR_SupplyPointEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var script = (SR_SupplyPoint)target;


        if (GUILayout.Button("Place Waypoints on Ground (Cannot Undo)", GUILayout.Height(20)))
        {
            script.PlaceAllSosigsOnGround();
        }

    }
    
}
#endif