using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;


namespace SVassets.AnimationCrafter
{
    [AddComponentMenu("SV Assets/3 in 1 Tools/Image Screen")]
    [System.Serializable]
    public class ImageScreen : MonoBehaviour
    {
        public Texture2D[] frames;
        public string[] imageTypes = { "jpg", "png" };
        public int selectedImgType = 1;
        public int currentFrame = 0;
        private GameObject targetPlane;  // Reference to the target GameObject to trace path for

        public bool syncWithAnimation = false;

        public AnimationWindow animationWindow;

        public void LoadFrames()
        {
            string path = EditorUtility.OpenFolderPanel("Select Frame Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                List<Texture2D> loadedFrames = new List<Texture2D>();
                string[] files = Directory.GetFiles(path, "*." + imageTypes[selectedImgType]); // You can also add other formats like "*.jpg", "*.jpeg"

                Array.Sort(files); // Sort files by name

                // Create a new array to store the rearranged files
                string[] sortedFiles = new string[files.Length];

                for (int i = 0; i < files.Length; i++)
                {
                    string name = files[i];
                    Match match = Regex.Match(name, @"\((\d+)\)");

                    if (match.Success)
                    {
                        // Extract the number as a string and convert it to an integer
                        int imgNum = int.Parse(match.Groups[1].Value) - 1;

                        // Check if the extracted number is within bounds of the array
                        if (imgNum >= 0 && imgNum < sortedFiles.Length)
                        {
                            sortedFiles[imgNum] = files[i]; // Place the file at the correct index
                        }
                        else
                        {
                            Debug.LogWarning($"Extracted number {imgNum} is out of bounds.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"No number found in the string: {name}");
                    }
                }

                foreach (string file in sortedFiles)
                {
                    byte[] fileData = File.ReadAllBytes(file);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(fileData); // Automatically resizes the texture dimensions.
                    loadedFrames.Add(texture);
                }

                frames = loadedFrames.ToArray();
                currentFrame = 0;
                UpdateTexture();
            }
        }

        public void UpdateTexture()
        {
            targetPlane = gameObject;

            if (targetPlane != null && frames != null && currentFrame < frames.Length && targetPlane.GetComponent<Renderer>() != null)
            {
                Renderer renderer = targetPlane.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = new Material(Shader.Find("Unlit/Texture"));
                    // Update the texture on every frame change
                    renderer.sharedMaterial.mainTexture = frames[currentFrame];

                }else{
                    renderer.sharedMaterial.mainTexture = frames[currentFrame];
                }
            }
        }

        public void ResetReferencer()
        {
            Array.Clear(frames, 0, frames.Length);
            currentFrame = 0;
        }
    }

    
}
