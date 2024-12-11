using UnityEditor;

[CustomEditor(typeof(CustomPressButton))]
public class CustomPressButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        //CustomPressButton button = (CustomPressButton)target;
        DrawDefaultInspector();
    }
}
