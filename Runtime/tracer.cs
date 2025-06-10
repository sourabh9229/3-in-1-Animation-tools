using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace SVassets.AnimationCrafter
{
    [AddComponentMenu("SV Assets/3 in 1 Tools/Tracer")]
    [System.Serializable]
    public class Tracer : MonoBehaviour
    {
        public Transform traceTarget; // The transform to trace
        public Transform pointParent;
        public Color pathColor = Color.cyan; // Color of the path
        public Color pointColor = Color.white; // Color of the path
        public bool LineVisible = true;
        public bool PointsVisible = true;
        public bool fadingTrail = true;
        public float pointSize = 0.2f;
        public bool alwaysShow = false;

        public bool autoDropAnchor = false;
        public float dropDistThreshold = 0.2f;

        public bool autoDelete = false;
        public int maxPointCount = 4;

        public bool MoveAllPoints = false;
        public bool deselected = false;

        public Vector3 parentPos = Vector3.zero;

        // [HideInInspector]
        // public bool traceCurrent = true;

        public List<Vector3> points = new();


        private void OnDrawGizmos()
        {
            if (alwaysShow && deselected)
            {
                UpdatePath();
                DrawLines();
                DrawPoints();
            }
        }

        public void UpdatePath()
        {
            traceTarget = gameObject.transform;

            if (MoveAllPoints)
            {
                pointParent = traceTarget;
            }
            else
            {
                pointParent = null;
            }

            // move last point to target
            if (traceTarget != null)
            {
                if (points.Count > 0)
                {
                    points[points.Count - 1] = traceTarget.position;
                }

            }

            if (pointParent != null && MoveAllPoints)
            {
                if (parentPos == Vector3.zero)
                {
                    parentPos = pointParent.position;
                }

                if (pointParent.transform.position != parentPos)
                {
                    Vector3 deltaMove = pointParent.transform.position - parentPos;
                    for (int i = 0; i < points.Count; i++)
                    {
                        points[i] += deltaMove;
                    }
                }
                parentPos = pointParent.transform.position;
            }
            else if (pointParent == null || !MoveAllPoints)
            {
                parentPos = Vector3.zero;
            }

            // special cases
            if (autoDropAnchor)
            {
                AutoDropNewAnchor();
            }
            if (autoDelete)
            {
                AutoDeleteLast();
            }
        }

        public void AutoDropNewAnchor()
        {
            if (autoDropAnchor && points.Count == 1)
                SetPoint();

            if (autoDropAnchor && points.Count > 1)
            {
                float dist = Vector3.Distance(traceTarget.position, points[^2]);

                if (dist > dropDistThreshold)
                {
                    SetPoint();  // Drop a new anchor at the current GameObject position
                }
            }
        }

        public void AutoDeleteLast()
        {
            if (autoDelete && points.Count > 1)
            {
                if (points.Count > maxPointCount)
                {
                    points.RemoveAt(0);
                }
            }
        }


        public void DrawLines()
        {
            if (points == null || points.Count < 2)
                return;


            if (LineVisible)
            {
                Handles.color = pathColor;

                float opacityReduction = 1f / points.Count;
                float currOpacity = 0f;

                // Draw the path by connecting the points
                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (fadingTrail)
                    {
                        Handles.color = new Color(pathColor.r, pathColor.g, pathColor.b, currOpacity);
                    }
                    else
                    {
                        Handles.color = pathColor;
                    }

                    Handles.DrawLine(points[i], points[i + 1]);

                    currOpacity += opacityReduction;
                    currOpacity = Mathf.Clamp01(currOpacity);
                }
            }

        }

        public void DrawPoints()
        {
            if (PointsVisible)
            {
                if (points.Count > 0)
                {
                    for (int i = 0; i < points.Count; i++)
                    {
                        if (i == points.Count - 1)
                        {
                            Handles.color = Color.red;
                        }
                        else
                        {
                            Handles.color = pointColor;
                        }

                        float handleSize = SceneView.currentDrawingSceneView.in2DMode ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

                        // Handles.DotHandleCap(i, points[i], Quaternion.LookRotation(Vector3.up), handleSize, EventType.Repaint);
                        Vector3 newPointPos = Handles.FreeMoveHandle(points[i], handleSize, Vector3.zero, Handles.DotHandleCap);

                        if (points[i] != newPointPos)
                        {
                            MovePoint(i, newPointPos);
                        }
                    }

                }
            }

        }
        public void FirstSetup()
        {

            if (points.Count < 1)
            {
                SetPoint();
            }
        }

        public void ResetTracer()
        {
            // check if, is there points
            if (points.Count > 1)
            {
                // Clear the points lists 
                points.Clear();

            }
        }

        public void SetPoint()
        {
            if (traceTarget != null)
            {
                points.Add(traceTarget.position);
            }

        }

        public void MovePoint(int index, Vector3 newpos)
        {
            points[index] = newpos;
        }

    }


}
