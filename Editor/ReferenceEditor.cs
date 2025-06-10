using UnityEditor;
using UnityEngine;

namespace SVassets.AnimationCrafter.EditorTools
{
    [CustomEditor(typeof(ImageScreen))]
    public class ReferenceEditor : Editor
    {
        ImageScreen referencer;

        public override void OnInspectorGUI()
        {
            referencer = (ImageScreen)target;

            if (referencer.animationWindow == null && referencer.syncWithAnimation) // set animation window
            {
                referencer.animationWindow = EditorWindow.GetWindow<AnimationWindow>();
            }

            if (referencer.frames != null && referencer.frames.Length > 0)
            {
                float width = EditorGUIUtility.currentViewWidth;  // Full width of the inspector
                float aspectRatio = (float)referencer.frames[referencer.currentFrame].width / referencer.frames[referencer.currentFrame].height;
                float height = width / aspectRatio;  // Maintain aspect ratio for the image

                if (referencer.syncWithAnimation)
                {
                    if (referencer.animationWindow != null)
                    {
                        GUILayout.Label("Frame  " + (referencer.currentFrame + 1));

                        // Draw the image with a fixed size and no extra padding/margins
                        Rect rect = GUILayoutUtility.GetRect(width, height);  // Get a rect of the correct size
                        GUI.DrawTexture(rect, referencer.frames[referencer.currentFrame], ScaleMode.ScaleToFit);  // Draw the image

                        referencer.syncWithAnimation = EditorGUILayout.Toggle(new GUIContent("Sync with Animation Frames", "When enabled, frame will change according to current frames in animation window."), referencer.syncWithAnimation);

                        EditorGUILayout.Slider(new GUIContent("Frame Index", "Only readable frame count - 1 is here, and controlled via Animation window frame."), referencer.currentFrame, 0f, referencer.frames.Length - 1); // slider timeline

                        // Ensure currentFrame is within valid bounds
                        if (referencer.animationWindow.frame < referencer.frames.Length && referencer.animationWindow.frame >= 0)
                        {
                            referencer.currentFrame = referencer.animationWindow.frame;
                            referencer.UpdateTexture();
                        }

                        Repaint();
                    }
                }
                else
                {
                    GUILayout.Label("Frame  " + (referencer.currentFrame + 1));

                    Rect rect = GUILayoutUtility.GetRect(width, height);  // Get a rect of the correct size
                    GUI.DrawTexture(rect, referencer.frames[referencer.currentFrame], ScaleMode.ScaleToFit);  // Draw the image

                    referencer.syncWithAnimation = EditorGUILayout.Toggle(new GUIContent("Sync with Animation Frames", "When enabled, frame will change according to current frames in animation window."), referencer.syncWithAnimation);

                    referencer.currentFrame = (int)EditorGUILayout.Slider(new GUIContent("Frame Index", "This slider is movable, shows frame count - 1, Also controlled via previous-next frame buttons."), referencer.currentFrame, 0f, referencer.frames.Length - 1); // slider timeline

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Previous"))
                    {
                        if (referencer.currentFrame <= 0)
                        {
                            referencer.currentFrame = 0;
                        }
                        else
                        {
                            referencer.currentFrame--;
                        }
                    }
                    if (GUILayout.Button("Next"))
                    {
                        if (referencer.currentFrame >= referencer.frames.Length - 1)
                        {
                            referencer.currentFrame = 0;
                        }
                        else
                        {
                            referencer.currentFrame++;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    referencer.UpdateTexture();
                    Repaint();
                }
            }
            else if (referencer.frames == null)
            {
                GUILayout.Label("No video frames loaded.");
            }

            EditorGUILayout.BeginHorizontal();
            referencer.selectedImgType = EditorGUILayout.Popup(new GUIContent("Format", "Only images with selected format will be searched inside folder using Load Frame button."), referencer.selectedImgType, referencer.imageTypes);
            // Load frames button
            if (GUILayout.Button(new GUIContent("Load Frames", "Images must be named according to, name(1), name(2), name(3)... to be sorted right way.")))
            {
                referencer.LoadFrames();
            }
            EditorGUILayout.EndHorizontal();

        }
    }
}