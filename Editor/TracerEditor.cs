using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SVassets.AnimationCrafter.EditorTools
{
    [CustomEditor(typeof(Tracer))]
    public class TracerEditor : Editor
    {
        Tracer tracer;

        bool showPathOptions;

        public override void OnInspectorGUI()
        {
            tracer = (Tracer)target;

            FirstSetup();

            GUILayout.BeginHorizontal();
            tracer.MoveAllPoints = EditorGUILayout.Toggle(new GUIContent("Hold all Points", "When enabled, all point will be hooked to target or Game Object."), tracer.MoveAllPoints);
            tracer.alwaysShow = EditorGUILayout.Toggle(new GUIContent("Always Show Points", "When enabled, Path will not disappear even when Game Object is deselected."), tracer.alwaysShow);
            GUILayout.EndHorizontal();

            tracer.autoDropAnchor = EditorGUILayout.Toggle(new GUIContent("Auto Drop Anchor", "When enabled, next point will be added over a certain distance. Better for use in Play mode"), tracer.autoDropAnchor);
            if (tracer.autoDropAnchor)
            {
                tracer.dropDistThreshold = EditorGUILayout.FloatField(new GUIContent("Max Distance", "Over this distance from last point, a new anchor will be dropped."), tracer.dropDistThreshold);
            }

            tracer.autoDelete = EditorGUILayout.Toggle(new GUIContent("Auto Delete Last", "When enabled, last point will be automatically deleted over certain count."), tracer.autoDelete);
            if (tracer.autoDelete)
            {
                tracer.maxPointCount = EditorGUILayout.IntField(new GUIContent("Max Points", "Over this points count, last point will delete"), tracer.maxPointCount);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Drop Anchor"))
            {
                SetPoint();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset Path"))
            {
                ResetTracer();
                SceneView.RepaintAll();
            }

            // Create the foldout for path editing options
            showPathOptions = EditorGUILayout.Foldout(showPathOptions, "Display Options");

            if (showPathOptions)
            {
                tracer.pathColor = EditorGUILayout.ColorField("Path Color", tracer.pathColor);
                tracer.pointColor = EditorGUILayout.ColorField("Point Color", tracer.pointColor);

                tracer.pointSize = EditorGUILayout.FloatField("Points Size", tracer.pointSize);

                tracer.LineVisible = EditorGUILayout.Toggle("Show Lines", tracer.LineVisible);
                SceneView.RepaintAll();

                tracer.PointsVisible = EditorGUILayout.Toggle("Show Points", tracer.PointsVisible);
                SceneView.RepaintAll();

                tracer.fadingTrail = EditorGUILayout.Toggle("Fading Trail", tracer.fadingTrail);
                SceneView.RepaintAll();

            }

        }

        public void AutoDropNewAnchor()
        {
            if (tracer.autoDropAnchor && tracer.points.Count == 1)
                SetPoint();

            if (tracer.autoDropAnchor && tracer.points.Count > 1)
            {
                float dist = Vector3.Distance(tracer.traceTarget.position, tracer.points[^2]);

                if (dist > tracer.dropDistThreshold)
                {
                    SetPoint();  // Drop a new anchor at the current GameObject position
                }
            }
        }

        public void AutoDeleteLast()
        {
            if (tracer.autoDelete && tracer.points.Count > 1)
            {
                if (tracer.points.Count > tracer.maxPointCount)
                {
                    tracer.points.RemoveAt(0);
                }
            }
        }

        public void FirstSetup()
        {

            if (tracer.points.Count < 1)
            {
                SetPoint();
            }
        }

        public void ResetTracer()
        {
            // check if, is there points
            if (tracer.points.Count > 1)
            {
                // Clear the points lists 
                tracer.points.Clear();

            }
        }

        public void SetPoint()
        {
            if (tracer.points.Count > 1)
            {
                // Insert the new point just before the last point, which follows the GameObject
                tracer.points.Insert(tracer.points.Count - 1, tracer.traceTarget.position);
            }
            else
            {
                // If there are no points or only one, just add it to the list
                tracer.points.Add(tracer.traceTarget.position);
            }

        }

        public void MovePoint(int index, Vector3 newpos)
        {
            tracer.points[index] = newpos;
        }

        public void OnEnable()
        {
            tracer = (Tracer)target;
            tracer.deselected = false;

            tracer.traceTarget = tracer.gameObject.transform;
        }
        public void OnDisable()
        {
            tracer = (Tracer)target;
            tracer.deselected = true;
        }

        private void OnSceneGUI()
        {
            tracer = (Tracer)target;

            UpdatePath();
            DrawLines(tracer.points);
            DrawPoints(tracer.points);

        }

        public void UpdatePath()
        {
            tracer.traceTarget = tracer.gameObject.transform;

            // moving all points if case
            if (tracer.MoveAllPoints)
            {
                tracer.pointParent = tracer.traceTarget;
            }
            else
            {
                tracer.pointParent = null;
            }

            // move last point to target
            if (tracer.traceTarget != null)
            {
                if (tracer.points.Count > 0)
                {
                    tracer.points[tracer.points.Count - 1] = tracer.traceTarget.position;
                }

            }

            // holding all point
            if (tracer.pointParent != null && tracer.MoveAllPoints)
            {
                if (tracer.parentPos == Vector3.zero)
                {
                    tracer.parentPos = tracer.pointParent.position;
                }

                if (tracer.pointParent.transform.position != tracer.parentPos)
                {
                    Vector3 deltaMove = tracer.pointParent.transform.position - tracer.parentPos;
                    for (int i = 0; i < tracer.points.Count; i++)
                    {
                        tracer.points[i] += deltaMove;
                    }
                }
                tracer.parentPos = tracer.pointParent.transform.position;
            }
            else if (tracer.pointParent == null || !tracer.MoveAllPoints)
            {
                tracer.parentPos = Vector3.zero;
            }

            // special cases
            if (tracer.autoDropAnchor)
            {
                AutoDropNewAnchor();
            }
            if (tracer.autoDelete)
            {
                AutoDeleteLast();
            }

        }


        public void DrawLines(List<Vector3> MyPoints)
        {
            if (MyPoints == null || MyPoints.Count < 2)
                return;


            if (tracer.LineVisible)
            {
                Handles.color = tracer.pathColor;

                float opacityReduction = 1f / MyPoints.Count;
                float currOpacity = 0f;

                // Draw the path by connecting the points
                for (int i = 0; i < MyPoints.Count - 1; i++)
                {
                    if (tracer.fadingTrail)
                    {
                        Handles.color = new Color(tracer.pathColor.r, tracer.pathColor.g, tracer.pathColor.b, currOpacity);
                    }
                    else
                    {
                        Handles.color = tracer.pathColor;
                    }

                    Handles.DrawLine(MyPoints[i], MyPoints[i + 1]);

                    currOpacity += opacityReduction;
                    currOpacity = Mathf.Clamp01(currOpacity);
                }
            }

        }

        public void DrawPoints(List<Vector3> PathPoints)
        {
            if (tracer.PointsVisible)
            {

                if (PathPoints.Count > 0)
                {
                    for (int i = 0; i < PathPoints.Count; i++)
                    {
                        if (i == PathPoints.Count - 1)
                        {
                            Handles.color = Color.red;
                        }
                        else
                        {
                            Handles.color = tracer.pointColor;
                        }
                        float handleSize = (SceneView.currentDrawingSceneView.in2DMode) ? HandleUtility.GetHandleSize(Vector3.zero) * tracer.pointSize : tracer.pointSize;

                        // Handles.DotHandleCap(i, points[i], Quaternion.LookRotation(Vector3.up), handleSize, EventType.Repaint);
                        Vector3 newPointPos = Handles.FreeMoveHandle(PathPoints[i], handleSize, Vector3.zero, Handles.DotHandleCap);

                        if (PathPoints[i] != newPointPos)
                        {
                            tracer.MovePoint(i, newPointPos);
                        }
                    }

                }
            }

        }
    }
}