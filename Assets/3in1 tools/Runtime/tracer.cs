using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace SVassets.AnimationCrafter
{
    [ExecuteInEditMode]
    [AddComponentMenu("SV Assets/3 in 1 Tools/UI Tracer")]
    public class Tracer : MonoBehaviour
    {
        public bool enable = true;

        public float eachPointDist = 1f;
        public int maxPointCount = 16;

        public List<Vector3> points = new();

        private Vector3 lastPos = Vector3.zero;


        void Update()
        {
            UpdatePath();
        }

        void OnDrawGizmos()
        {
            DrawLines();
        }

        public void UpdatePath()
        {
            if (lastPos != transform.position)
            {
                if (points.Count() < 2)
                {
                    points.Add(transform.position);
                    points.Add(transform.position);
                }

                if (Vector3.Distance(points[^2], transform.position) > eachPointDist)
                {
                    points.Add(transform.position);

                    if (points.Count > maxPointCount) points.RemoveAt(0);
                }

                lastPos = transform.position;
                points[^1] = transform.position;
            }
        }


        public void DrawLines()
        {
            if (points == null || points.Count < 2)
                return;


            Gizmos.color = Color.cyan;

            // Draw the path by connecting the points
            for (int i = 0; i < points.Count - 1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }

        }



    }


}
