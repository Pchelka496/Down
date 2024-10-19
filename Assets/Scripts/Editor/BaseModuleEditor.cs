using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseModule), true)]
public class BaseModuleEditor : Editor
{
    private void OnSceneGUI()
    {
        // Указываем цвет круга
        Handles.color = Color.green;

        // Рисуем незаполненный круг в точке (0, 0) с радиусом 1
        Handles.DrawWireDisc(Vector3.zero, Vector3.forward, CharacterController.PLAYER_RADIUS);

        // Обновляем сцену
        SceneView.RepaintAll();
    }

}
