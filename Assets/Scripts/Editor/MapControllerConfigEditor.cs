using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapControllerConfig))]
public class MapControllerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapControllerConfig config = (MapControllerConfig)target;

        if (GUILayout.Button("Platform height calculation"))
        {
            CalculatePlatformHeights(config);
        }

        float totalLevelHeight = CalculateTotalLevelHeight(config);
        EditorGUILayout.LabelField("Total height of all levels", totalLevelHeight.ToString());
    }

    private void CalculatePlatformHeights(MapControllerConfig config)
    {
        var levels = config.Levels;
        if (levels == null || levels.Length == 0) return;

        var lastPlatform = levels[levels.Length - 1];
        lastPlatform.CurrentPlatformHeight = 0f;
        lastPlatform.TargetPlatformHeight = -(lastPlatform.LevelHeight + lastPlatform.HeightAfterTheCurrentPlatform);

        for (int i = levels.Length - 2; i >= 0; i--)
        {
            var level = levels[i];

            var nextLevel = config.Levels[i + 1];

            level.TargetPlatformHeight = nextLevel.CurrentPlatformHeight;
            level.CurrentPlatformHeight = level.TargetPlatformHeight + level.LevelHeight + level.HeightAfterTheCurrentPlatform + nextLevel.HeightBeforeTheCurrentPlatform;
        }

        EditorUtility.SetDirty(config);
    }


    private float CalculateTotalLevelHeight(MapControllerConfig config)
    {
        var levels = config.Levels;

        float totalHeight = 0f;

        for (int i = 0; i < levels.Length; i++)
        {
            totalHeight += levels[i].LevelHeight + levels[i].HeightBeforeTheCurrentPlatform + levels[i].HeightAfterTheCurrentPlatform;
        }

        totalHeight -= levels[0].HeightBeforeTheCurrentPlatform;
        totalHeight -= levels[levels.Length - 1].HeightAfterTheCurrentPlatform;

        return totalHeight;
    }

}
