using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;

public class EditorController : MonoBehaviour
{
    private int[,] map;
    private GameObject[,] goMap;
    private GameObject[,] gridMap;
    private int mapSizeX = 5, mapSizeY = 5;

    public GameObject GridSprite;
    private TilemapObject tilemap = new TilemapObject();
    private GameObject tmpCorner;
    // Start is called before the first frame update
    void Start()
    {
        tilemap.LoadTiles();
        goMap = new GameObject[mapSizeX, mapSizeY];
        gridMap = new GameObject[mapSizeX, mapSizeY];

        tmpCorner = new GameObject();
        tmpCorner.AddComponent<SpriteRenderer>();
        tmpCorner.GetComponent<SpriteRenderer>().sortingLayerName = "TileCorners";
        tmpCorner.SetActive(false);

        GameObject tempObject = new GameObject();
        tempObject.AddComponent<SpriteRenderer>();

        for (int y = 0; y < mapSizeY; ++y) {
            for (int x = 0; x < mapSizeX; ++x) {
                gridMap[x, y] = Instantiate(tempObject, new Vector3(0.5f + x * 1.0f, 0.5f + y * 1.0f, 0), Quaternion.identity);
                gridMap[x, y].GetComponent<SpriteRenderer>().sortingLayerName = "Grid";
            }
        }

        for (int y = 0; y < mapSizeY; ++y) {
            for (int x = 0; x < mapSizeX; ++x) {
                goMap[x, y] = Instantiate(tempObject, new Vector3(0.5f + x * 1.0f, 0.5f + y * 1.0f, 0), Quaternion.identity);
                goMap[x, y].GetComponent<SpriteRenderer>().sortingLayerName = "Tiles";
            }
        }
        Destroy(tempObject);
        map = new int[mapSizeX,mapSizeY];
        refreshMap();
    }

    // Update is called once per frame
    void Update()
    {


        Vector3 activePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2Int activeTile = new Vector2Int(Mathf.FloorToInt(activePoint.x), Mathf.FloorToInt(activePoint.y));
        if (Input.GetMouseButtonDown(0)) {
            Debug.Log(activeTile);
            setMapValueAt(activeTile.x, activeTile.y, 1);
            refreshMap();
        }
        if (Input.GetMouseButtonDown(1)) {
            setMapValueAt(activeTile.x, activeTile.y, 0);
            refreshMap();
        }

    }
    private void refreshMap() {
        tilemap.cornerFields.Clear();
        for (int y = 0; y < mapSizeY; ++y) {
            for (int x = 0; x < mapSizeX; ++x) {
                gridMap[x, y].GetComponent<SpriteRenderer>().sprite = GridSprite.GetComponent<SpriteRenderer>().sprite;
                goMap[x,y].GetComponent<SpriteRenderer>().sprite = GetSurroundingSprite(x, y);
            }
        }
    }
    private int mapValueAt(int x, int y, bool wallCheckup = false) {
        if (x < 0) return 0;
        else if (x >= mapSizeX) return 0;
        else if (y < 0) return 0;
        else if (y >= mapSizeY) return 0;
        else {
            // If wall - act like floor - wallCheckup lets you see 'true' field
            if (map[x, y] == 2 && wallCheckup == false) return 1;
            return map[x, y];
        }
    }

    private void setMapValueAt(int x,int y, int val) {
        if (x < 0) return;
        else if (x >= mapSizeX) return;
        else if (y < 0) return;
        else if (y >= mapSizeY) return;
        else {
            map[x, y] = val;
            if(val == 1 && mapValueAt(x,y+1) == 0 && mapValueAt(x, y+2) == 0) {
                setMapValueAt(x, y + 1, 2);
            }
            if(val == 1 && mapValueAt(x,y-1,true) == 2) {
                setMapValueAt(x, y - 1, 0);
            }
        }
    }

    Sprite GetSurroundingSprite(int x, int y)
    {
        TileType result = TileType.E;
        if (map[x, y] == 1) result = TileType.F;
        if (map[x, y] == 2) result = TileType.W;
        else if (map[x, y] == 0) {
            int code = mapValueAt(x, y + 1) + (mapValueAt(x + 1, y) << 1) + (mapValueAt(x, y - 1) << 2) + (mapValueAt(x - 1, y) << 3);
            result = tilemap.GetTileTypeByCode(code);
            // Corner setting
            tmpCorner.SetActive(true);
            if (mapValueAt(x-1,y+1) == 1) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CLU);
                tilemap.cornerFields.Add(Instantiate(tmpCorner,gridMap[x,y].transform));
            }
            if (mapValueAt(x + 1, y + 1) == 1) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CRU);
                tilemap.cornerFields.Add(Instantiate(tmpCorner, gridMap[x, y].transform));
            }
            if (mapValueAt(x - 1, y - 1) == 1) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CLD);
                tilemap.cornerFields.Add(Instantiate(tmpCorner, gridMap[x, y].transform));
            }
            if (mapValueAt(x + 1, y - 1) == 1) {
                tmpCorner.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TileType.CRD);
                tilemap.cornerFields.Add(Instantiate(tmpCorner, gridMap[x, y].transform));
            }
            tmpCorner.SetActive(false);
        }
        Sprite returnedSprite = tilemap.GetTileSprite(result);
        return returnedSprite;
    }
}
