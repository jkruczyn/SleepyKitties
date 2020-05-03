using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;

public class EditorController : MonoBehaviour
{
    private FieldType[,] map;
    private GameObject[,] goMap;// Game Object map for tile sprites
    private GameObject[,] gridMap;// Game object map for GRID
    private int mapSizeX = 6, mapSizeY = 5; // Placeable blocks
    private int gameMapSizeX, gameMapSizeY; // "true" map size - borders around placeable blocks

    public GameObject GridSprite;
    private TilemapObject tilemap = new TilemapObject();
    public List<GameObject> cornerFields = new List<GameObject>(); // List of corner sprites active on board
    private GameObject tmpCorner; // tmp object to append corners
    private Vector2 offsetStart = new Vector2(-3f, -3f); // Offseting corner to draw map

    private enum FieldType {
        EMPTY=0,
        FLOOR,
        WALL
    };
    // Start is called before the first frame update
    void Start()
    {
        gameMapSizeX = mapSizeX + 2; // 1 tile to left and to right around map
        gameMapSizeY = mapSizeY + 3; // 1 tile below map and 2 above (for wall and corners of wall)
        tilemap.LoadTiles();
        goMap = new GameObject[gameMapSizeX, gameMapSizeY]; 
        gridMap = new GameObject[mapSizeX, mapSizeY];
        map = new FieldType[gameMapSizeX, gameMapSizeY];


        tmpCorner = new GameObject();
        tmpCorner.AddComponent<SpriteRenderer>();
        tmpCorner.GetComponent<SpriteRenderer>().sortingLayerName = "TileCorners";
        tmpCorner.SetActive(false);

        // Populating GameObjects with correct positions onto map, based on tempObject 'prefab'
        GameObject tempObject = new GameObject();
        tempObject.AddComponent<SpriteRenderer>();
        for (int y = 0; y < mapSizeY; ++y) {
            for (int x = 0; x < mapSizeX; ++x) {
                // offset starting position + 1.0f (border of 'placeable' tiles) + 0.5f (offset for aligning tiles with grid)
                gridMap[x, y] = Instantiate(tempObject, new Vector3(offsetStart.x + 1.0f + 0.5f + x * 1.0f, offsetStart.y + 1.0f + 0.5f + y * 1.0f, 0), Quaternion.identity);
                gridMap[x, y].GetComponent<SpriteRenderer>().sortingLayerName = "Grid";
            }
        }

        for (int y = 0; y < gameMapSizeY; ++y) {
            for (int x = 0; x < gameMapSizeX; ++x) {
                goMap[x, y] = Instantiate(tempObject, new Vector3(offsetStart.x + 0.5f + x * 1.0f, offsetStart.y + 0.5f + y * 1.0f, 0), Quaternion.identity);
                goMap[x, y].GetComponent<SpriteRenderer>().sortingLayerName = "Tiles";
            }
        }
        Destroy(tempObject);
        refreshMap();
    }

