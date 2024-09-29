using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapControllerConfig))]
public class MapControllerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapControllerConfig config = (MapControllerConfig)target;

        if (GUILayout.Button("All platform height calculation"))
        {
            CalculatePlatformHeights(config);
        }
        if (GUILayout.Button("Calculate all map position"))
        {
            CalculateMapPosition(config);
        }

        DoorPositionSetting(config);

        float totalLevelHeight = CalculateTotalLevelHeight(config);
        EditorGUILayout.LabelField("Total height of all levels", totalLevelHeight.ToString());
    }

    private void DoorPositionSetting(MapControllerConfig config)
    {
        if (config.Levels == null || config.Levels.Length < 2)
        {
            Debug.LogWarning("Недостаточно уровней для настройки дверей.");
            return;
        }

        for (int i = 0; i < config.Levels.Length - 1; i++)
        {
            var currentLevel = config.Levels[i];
            var nextLevel = config.Levels[i + 1];

            if (nextLevel.CurrentPlatformDoorsPositions != null)
            {
                currentLevel.TargetPlatformDoorsPositions = (float[])nextLevel.CurrentPlatformDoorsPositions.Clone();
            }
            else
            {
                Debug.LogWarning($"Нет дверей у текущей платформы уровня {i + 1}");
            }
        }
    }

    private void CalculateMapPosition(MapControllerConfig config)
    {
        var levels = config.Levels;
        if (levels == null || levels.Length == 0) return;

        foreach (var level in levels)
        {
            float averageHeight = Mathf.Lerp(level.CurrentPlatformHeight, level.TargetPlatformHeight, 0.5f);

            level.MapGlobalPosition = new Vector2(0f, averageHeight);
        }
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
