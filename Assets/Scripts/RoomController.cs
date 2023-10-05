using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/**
 * Controller for a Room. Contains a list of shapes and a list of prop locations
 */
public class RoomController : MonoBehaviour
{

    public GameObject tilePrefab;
    public GameObject doorPrefab;
    public GameObject fogPrefab;
    public GameObject[] propOptions;
    public GameObject[] monsterOptions;


    private GameObject[,] fogLayer;
    public Shape[] shapes;
    public Shape[] props; //For now props are assumed to be 1x1.
    public Vector2[] monsters; //locations for monsters.
    public Vector2[] doors; //Should be updated to being possible door locations.
    public int width;
    public int height;
    public bool hasPath = false;

    private int x;
    private int y;
    private Boolean hidden = false;

    private System.Random random;


    public List<Path> paths {get; private set;}

    private static int roomCount = 0; //Counts the number for each room for recorder, do not modify for anything else


    public RoomController()
    {   
        random = new System.Random();
        //TODO: Also make sure that the shapes are not overlapping
    }


    public void SetPosition(int x, int y){
        this.x = x;
        this.y = y;
    }

    /**
     * Place all of the tiles and props into the game, and also insert them into the arrays for foreground and background objects.
     */
    public void PlaceRoom(Transform transformParent, GameObject[,] background, GameObject[,] foreground, GameObject[,] gridFogLayer, float size, float margin, Recorder recorder, CustomGeneration customGeneration, SpritesheetManager spritesheetManager)
    {

        this.paths = new();

        recorder.AddRoom(new RecorderRoom(++roomCount, this.x, this.y));
        //Place the floors for each shape that makes up the room
        foreach (Shape shape in shapes)
        {
            for (int x = shape.x1;  x < shape.x2; x++)
            {
                for (int y = shape.y1;  y < shape.y2; y++)
                {
                    GameObject tile = Instantiate(tilePrefab, new Vector3((x + this.x) * (size + margin), (y + this.y) * (size + margin), 0), Quaternion.identity, transformParent);
                    tile.GetComponent<FloorController>().Init(size - margin * 2, spritesheetManager);
                    background[x + this.x, y + this.y] = tile;
                    recorder.AddTile(new RecorderTile("floor", x + this.x, y + this.y, roomCount));
                }
            }
        }
        
        //Place the doors for each door in the room
        foreach (Vector2 doorLoc in doors)
        {
            GameObject door = Instantiate(doorPrefab, new Vector3((doorLoc.x + this.x) * (size + margin), (doorLoc.y + this.y) * (size + margin), 1), Quaternion.identity, transformParent);
            door.GetComponent<DoorController>().Init(size - margin * 2, spritesheetManager);
            door.GetComponent<DoorController>().SetParent(this);
            background[(int)doorLoc.x + this.x, (int)doorLoc.y + this.y] = door;
            recorder.AddTile(new RecorderTile("door", (int)doorLoc.x + this.x, (int)doorLoc.y + this.y, roomCount));
        }

        //Randomly select props for each prop location in the room.
        if (customGeneration.HasProps())
        {
            foreach (Shape propLocation in props)
            {
                //propIndex is used to store the random number to choose which prop is shown so that it can be given to the recorder.
                int propIndex = random.Next(propOptions.Length);
                GameObject prop = Instantiate(propOptions[propIndex], new Vector3((propLocation.x1 + this.x) * (size + margin), (propLocation.y1 + this.y) * (size + margin), -1), Quaternion.identity, transformParent);
                prop.GetComponent<PropController>().Init(size - margin * 2);
                foreground[propLocation.x1 + this.x, propLocation.y1 + this.y] = prop;
                recorder.AddTile(new RecorderTile("prop", propLocation.x1 + this.x, propLocation.y1 + this.y, roomCount, propOptions[propIndex].ToString()));
            }
        }


        if (customGeneration.HasMonsters())
        {
            foreach (Vector2 monsterLoc in monsters)
            {
                //monsterIndex is used to store the random number to choose which prop is shown so that it can be given to the recorder.
                int monsterIndex = random.Next(monsterOptions.Length);
                GameObject monster = Instantiate(monsterOptions[monsterIndex], new Vector3((monsterLoc.x + this.x) * (size + margin), (monsterLoc.y + this.y) * (size + margin), -1), Quaternion.identity, transformParent);
                monster.GetComponent<MonsterController>().Init(size - margin * 2);
                foreground[(int)(monsterLoc.x + x), (int)(monsterLoc.y + y)] = monster;
                recorder.AddTile(new RecorderTile("monster", (int)(monsterLoc.x + x), (int)(monsterLoc.y + y), roomCount, monsterOptions[monsterIndex].ToString()));
            }
        }

        fogLayer = new GameObject[width, height];

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                GameObject fog = Instantiate(fogPrefab, new Vector3((w + this.x) * (size + margin), (h + this.y) * (size + margin), -2), Quaternion.identity, transformParent);
                fog.GetComponent<TileController>().Init(size - margin * 2);
                fogLayer[w, h] = fog;
                gridFogLayer[w + x, h + y] = fog;
                recorder.AddTile(new RecorderTile("fog", w, h, roomCount));
            }
        }
        
    }

    /*
     * Removes the fog if point is inside room
     */
    public void ClearFog(Vector2 mousePos)
    {
        if (InsideRoom(mousePos) && !hidden)
        {
            hidden = true;
            HideTiles();
            fogLayer = new GameObject[width, height];
        }
    }

    /*
     * Checks if point is within this room
     */
    public Boolean InsideRoom(Vector2 mousePos)
    {
        if (mousePos.x <= (this.x + this.width)
            && mousePos.x >= this.x
            && mousePos.y <= (this.y + this.height)
            && mousePos.y >= this.y)
        {
            return true;
        }
        return false;
    }

    /*
     * Hides all fog tiles
     */
    public void HideTiles()
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                fogLayer[w, h].SetActive(false);
            }
        }
    }

    /*
     * Shows all fog tiles
     */
    public void ShowTiles()
    {
        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                fogLayer[w, h].SetActive(true);
            }
        }
    }

    public void setHasPathTrue()
    {
        this.hasPath = true;
    }

    public int GetX(){
        return this.x;
    }

    public int GetY(){
        return this.y;
    }
  
}
