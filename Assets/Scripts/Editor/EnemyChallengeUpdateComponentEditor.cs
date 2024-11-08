using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(EnemyChallengeUpdateComponent))]
public class EnemyChallengeUpdateComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EnemyChallengeUpdateComponent challengeComponent = (EnemyChallengeUpdateComponent)target;

        if (GUILayout.Button("Setup Visibility Notifiers"))
        {
            SetupVisibilityNotifiers(challengeComponent);
        }
    }

    private void SetupVisibilityNotifiers(EnemyChallengeUpdateComponent challengeComponent)
    {
        SpriteRenderer[] renderers = challengeComponent.GetComponentsInChildren<SpriteRenderer>();

        challengeComponent.VisibilityStatuses = new bool[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            var renderer = renderers[i];
            var gameObject = renderer.gameObject;

            var existingNotifier = gameObject.GetComponent<VisibilityNotifier>();
            if (existingNotifier != null)
            {
                DestroyImmediate(existingNotifier);
            }

            var notifier = gameObject.AddComponent<VisibilityNotifier>();
            notifier.ChallengeComponent = challengeComponent;
            notifier.VisibilityIndex = i;
        }

        EditorUtility.SetDirty(challengeComponent);
    }

}
