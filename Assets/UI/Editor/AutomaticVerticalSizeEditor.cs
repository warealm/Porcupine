using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutomaticVerticalSize))]
public class AutomaticVerticalSizeEditor : Editor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Recalc Size"))
        {
            ((AutomaticVerticalSize)target).AdjustSize();
        }

        //base.OnInspectorGUI();
    }


}
