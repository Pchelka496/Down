using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseModule), true)]
public class BaseModuleEditor : Editor
{
    private void OnSceneGUI()
    {
        // ��������� ���� �����
        Handles.color = Color.green;

        // ������ ������������� ���� � ����� (0, 0) � �������� 1
        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, CharacterController.PLAYER_RADIUS);

        // ��������� �����
        SceneView.RepaintAll();
    }

}
