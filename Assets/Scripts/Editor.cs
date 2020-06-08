using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Editor {
    public enum FieldType {
        EMPTY = 0,
        FLOOR,
        WALL
    };
    public enum Elements {
        EMPTY = 0,
        CAT,
        PILLOW,
    };
    private class MapRepresentation<T> {
        public T field;
        public GameObject gameObject;
    }

    private MapRepresentation<FieldType>[,] tiles;
    private MapRepresentation<Elements>[,] elements;
    private GameObject[,] grid;
    private List<GameObject> corners;

    private TilemapObject tilemap;

    private Vector2Int mapSize;
    private Vector2Int drawingOffset;

    private Dictionary<Elements, GameObject> elementsDictionary;
    private Sprite gridSprite;

    GameObject tempObject;
    public Editor() {
        tilemap = new TilemapObject();
        tilemap.LoadTiles();

        mapSize = new Vector2Int(5, 7);
        drawingOffset = new Vector2Int(-3, -3);
        tiles = new MapRepresentation<FieldType>[GetMapSizeWithBorders().x, GetMapSizeWithBorders().y];
        elements = new MapRepresentation<Elements>[GetMapSize().x, GetMapSize().y];
        grid = new GameObject[GetMapSize().x, GetMapSize().y];
        corners = new List<GameObject>();
        elementsDictionary = new Dictionary<Elements, GameObject>();

        for (int y = 0; y < GetMapSizeWithBorders().y; ++y) {
            for (int x = 0; x < GetMapSizeWithBorders().x; ++x) {
                tiles[x, y] = new MapRepresentation<FieldType>();
            }
        }
        for (int y = 0; y < GetMapSize().y; ++y) {
            for (int x = 0; x < GetMapSize().x; ++x) {
                elements[x, y] = new MapRepresentation<Elements>();
            }
        }

        tempObject = new GameObject();
        tempObject.AddComponent<SpriteRenderer>();

        GameObject emptySpriteGO = new GameObject();
        emptySpriteGO.AddComponent<SpriteRenderer>();
        emptySpriteGO.GetComponent<SpriteRenderer>().sprite = GetEmptySprite();
        AddElementToDictionary(emptySpriteGO, 0);

        PopulateMaps();
    }

    private Vector3 CalculateTilePosition(int tileX, int tileY, bool editableTiles = true) {
        // offset starting position + 0.5f (offset for aligning tiles with grid) + (eventually) 1.0f (border of 'placeable' tiles)
        return editableTiles ? new Vector3(drawingOffset.x + 1.0f + 0.5f + tileX * 1.0f, drawingOffset.y + 1.0f + 0.5f + tileY * 1.0f, 0) 
                             : new Vector3(drawingOffset.x + 0.5f + tileX * 1.0f, drawingOffset.y + 0.5f + tileY * 1.0f, 0);
    }

    private void PopulateMaps() {
        // Populating GameObjects with correct positions onto map, based on tempObject 'prefab'
        tempObject.GetComponent<SpriteRenderer>().sprite = GetEmptySprite();
        for (int y = 0; y < GetMapSize().y; ++y) {
            for (int x = 0; x < GetMapSize().x; ++x) {
                grid[x, y] = GameObject.Instantiate(tempObject, CalculateTilePosition(x,y), Quaternion.identity);
                grid[x, y].GetComponent<SpriteRenderer>().sortingLayerName = "Grid";


                elements[x, y].gameObject = GameObject.Instantiate(tempObject, CalculateTilePosition(x, y), Quaternion.identity);
                elements[x, y].gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Objects";
                elements[x, y].field = Elements.EMPTY;
            }
        }

        for (int y = 0; y < GetMapSizeWithBorders().y; ++y) {
            for (int x = 0; x < GetMapSizeWithBorders().x; ++x) {
                tiles[x, y].gameObject = GameObject.Instantiate(tempObject, CalculateTilePosition(x, y, false), Quaternion.identity);
                tiles[x, y].gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Tiles";
                tiles[x, y].field = FieldType.EMPTY;
            }
        }
    }


    private Sprite GetSurroundingSprite(int x, int y) {
        TilemapObject.TileType result = TilemapObject.TileType.E;
        if (GetFieldTypeAt(x, y) == FieldType.FLOOR) result = TilemapObject.TileType.F;
        if (GetFieldTypeAt(x, y, true) == FieldType.WALL) {
            if (GetFieldTypeAt(x - 1, y, true) == FieldType.FLOOR) result = TilemapObject.TileType.WR;
            else if (GetFieldTypeAt(x + 1, y, true) == FieldType.FLOOR) result = TilemapObject.TileType.WL;
            else result = TilemapObject.TileType.W;
        }
        else if (GetFieldTypeAt(x, y) == FieldType.EMPTY) {
            int code = (int)GetFieldTypeAt(x, y + 1) + ((int)GetFieldTypeAt(x + 1, y) << 1) + ((int)GetFieldTypeAt(x, y - 1) << 2) + ((int)GetFieldTypeAt(x - 1, y) << 3);
            result = tilemap.GetTileTypeByCode(code);
            // Corner setting
            if (code != 15) {
                if (GetFieldTypeAt(x - 1, y + 1) == FieldType.FLOOR) {
                    tempObject.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TilemapObject.TileType.CLU);
                    corners.Add(GameObject.Instantiate(tempObject, tiles[x, y].gameObject.transform));
                }
                if (GetFieldTypeAt(x + 1, y + 1) == FieldType.FLOOR) {
                    tempObject.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TilemapObject.TileType.CRU);
                    corners.Add(GameObject.Instantiate(tempObject, tiles[x, y].gameObject.transform));
                }
                if (GetFieldTypeAt(x - 1, y - 1) == FieldType.FLOOR) {
                    tempObject.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TilemapObject.TileType.CLD);
                    corners.Add(GameObject.Instantiate(tempObject, tiles[x, y].gameObject.transform));
                }
                if (GetFieldTypeAt(x + 1, y - 1) == FieldType.FLOOR) {
                    tempObject.GetComponent<SpriteRenderer>().sprite = tilemap.GetTileSprite(TilemapObject.TileType.CRD);
                    corners.Add(GameObject.Instantiate(tempObject, tiles[x, y].gameObject.transform));
                }
            }
        }
        Sprite returnedSprite = tilemap.GetTileSprite(result);
        return returnedSprite;
    }

    public void DrawGrid() {
        if (gridSprite == null) {
            Debug.LogWarning("Set grid sprite before drawing the grid! \nAborting drawing the grid.");
            return;
        }
        for (int y = 0; y < GetMapSize().y; ++y) {
            for (int x = 0; x < GetMapSize().x; ++x) {
                grid[x, y].GetComponent<SpriteRenderer>().sprite = gridSprite;
            }
        }
    }
    public void RefreshMap() {
        // Recalculate borders around floor, reassign sprites and repopulate corners
        foreach (var v in corners) GameObject.Destroy(v);
        corners.Clear();
        for (int y = 0; y < GetMapSize().y; ++y) {
            for (int x = 0; x < GetMapSize().x; ++x) {
                elements[x, y].gameObject.GetComponent<SpriteRenderer>().sprite = elementsDictionary[elements[x, y].field].GetComponent<SpriteRenderer>().sprite;
            }
        }

        tempObject.GetComponent<SpriteRenderer>().sortingLayerName = "TileCorners";
        tempObject.SetActive(true);
        for (int y = 0; y < GetMapSizeWithBorders().y; ++y) {
            for (int x = 0; x < GetMapSizeWithBorders().x; ++x) {
                tiles[x, y].gameObject.GetComponent<SpriteRenderer>().sprite = GetSurroundingSprite(x, y);
            }
        }
        tempObject.SetActive(false);
    }

    public void AddElementToDictionary(GameObject o, Elements e) {
        elementsDictionary[e] = o;
    }

    public void SetGridSprite(Sprite gridSprite) {
        this.gridSprite = gridSprite;
    }

    public Sprite GetEmptySprite() {
        return tilemap.GetTileSprite(TilemapObject.TileType.E);
    }

    public Vector2Int GetMapSize() {
        return mapSize;
    }
    public Vector2Int GetMapSizeWithBorders() {
        return new Vector2Int(this.mapSize.x + 2, this.mapSize.y + 3);
    }

    public FieldType GetFieldTypeAt(int x, int y, bool wallCheckup = false) {
        if (x < 0) return 0;
        else if (x >= GetMapSizeWithBorders().x) return 0;
        else if (y < 0) return 0;
        else if (y >= GetMapSizeWithBorders().y) return 0;
        else {
            // If wall - act like floor - wallCheckup lets you see 'true' field
            if (tiles[x, y].field == FieldType.WALL && wallCheckup == false) return FieldType.FLOOR;
            return tiles[x, y].field;
        }
    }
    public Elements GetElementAt(Vector2Int pos) {
        if (pos.x < 0) return 0;
        else if (pos.x >= GetMapSizeWithBorders().x) return 0;
        else if (pos.y < 0) return 0;
        else if (pos.y >= GetMapSizeWithBorders().y) return 0;
        else {
            // If wall - act like floor - wallCheckup lets you see 'true' field
            return elements[pos.x, pos.y].field;
        }
    }
    public void SetElementAt(Vector2Int pos, Elements val) {
        if (pos.x < 0) return;
        else if (pos.x >= GetMapSize().x + 1) return;
        else if (pos.y < 0) return;
        else if (pos.y >= GetMapSize().y + 1) return;
        elements[pos.x, pos.y].field = val;
    }

    public void SetTileAt(int x, int y, FieldType field) {
        if (x < 0) return;
        else if (x >= GetMapSizeWithBorders().x) return;
        else if (y < 0) return;
        else if (y >= GetMapSizeWithBorders().y) return;
        else {
            tiles[x, y].field = field;
            if (field == FieldType.FLOOR) {
                // Adding wall if 2 spaces above are empty
                if (GetFieldTypeAt(x, y + 1) == FieldType.EMPTY && GetFieldTypeAt(x, y + 2) == FieldType.EMPTY) {
                    SetTileAt(x, y + 1, FieldType.WALL);
                }
                // Removing wall if directly below floor
                if (GetFieldTypeAt(x, y - 1, true) == FieldType.WALL) {
                    SetTileAt(x, y - 1, FieldType.EMPTY);
                }
            }
            else if (field == FieldType.EMPTY) {
                // Removing wall above if removing field
                if (GetFieldTypeAt(x, y + 1, true) == FieldType.WALL) {
                    SetTileAt(x, y + 1, FieldType.EMPTY);
                }
                // Adding wall below if removing field and conditions are met
                if (GetFieldTypeAt(x, y - 1) == 0 && GetFieldTypeAt(x, y - 2, true) == FieldType.FLOOR) {
                    SetTileAt(x, y - 1, FieldType.WALL);
                }
                // Adding wall at field if removing field and conditions are met
                if (GetFieldTypeAt(x, y) == 0 && GetFieldTypeAt(x, y + 1) == 0 && GetFieldTypeAt(x, y - 1, true) == FieldType.FLOOR) {
                    SetTileAt(x, y, FieldType.WALL);
                }
            }
        }
    }

    public Vector2Int GetWorldTile(Vector3 worldPoint) {
        return new Vector2Int(Mathf.FloorToInt(worldPoint.x - drawingOffset.x), Mathf.FloorToInt(worldPoint.y - drawingOffset.y));
    }
    private bool EditableTile(Vector2Int activeTile) {
        // Check if activeTile is within borders of editable grid
        if ((activeTile.x > 0) && (activeTile.x <= GetMapSize().x) && (activeTile.y > 0) && (activeTile.y <= GetMapSize().y)) {
            return true;
        }
        return false;
    }

    public void ModifyTileAt(Vector2Int tile, EditorUIController.Buttons element) {
        if (EditableTile(tile)) {
            if (element == EditorUIController.Buttons.FLOOR) {
                if (GetFieldTypeAt(tile.x, tile.y, true) == FieldType.EMPTY)
                    SetTileAt(tile.x, tile.y, FieldType.FLOOR);
                else if (GetFieldTypeAt(tile.x, tile.y, true) == FieldType.FLOOR)
                    SetTileAt(tile.x, tile.y, FieldType.EMPTY);
                else if (GetFieldTypeAt(tile.x, tile.y, true) == FieldType.WALL)
                    SetTileAt(tile.x, tile.y, FieldType.FLOOR);
            }
            else {
                if (GetFieldTypeAt(tile.x, tile.y, true) == FieldType.FLOOR) {
                    Vector2Int editableTile = new Vector2Int(tile.x - 1, tile.y - 1);
                    if(GetElementAt(editableTile) == Elements.EMPTY)
                        SetElementAt(editableTile, (Elements)element);
                    else
                        SetElementAt(editableTile, Elements.EMPTY);
                }
            }
        }
    }
}
