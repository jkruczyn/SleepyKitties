using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Enums;
using System.Linq;
public class TilemapObject
{
    /*
     * names = ['CRD', 'D', 'CLD', 'LU' , 'UR' , 'LUR', 
     *          'WR' , 'W', 'WL' , 'LD' , 'RD' , 'LDR', 
     *          'R'  , 'F', 'L'  , 'DLU', 'DRU', 'LURD', 
     *          'CRU', 'U', 'CLU', 'LR' , 'UD' , 'E'    ]

     */

    private Sprite[] tiles;
    private bool loaded = false;
    private Dictionary<int, TileType> tileCodes = new Dictionary<int, TileType>();
    public TilemapObject() {
        tileCodes.Add(0, TileType.E);
        tileCodes.Add(1, TileType.U);
        tileCodes.Add(2, TileType.R);
        tileCodes.Add(3, TileType.UR);
        tileCodes.Add(4, TileType.D);
        tileCodes.Add(5, TileType.UD);
        tileCodes.Add(6, TileType.RD);
        tileCodes.Add(7, TileType.DRU);
        tileCodes.Add(8, TileType.L);
        tileCodes.Add(9, TileType.LU);
        tileCodes.Add(10, TileType.LR);
        tileCodes.Add(11, TileType.LUR);
        tileCodes.Add(12, TileType.LD);
        tileCodes.Add(13, TileType.DLU);
        tileCodes.Add(14, TileType.LDR);
        tileCodes.Add(15, TileType.LURD);
    }
    public void LoadTiles() {
        tiles = Resources.LoadAll<Sprite>("Tilemaps/dungeon");
        tiles = tiles.OrderBy(x => int.Parse(x.name)).ToArray();
        loaded = true;
    }

    public Sprite GetTileSprite(TileType type) {
        if (loaded) {
            return tiles[(int)type];
        }
        else {
            Debug.Log("WARNING - sprites not loaded");
            return null;
        }
    }

    public TileType GetTileTypeByCode(int code) {
        TileType result;
        try {
            result = tileCodes[code];
        }
        catch (KeyNotFoundException) {
            Debug.LogError("No tile for corresponding code.");
            return TileType.E;
        }
        return result;
    }




}
