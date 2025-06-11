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


		// prefs
		public Color pathColor = Color.yellow; // Color of the path
		public Color KeysColor = Color.white; // Color of the path

		public float pointSize = 0.1f;

		public bool ShowPath = true;
		public bool showKeyPoints = false;
		public bool ShowFramePoints = false;

		public bool deselected = false;
		public bool alwaysShow = false;
		public bool AutoUpdatePath = false;


		// database
		public List<Vector3> KeyPoints = new();
		public List<Vector3> valueEachFrame = new();
		public XYZCurves curves = new();



		public void OnDrawGizmos()
		{
			if (alwaysShow && deselected)
			{
				if (AutoUpdatePath)
				{
					UpdateMotionPath();
				}
				DrawPath();
				DrawKeyframePoints();
				DrawFramePoints();
			}

		}


		// updating motion paths
		public void UpdateMotionPath()
		{
			if (transform != null && clip != null)
			{
				animator = GetComponent<Animator>();

				// getting curves
				foreach (var binding in AnimationUtility.GetCurveBindings(clip))
				{
					if ((animator != null) ? binding.path == "" : binding.path.EndsWith(transform.name))
					{
						if (binding.propertyName == "m_LocalPosition.x")
						{
							curves.x = AnimationUtility.GetEditorCurve(clip, binding);
						}
						if (binding.propertyName == "m_LocalPosition.y")
						{
							curves.y = AnimationUtility.GetEditorCurve(clip, binding);
						}
						if (binding.propertyName == "m_LocalPosition.z")
						{
							curves.z = AnimationUtility.GetEditorCurve(clip, binding);
						}
					}
				}

				// Getting value each frame
				int sampleCount = Mathf.FloorToInt(clip.length * 60f) + 1;
				Vector3[] positionSamples = new Vector3[sampleCount];

				for (int i = 0; i < sampleCount; i++)
				{
					float time = i * (1 / 60f);
					Vector3 pos = GetWorldPositionFromCurves(time);

					positionSamples[i] = pos;
				}

				valueEachFrame = positionSamples.ToList();


				// gettign keyframe positions
				if (curves.x.keys.Length > 0 && curves.y.keys.Length > 0 && curves.z.keys.Length > 0)
				{
					SortedSet<float> keyTimes = new();

					// Add keyframe times from both curves
					foreach (var keyframe in curves.x.keys)
					{
						keyTimes.Add(keyframe.time);
					}
					foreach (var keyframe in curves.y.keys)
					{
						keyTimes.Add(keyframe.time);
					}
					foreach (var keyframe in curves.z.keys)
					{
						keyTimes.Add(keyframe.time);
					}

					List<Vector3> pointArr = new();

					// Iterate through all the unique keyframe times in order
					foreach (float time in keyTimes)
					{
						Vector3 pos = GetWorldPositionFromCurves(time);

						// Add the resulting Vector2 to the list
						pointArr.Add(pos);
					}

					KeyPoints = pointArr;
				}

				if (curves.IsNull() || curves.HaveNoKeyframes())
				{
					ResetMotionPath();
				}

				SceneView.RepaintAll();

			}

		}

		private Vector3 GetWorldPositionFromCurves(float time)
		{
			float x = GetValueFromCurveOrPosition(curves.x, time);
			float y = GetValueFromCurveOrPosition(curves.y, time);
			float z = GetValueFromCurveOrPosition(curves.z, time);

			Vector3 local = new(x, y, z);

			Matrix4x4 matrix = Matrix4x4.TRS(local, Quaternion.identity, Vector3.one);

			Transform current = transform.parent;
			while (current != null)
			{
				Matrix4x4 parentMatrix = Matrix4x4.TRS(current.localPosition, current.localRotation, current.localScale);
				matrix = parentMatrix * matrix;

				current = current.parent;
			}

			return matrix.MultiplyPoint3x4(Vector3.zero);
		}


		private float GetValueFromCurveOrPosition(AnimationCurve curve, float time)
		{
			// Check if the curve has keyframes
			if (curve.length > 0)
			{
				if (time <= curve.keys[0].time)
				{
					return curve.keys[0].value;
				}
				else if (time >= curve.keys[curve.length - 1].time)
				{
					return curve.keys[curve.length - 1].value;
				}
				else
				{
					return curve.Evaluate(time);
				}
			}
			else
			{
				return 0f;
			}
		}


		// drawing
		public void DrawPath()
		{
			if (clip != null)
			{
				if (valueEachFrame == null || valueEachFrame.Count < 2)
				{
					return;
				}

				if (ShowPath)
				{
					Handles.color = pathColor;

					// Draw the path by connecting the points
					for (int i = 0; i < valueEachFrame.Count - 1; i++)
					{
						Handles.DrawLine(valueEachFrame[i], valueEachFrame[i + 1]);
					}
				}
			}
		}

		public void DrawKeyframePoints()
		{
			if (clip != null)
			{
				if (KeyPoints == null && KeyPoints.Count < 1)
					return;


				if (showKeyPoints)
				{
					Handles.color = KeysColor;
					for (int i = 0; i < KeyPoints.Count; i++)
					{
						float handleSize = SceneView.currentDrawingSceneView.in2DMode ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

						Handles.CylinderHandleCap(0, KeyPoints[i], Quaternion.identity, handleSize, EventType.Repaint);
					}
				}
			}

		}

		public void DrawFramePoints()
		{
			if (valueEachFrame == null && valueEachFrame.Count < 1)
				return;

			if (ShowFramePoints)
			{
				Handles.color = KeysColor;
				for (int i = 0; i < valueEachFrame.Count; i++)
				{
					float handleSize = SceneView.currentDrawingSceneView.in2DMode ? HandleUtility.GetHandleSize(Vector3.zero) * pointSize : pointSize;

					Handles.DrawWireDisc(valueEachFrame[i], Vector3.forward, handleSize * 0.3f);
				}
			}
		}

		// resetting
		public void ResetMotionPath()
		{
			KeyPoints.Clear();
			valueEachFrame.Clear();
			curves.Clear();

			SceneView.RepaintAll();
		}

	}


}
