using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelManagerConfig))]
public class LevelManagerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LevelManagerConfig config = (LevelManagerConfig)target;

        if (GUILayout.Button("Reset Saved Height"))
        {
            config.PlayerSavedHeight = LevelManagerConfig.NO_SAVED_HEIGHT;
            EditorUtility.SetDirty(config);
        }
    }

}
