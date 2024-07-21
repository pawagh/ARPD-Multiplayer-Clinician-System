using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System;

public class JSONLoader : MonoBehaviour
{
    public TextAsset jsonFile;
    private List<Vector3[]> frames = new List<Vector3[]>();

    private void Awake()
    {
        LoadJSONData();
    }

    private void LoadJSONData()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON file not assigned in JSONLoader.");
            return;
        }

        try
        {
            string jsonText = jsonFile.text;
            JObject jsonObject = JObject.Parse(jsonText);

            foreach (var frame in jsonObject)
            {
                Vector3[] jointPositions = new Vector3[24];  // Assuming 24 joints as per JSON structure
                JArray frameData = (JArray)frame.Value;

                for (int i = 0; i < 24 && i < frameData.Count; i++)
                {
                    JArray jointPosition = (JArray)frameData[i];
                    if (jointPosition.Count >= 3)
                    {
                        jointPositions[i] = new Vector3(
                            (float)jointPosition[0],
                            (float)jointPosition[1],
                            (float)jointPosition[2]
                        );
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid joint position data for frame {frame.Key}, joint {i}");
                        jointPositions[i] = Vector3.zero;
                    }
                }
                frames.Add(jointPositions);
            }

            Debug.Log($"Loaded {frames.Count} frames from JSON");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing JSON: {e.Message}");
        }
    }

    public int GetFrameCount()
    {
        return frames.Count;
    }

    public Vector3[] GetFrameSafe(int index)
    {
        if (index < 0 || index >= frames.Count)
        {
            Debug.LogWarning($"Frame index {index} is out of range. Using closest valid frame.");
            index = Mathf.Clamp(index, 0, frames.Count - 1);
        }
        return frames[index];
    }
}