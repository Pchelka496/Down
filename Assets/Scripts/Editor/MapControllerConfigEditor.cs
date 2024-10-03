using UnityEditor;
using System.Linq;
using UnityEngine;

[CustomEditor(typeof(MapControllerConfig))]
public class MapControllerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapControllerConfig config = (MapControllerConfig)target;

        if (GUILayout.Button("Height edit"))
        {
            HeightEdit(config);
        }

    }

    private void HeightEdit(MapControllerConfig config)
    {
        var savingHeights = config.SavingHeights;
        var sortedHeights = savingHeights.OrderBy(h => h).ToArray();
        config.SavingHeights = sortedHeights;
    }

}
