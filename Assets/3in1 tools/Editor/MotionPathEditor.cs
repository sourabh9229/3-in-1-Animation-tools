#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SVassets.AnimationCrafter.EditorTools
{
    [CustomEditor(typeof(MotionPath))]
    public class MotionPathEditor : Editor
    {
        MotionPath motionPath;

        bool showDisplayOption = false;

        public override void OnInspectorGUI()
        {
            motionPath = (MotionPath)target;

            motionPath.clip = (AnimationClip)EditorGUILayout.ObjectField(new GUIContent("Animation Clip", "Put associated Animation clip here."), motionPath.clip, typeof(AnimationClip), true);

            if (motionPath.clip != null)
            {
                motionPath.alwaysShow = EditorGUILayout.Toggle(new GUIContent("Always Show", "If enabled, path will not disappear on deselecting Game Object."), motionPath.alwaysShow);
                motionPath.AutoUpdatePath = EditorGUILayout.Toggle(new GUIContent("Auto Update Path", "If enabled, path will continuously update from animation clip."), motionPath.AutoUpdatePath);

                if (GUILayout.Button("Update Keys"))
                {
                    motionPath.UpdateMotionPath();
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Reset Path"))
                {
                    motionPath.ResetMotionPath();
                    SceneView.RepaintAll();
                }

                showDisplayOption = EditorGUILayout.Foldout(showDisplayOption, "Display Options");
                if (showDisplayOption)
                {
                    if (motionPath.clip != null)
                    {
                        motionPath.pathColor = EditorGUILayout.ColorField("Path Color", motionPath.pathColor);
                        motionPath.KeysColor = EditorGUILayout.ColorField("Point Color", motionPath.KeysColor);

                        motionPath.pointSize = EditorGUILayout.FloatField("KeyPoint Size", motionPath.pointSize);

                        motionPath.ShowPath = EditorGUILayout.Toggle("Show Path", motionPath.ShowPath);

                        motionPath.showKeyPoints = EditorGUILayout.Toggle("Show Keyframes", motionPath.showKeyPoints);
                        motionPath.ShowFramePoints = EditorGUILayout.Toggle("Frame Points", motionPath.ShowFramePoints);

                    }
                }

                Repaint();
                SceneView.RepaintAll();

            }


            if (motionPath.clip == null)
            {
                EditorGUILayout.LabelField("No Active Animation Clip.");
            }
        }

        private void OnSceneGUI()
        {
            motionPath = (MotionPath)target;

            if (motionPath.AutoUpdatePath)
            {
                motionPath.UpdateMotionPath();
            }
            motionPath.DrawPath();
            motionPath.DrawKeyframePoints();
            motionPath.DrawFramePoints();
        }

        public void OnEnable()
        {
            motionPath = (MotionPath)target;

            motionPath.deselected = false;
        }

        public void OnDisable()
        {
            motionPath.deselected = true;
        }
    }
}
#endif