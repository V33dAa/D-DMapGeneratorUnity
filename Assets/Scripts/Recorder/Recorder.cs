using UnityEngine;
using System.Collections.Generic;

using System.IO;

[System.Serializable]
public class Recorder
{
    public int width;
    public int height;
    public float cellSize;
    public float cellSpacing;

    public List<RecorderTile> tiles = new List<RecorderTile>();
    public List<RecorderRoom> rooms = new List<RecorderRoom>();

    public Recorder(GridController gridController)
    {
        width = gridController.width;
        height = gridController.height;
        cellSize = gridController.cellSize;
        cellSpacing = gridController.cellSpacing;    
    }

    public void AddTile(RecorderTile tile)
    {
        tiles.Add(tile);
    }

    public void AddRoom(RecorderRoom room)
    {
        rooms.Add(room);
    }
}

public class GridControllerJsonSerializer : MonoBehaviour
{
    public static void SerializeToJson(GridController gridController, string filePath, Recorder serializableGridController)
    {
        string json = JsonUtility.ToJson(serializableGridController, true);
        File.WriteAllText(filePath, json);
    }

    public static Recorder DeserializeFromJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<Recorder>(json);
        }
        else
        {
            Debug.LogError("File not found: " + filePath);
            return null;
        }
    }

    public static void SaveSceneAsPNG(string filePath, int width, int height, Camera topDownCamera)
    {
        // Create a new RenderTexture with the specified width and height
        RenderTexture rt = new RenderTexture(width, height, 24);

        // Use the top-down camera for rendering
        topDownCamera.targetTexture = rt;
        topDownCamera.Render();

        // Create a Texture2D and read the RenderTexture data into it
        Texture2D screenshot = new Texture2D(width / 3 * 2, height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(width / 3, 0, width / 3 * 2, height), 0, 0);
        topDownCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        // Encode the Texture2D to a PNG file and save it
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
    }
}