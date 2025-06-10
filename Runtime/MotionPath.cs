using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace SVassets.AnimationCrafter
{
    [AddComponentMenu("SV Assets/3 in 1 Tools/Motion Path")]
    [System.Serializable]
    public class MotionPath : MonoBehaviour
    {
        public AnimationClip clip;
        public Animator animator;
        public Transform pathTarget; // The transform to trace
        public Transform pathParent; // path parent
        private Vector3 parentPos = Vector3.zero;
        public bool HoldAlPoints = false;


        public Color pathColor = Color.yellow; // Color of the path
        public Color pointColor = Color.white; // Color of the path
        public float pointSize = 0.1f;
        public bool showPoints = true;
        public bool ShowLines = true;
        public bool ShowCirles = true;



        public List<Vector3> points = new();
        public List<Vector3> valueEachTime = new();
        public AnimationCurve xCurve = new();
        public AnimationCurve yCurve = new();
        public AnimationCurve zCurve = new();


        public bool deselected = false;
        public bool alwaysShow = false;
        public bool AutoUpdatePath = false;


        public void OnDrawGizmos()
        {
            if (alwaysShow && deselected)
            {
                if (AutoUpdatePath)
                {
                    UpdateMotionPath();
                }
                DrawLines();
                DrawPoints();
                DrawRailsPoints(valueEachTime);
            }

        }

        // updating motion paths
        public void UpdateMotionPath()
        {
            if (pathTarget != null && clip != null)
            {
                animator = pathTarget.GetComponent<Animator>();

                // get curves
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if ((animator != null) ? binding.path == "" : binding.path.EndsWith(pathTarget.name))
                    {

                        if (binding.propertyName == "m_LocalPosition.x")
                        {
                            xCurve = AnimationUtility.GetEditorCurve(clip, binding);
                        }
                        if (binding.propertyName == "m_LocalPosition.y")
                        {
                            yCurve = AnimationUtility.GetEditorCurve(clip, binding);
                        }
                        if (binding.propertyName == "m_LocalPosition.z")
                        {
                            zCurve = AnimationUtility.GetEditorCurve(clip, binding);
                        }
                    }
                }

                // Sample points every second
                int sampleCount = Mathf.FloorToInt(clip.length * 60f) + 1;
                Vector3[] positionSamples = new Vector3[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    float time = i * (1 / 60f);
                    float myxValue = xCurve != null ? xCurve.Evaluate(time) : 0f;
                    float myyValue = yCurve != null ? yCurve.Evaluate(time) : 0f;
                    float myzValue = zCurve != null ? zCurve.Evaluate(time) : 0f;

                    positionSamples[i] = new Vector3(myxValue, myyValue, myzValue);

                }
                valueEachTime = positionSamples.ToList();



                if (xCurve.keys.Length > 0 && yCurve.keys.Length > 0 && zCurve.keys.Length > 0)
                {
                    SortedSet<float> keyTimes = new();

                    // Add keyframe times from both curves
                    foreach (var keyframe in xCurve.keys)
                    {
                        keyTimes.Add(keyframe.time);
                    }
                    foreach (var keyframe in yCurve.keys)
                    {
                        keyTimes.Add(keyframe.time);
                    }
                    foreach (var keyframe in zCurve.keys)
                    {
                        keyTimes.Add(keyframe.time);
                    }

                    List<Vector3> pointArr = new();

                    // Iterate through all the unique keyframe times in order
                    foreach (float time in keyTimes)
                    {
                        float xValue = GetValueFromCurveOrPosition(xCurve, time, pathTarget, "x");

                        float yValue = GetValueFromCurveOrPosition(yCurve, time, pathTarget, "y");

                        float zValue = GetValueFromCurveOrPosition(zCurve, time, pathTarget, "z");

                        Vector3 pos = new(xValue, yValue, zValue);

                        Transform currentTransform = pathTarget;
                        // find the topmost parent (root)
                        while (currentTransform.parent != null)
                        {
                            if (animator != null)
                            {
                                break;
                            }
                            else
                            {
                                currentTransform = currentTransform.parent;
                            }
                        }

                        // someone other is parent
                        if (currentTransform != pathTarget)
                        {
                            pos = currentTransform.TransformPoint(pos); // local scaling
                        }

                        // Add the resulting Vector2 to the list
                        pointArr.Add(pos);
                    }

                    points = pointArr;
                }

                if (xCurve.keys.Length < 1 && yCurve.keys.Length < 1)
                {
                    ResetMotionPath();
                }

                SceneView.RepaintAll();

            }

        }

        private float GetValueFromCurveOrPosition(AnimationCurve curve, float time, Transform target, string axis)
        {
            // Check if the curve has keyframes and the time is within range
            if (curve.length > 0 && curve.keys[0].time <= time && curve.keys[curve.length - 1].time >= time)
            {
                // Optional: Add actions you want to perform when the curve is evaluated
                return curve.Evaluate(time);  // Return the evaluated curve value
            }
            else
            {
                // Optional: Add actions you want to perform when the curve is not used

                if (axis == "x")
                {
                    return target.position.x;
                }
                else if (axis == "y")
                {
                    return target.position.x;
                }
                else if (axis == "z")
                {
                    return target.position.x;
                }
                else
                {
                    return 0;
                }
            }
        }


        public void DrawLines()
        {
            if (pathTarget != null && clip != null)
            {
                if (valueEachTime == null || valueEachTime.Count < 2)
                {
                    return;
                }

                if (ShowLines)
                {
                    Handles.color = pathColor;

                    // Draw the path by connecting the points
                    for (int i = 0; i < valueEachTime.Count - 1; i++)
                    {
                        Handles.DrawLine(valueEachTime[i], valueEachTime[i + 1]);
                    }
                }
            }
        }

        public void DrawPoints()
        {
            if (pathTarget != null && clip != null)
            {
                if (points == null)
                    return;


                if (showPoints)
                {
                    if (points.Count > 0)
                    {
                        Handles.color = pointColor;
                        for (int i = 0; i < points.Count; i++)
                        {
                            float handleSize = (SceneView.currentDrawingSceneView.in2DMode) ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

                            // Handles.DotHandleCap(i, points[i], Quaternion.LookRotation(Vector3.up), handleSize, EventType.Repaint);
                            Vector3 newPointPos = Handles.FreeMoveHandle(points[i], handleSize, Vector3.zero, Handles.CubeHandleCap);

                        }

                    }
                }
            }

        }

        public void DrawRailsPoints(List<Vector3> MyPoints)
        {
            if (MyPoints == null)
                return;


            if (ShowCirles)
            {
                if (MyPoints.Count > 0)
                {
                    Handles.color = pointColor;
                    for (int i = 0; i < MyPoints.Count; i++)
                    {
                        float handleSize = (SceneView.currentDrawingSceneView.in2DMode) ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

                        Handles.DrawWireDisc(MyPoints[i], Vector3.forward, handleSize*0.3f);
                    }

                }
            }
        }


        public void ResetMotionPath()
        {
            // check if, is there points
            if (points != null)
            {
                // Clear the points lists 
                points.Clear();
                xCurve = new AnimationCurve();
                yCurve = new AnimationCurve();
                SceneView.RepaintAll();

            }
        }

    }


}
