using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PresetsCreator))]
public class PresetsCreatorEditor : Editor
{
    private void OnSceneGUI()
    {
        PresetsCreator presetsCreator = (PresetsCreator)target;

        // ���������, ���� ������ RewardPositions ����������
        if (presetsCreator != null && presetsCreator._rewardPosition?.RewardPositions != null)
        {
            // �������� �� ���� �������� � �������
            for (int i = 0; i < presetsCreator._rewardPosition.RewardPositions.Length; i++)
            {
                // ����������� ��������� ���������� � �������
                Vector3 worldPosition = presetsCreator.transform.TransformPoint(new Vector3(presetsCreator._rewardPosition.RewardPositions[i].x, presetsCreator._rewardPosition.RewardPositions[i].y, 0));

                // ������� ����������� ��� �������������� (PositionHandle)
                Vector3 newWorldPosition = Handles.PositionHandle(worldPosition, Quaternion.identity);

                // ���� ������� ����������, ��������� ������ � ���������
                if (worldPosition != newWorldPosition)
                {
                    Undo.RecordObject(presetsCreator, "Move Reward Position");
                    Vector3 localPosition = presetsCreator.transform.InverseTransformPoint(newWorldPosition);
                    presetsCreator._rewardPosition.RewardPositions[i] = new Vector2(localPosition.x, localPosition.y);

                    // �������� ���������, ����� Unity ���� � ���
                    EditorUtility.SetDirty(presetsCreator);
                }

                // ������ ����������� ���� �� ����� �������
                Handles.color = Color.green;
                Handles.DrawSolidDisc(newWorldPosition, Vector3.forward, 0.5f);
            }
        }
    }
}

