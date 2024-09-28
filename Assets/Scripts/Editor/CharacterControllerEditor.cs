using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterController))]
public class CharacterControllerEditor : Editor
{
    private void OnSceneGUI()
    {
        CharacterController characterController = (CharacterController)target;

        if (characterController != null && characterController.GroundCheck != null)
        {
            Handles.color = Color.red;

            Vector3 groundCheckPosition = characterController.GroundCheck.position;

            Vector3 rayEndPoint = groundCheckPosition + Vector3.down * characterController.GroundCheckDistance;

            Handles.DrawLine(groundCheckPosition, rayEndPoint);

            Handles.DrawWireDisc(rayEndPoint, Vector3.forward, 0.05f);
        }
    }

}
