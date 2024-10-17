using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using static UnityEngine.Rendering.STP;

[CustomEditor(typeof(PresetsCreator))]
public class PresetsCreatorEditor : Editor
{
    private void OnSceneGUI()
    {
        base.OnInspectorGUI();

        PresetsCreator presetsCreator = (PresetsCreator)target;

        if (GUILayout.Button("Add Reward Position to Config"))
        {
            presetsCreator.AddRewardPosition();
            EditorUtility.SetDirty(presetsCreator);
        }

        if (GUILayout.Button("Remove Last Reward Position from Config"))
        {
            presetsCreator.RemoveLastRewardPosition();
            EditorUtility.SetDirty(presetsCreator);
        }

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

