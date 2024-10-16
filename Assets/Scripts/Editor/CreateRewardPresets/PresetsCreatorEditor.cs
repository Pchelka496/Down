using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PresetsCreator))]
public class PresetsCreatorEditor : Editor
{
    private void OnSceneGUI()
    {
        PresetsCreator presetsCreator = (PresetsCreator)target;

        // Проверяем, если массив RewardPositions существует
        if (presetsCreator != null && presetsCreator._rewardPosition?.RewardPositions != null)
        {
            // Проходим по всем позициям в массиве
            for (int i = 0; i < presetsCreator._rewardPosition.RewardPositions.Length; i++)
            {
                // Преобразуем локальные координаты в мировые
                Vector3 worldPosition = presetsCreator.transform.TransformPoint(new Vector3(presetsCreator._rewardPosition.RewardPositions[i].x, presetsCreator._rewardPosition.RewardPositions[i].y, 0));

                // Создаем манипулятор для перетаскивания (PositionHandle)
                Vector3 newWorldPosition = Handles.PositionHandle(worldPosition, Quaternion.identity);

                // Если позиция изменилась, обновляем массив с векторами
                if (worldPosition != newWorldPosition)
                {
                    Undo.RecordObject(presetsCreator, "Move Reward Position");
                    Vector3 localPosition = presetsCreator.transform.InverseTransformPoint(newWorldPosition);
                    presetsCreator._rewardPosition.RewardPositions[i] = new Vector2(localPosition.x, localPosition.y);

                    // Отмечаем изменения, чтобы Unity знал о них
                    EditorUtility.SetDirty(presetsCreator);
                }

                // Рисуем заполненный круг на новой позиции
                Handles.color = Color.green;
                Handles.DrawSolidDisc(newWorldPosition, Vector3.forward, 0.5f);
            }
        }
    }
}