    void Update()
    {
        // Android 

        Vector3 activePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int activeTile = new Vector2Int(Mathf.FloorToInt(activePoint.x - offsetStart.x), Mathf.FloorToInt(activePoint.y - offsetStart.y));
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                activePoint = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
                activeTile = new Vector2Int(Mathf.FloorToInt(activePoint.x - offsetStart.x), Mathf.FloorToInt(activePoint.y - offsetStart.y));
                if (editableTile(activeTile)) {
                    if (mapValueAt(activeTile.x, activeTile.y) == FieldType.FLOOR)
                        setMapValueAt(activeTile.x, activeTile.y, FieldType.EMPTY); // TODO: something wrong here
                    else {
                        setMapValueAt(activeTile.x, activeTile.y, FieldType.FLOOR);
                    }
                    refreshMap();
                }
            }
        }
        // PC

        if (Input.GetMouseButtonDown(0)) {
            Debug.Log(activeTile);
            if (editableTile(activeTile)) {
                setMapValueAt(activeTile.x, activeTile.y, FieldType.FLOOR);
                refreshMap();
            }
        }
        if (Input.GetMouseButtonDown(1)) {
            if (editableTile(activeTile)) {
                if (mapValueAt(activeTile.x, activeTile.y, true) != FieldType.WALL) {
                    setMapValueAt(activeTile.x, activeTile.y, FieldType.EMPTY);
                    refreshMap();
                }
            }
        }

    }

    private bool editableTile(Vector2Int activeTile) {
        // Check if activeTile is within borders of editable grid
        if ((activeTile.x != 0) && (activeTile.x != mapSizeX + 1) && (activeTile.y != 0) && (activeTile.y != mapSizeY + 1) && (activeTile.y != mapSizeY + 2)) {
            return true;
        }
        return false;
    }

    private void refreshMap() {
        // Recalculate borders around floor, reassign sprites and repopulate corners
        foreach (var v in cornerFields) Destroy(v);
        cornerFields.Clear();
        for (int y = 0; y < mapSizeY; ++y) {
            for (int x = 0; x < mapSizeX; ++x) {
                gridMap[x, y].GetComponent<SpriteRenderer>().sprite = GridSprite.GetComponent<SpriteRenderer>().sprite;
            }
        }
        for (int y = 0; y < gameMapSizeY; ++y) {
            for (int x = 0; x < gameMapSizeX; ++x) {
                goMap[x, y].GetComponent<SpriteRenderer>().sprite = GetSurroundingSprite(x, y);
            }
        }
    }
    private FieldType mapValueAt(int x, int y, bool wallCheckup = false) {
        if (x < 0) return 0;
        else if (x >= gameMapSizeX) return 0;
        else if (y < 0) return 0;
        else if (y >= gameMapSizeY) return 0;
        else {
            // If wall - act like floor - wallCheckup lets you see 'true' field
            if (map[x, y] == FieldType.WALL && wallCheckup == false) return FieldType.FLOOR;
            return map[x, y];
        }
    }

    private void setMapValueAt(int x,int y, FieldType field) {
        if (x < 0) return;
        else if (x >= gameMapSizeX) return;
        else if (y < 0) return;
        else if (y >= gameMapSizeY) return;
        else {
            map[x, y] = field;
            // Adding wall if 2 spaces above are empty
            if(field == FieldType.FLOOR && mapValueAt(x,y+1) == FieldType.EMPTY && mapValueAt(x, y+2) == FieldType.EMPTY) {
                setMapValueAt(x, y + 1, FieldType.WALL);
            }
            // Removing wall if directly below floor
            if(field == FieldType.FLOOR && mapValueAt(x,y-1,true) == FieldType.WALL) {
                setMapValueAt(x, y - 1, FieldType.EMPTY);
            }
            // Removing wall above if removing field
            if (field == FieldType.EMPTY && mapValueAt(x, y + 1, true) == FieldType.WALL) {
                setMapValueAt(x, y + 1, FieldType.EMPTY);
            }
            // Adding wall below if removing field and conditions are met
            if (field == FieldType.EMPTY && mapValueAt(x, y - 1) == 0 && mapValueAt(x,y-2,true) == FieldType.FLOOR) {
                setMapValueAt(x, y - 1, FieldType.WALL);
            }
        }
    }

    Sprite GetSurroundingSprite(int x, int y)
    {
        TileType result = TileType.E;
        if (mapValueAt(x,y) == FieldType.FLOOR) result = TileType.F;
        if (mapValueAt(x, y, true) == FieldType.WALL) {
            if (mapValueAt(x - 1, y, true) == FieldType.FLOOR) result = TileType.WR;
            else if (mapValueAt(x + 1, y, true) == FieldType.FLOOR) result = TileType.WL;
            else result = TileType.W;
        }
        else if (mapValueAt(x, y) == 0) {
            int code = (int)mapValueAt(x, y + 1) + ((int)mapValueAt(x + 1, y) << 1) + ((int)mapValueAt(x, y - 1) << 2) + ((int)mapValueAt(x - 1, y) << 3);
            result = tilemap.GetTileTypeByCode(code);
            // Corner setting
            tmpCorner.SetActive(true);
            if (mapValueAt(x - 1, y + 1) == FieldType.FLOOR) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CLU);
                cornerFields.Add(Instantiate(tmpCorner, goMap[x, y].transform));
            }
            if (mapValueAt(x + 1, y + 1) == FieldType.FLOOR) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CRU);
                cornerFields.Add(Instantiate(tmpCorner, goMap[x, y].transform));
            }
            if (mapValueAt(x - 1, y - 1) == FieldType.FLOOR) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CLD);
                cornerFields.Add(Instantiate(tmpCorner, goMap[x, y].transform));
            }
            if (mapValueAt(x + 1, y - 1) == FieldType.FLOOR) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CRD);
                cornerFields.Add(Instantiate(tmpCorner, goMap[x, y].transform));
            }
            tmpCorner.SetActive(false);
        }
        Sprite returnedSprite = tilemap.GetTileSprite(result);
        return returnedSprite;
    }
}
