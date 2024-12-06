using UnityEditor;
using System.Linq;
using ScriptableObject.Map;
using UnityEngine;

[CustomEditor(typeof(MapControllerConfig))]
public class MapControllerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapControllerConfig config = (MapControllerConfig)target;

    }

}
