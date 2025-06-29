using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationPathRebinder : EditorWindow
{
    AnimationClip clip;
    GameObject oldRoot;
    GameObject newRoot;

    [MenuItem("Tools/Rebind Animation Clip Paths")]
    public static void ShowWindow()
    {
        GetWindow<AnimationPathRebinder>("Rebind Animation Paths");
    }

    void OnGUI()
    {
        clip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", clip, typeof(AnimationClip), false);
        oldRoot = (GameObject)EditorGUILayout.ObjectField("Old Root", oldRoot, typeof(GameObject), true);
        newRoot = (GameObject)EditorGUILayout.ObjectField("New Root", newRoot, typeof(GameObject), true);

        GUI.enabled = clip != null && oldRoot != null && newRoot != null;
        if (GUILayout.Button("Rebind Paths"))
        {
            RebindPaths();
        }
        GUI.enabled = true;
    }

    void RebindPaths()
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);
        var newBindings = new List<EditorCurveBinding>();
        var newCurves = new List<AnimationCurve>();

        foreach (var binding in bindings)
        {
            string oldPath = binding.path;
            Transform oldTarget = oldRoot.transform.Find(oldPath);
            if (oldTarget == null)
            {
                Debug.LogWarning($"Could not find old path: {oldPath}");
                continue;
            }

            string newPath = AnimationUtility.CalculateTransformPath(oldTarget, newRoot.transform);

            var newBinding = binding;
            newBinding.path = newPath;

            var curve = AnimationUtility.GetEditorCurve(clip, binding);

            newBindings.Add(newBinding);
            newCurves.Add(curve);
        }

        for (int i = 0; i < newBindings.Count; i++)
        {
            AnimationUtility.SetEditorCurve(clip, newBindings[i], newCurves[i]);
        }

        Debug.Log("✅ Rebinding complete.");
    }
}