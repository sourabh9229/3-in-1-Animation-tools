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
                        motionPath.pointColor = EditorGUILayout.ColorField("Point Color", motionPath.pointColor);

                        motionPath.pointSize = EditorGUILayout.FloatField("Points Size", motionPath.pointSize);

                        motionPath.showPoints = EditorGUILayout.Toggle("Show Points", motionPath.showPoints);
                        motionPath.ShowLines = EditorGUILayout.Toggle("Show Lines", motionPath.ShowLines);
                        motionPath.ShowCirles = EditorGUILayout.Toggle("Show Path Points", motionPath.ShowCirles);

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
            motionPath.DrawLines();
            motionPath.DrawPoints();
            motionPath.DrawRailsPoints(motionPath.valueEachTime);
        }

        public void OnEnable()
        {
            motionPath = (MotionPath)target;

            motionPath.pathTarget = motionPath.gameObject.transform;

            motionPath.deselected = false;
        }

        public void OnDisable()
        {
            motionPath.deselected = true;
        }

        public void DrawLines()
        {

            if (motionPath.pathTarget != null && motionPath.clip != null)
            {
                if (motionPath.valueEachTime == null || motionPath.valueEachTime.Count < 2)
                {
                    return;
                }

                if (motionPath.ShowLines)
                {
                    Handles.color = motionPath.pathColor;

                    // Draw the path by connecting the points
                    for (int i = 0; i < motionPath.valueEachTime.Count - 1; i++)
                    {
                        Handles.DrawLine(motionPath.valueEachTime[i], motionPath.valueEachTime[i + 1]);
                    }
                }

            }
        }

        public void DrawPoints()
        {
            if (motionPath.pathTarget != null && motionPath.clip != null)
            {
                if (motionPath.points == null)
                    return;


                if (motionPath.showPoints)
                {
                    if (motionPath.points.Count > 0)
                    {
                        Handles.color = motionPath.pointColor;
                        for (int i = 0; i < motionPath.points.Count; i++)
                        {
                            float handleSize = (SceneView.currentDrawingSceneView.in2DMode) ? HandleUtility.GetHandleSize(Vector3.zero) * motionPath.pointSize : motionPath.pointSize;

                            // Handles.DotHandleCap(i, points[i], Quaternion.LookRotation(Vector3.up), handleSize, EventType.Repaint);
                            Vector3 newPointPos = Handles.FreeMoveHandle(motionPath.points[i], handleSize, Vector3.zero, Handles.CubeHandleCap);

                        }

                    }
                }
            }

        }
    }
}