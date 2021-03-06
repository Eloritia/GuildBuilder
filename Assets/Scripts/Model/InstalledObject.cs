﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class InstalledObject {
    public Vector3Int tile { get; protected set; } //BASE tile, but in practice large objects may occupy multiple tiles

    //ObjectType queried by visual system to know what sprite to render for this object
    public string objectType { get; protected set; }

    //multiplier for movement, value of 2 means twice as slow(half speed)
    //SPECIAL: IF movementCost=0 then the tile is impassable(e.g. a wall)
    public float movementCost;
    int width;
    int height;
    public string sprite { get; protected set; }

    Action<InstalledObject> cbOnChanged;
    public Func<int, int, bool> funcPositionValidation;

    
    public void Update(float deltaTime){
        
    }
    protected InstalledObject () { }
    protected InstalledObject (InstalledObject proto) {
        this.sprite = proto.sprite;
        this.objectType = proto.objectType;
        this.movementCost = proto.movementCost;
        this.width = proto.width;
        this.height = proto.height;
        this.funcPositionValidation = proto.funcPositionValidation;
    }

    public InstalledObject (string objectType, string SpriteName, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeightbor = false) {
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.width = width;
        this.height = height;
        this.sprite = SpriteName;
        this.funcPositionValidation += this.DoorValidPosition;
        this.funcPositionValidation += this.IsValidPosition;
        return;
    }

    static public InstalledObject PlaceInstance (InstalledObject proto, Vector3Int tile_position) {
        if (proto.funcPositionValidation (tile_position.x, tile_position.y) == false) {
            Debug.Log ("Tile position:" + tile_position.x + "_" + tile_position.y);
            Debug.LogError ("Invalid Function Position");
            return null;
        }
        InstalledObject obj = new InstalledObject (proto);
        obj.tile = tile_position;
        return obj;

    }

    public void RegisterOnChangedCallback (Action<InstalledObject> callbackFunc) {
        cbOnChanged += callbackFunc;
    }
    public void UnregisterOnChangedCallback (Action<InstalledObject> callbackFunc) {
        cbOnChanged -= callbackFunc;
    }

    bool IsValidPosition (int x, int y) {
        if (x >= WorldController.Instance.World.Width - 1 || y >= WorldController.Instance.World.Height - 1 || x < 1 || y < 1) { return false; }

        return true;
    }
    bool DoorValidPosition (int x, int y) {
        if (objectType == "Door" || objectType == "Window") {
            if (IsValidPosition (x, y) == true) { //If general position is valid
                Tilemap tilemapFoundation = WorldController.Instance.tilemapFoundation.GetComponent<Tilemap> ();
                // Debug.Log ("Placing Door at position:" + x + "_" + y);
                //            Debug.Log (tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)).name.ToString ().Contains ("Wall"));
                if (tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)) != null &&
                    tilemapFoundation.GetTile (new Vector3Int (x + 1, y, 0)) != null &&
                    tilemapFoundation.GetTile (new Vector3Int (x - 1, y, 0)).name.ToString ().Contains ("Wall") == true &&
                    tilemapFoundation.GetTile (new Vector3Int (x + 1, y, 0)).name.ToString ().Contains ("Wall") == true && (
                        tilemapFoundation.GetTile (new Vector3Int (x, y, 0)) != null && (
                            tilemapFoundation.GetTile (new Vector3Int (x, y, 0)).name.ToString ().Contains ("Wall") == true ||
                            tilemapFoundation.GetTile (new Vector3Int (x, y, 0)).name.ToString ().Contains ("Wall")) == false
                    )) {
                    return true;
                } else if (
                    tilemapFoundation.GetTile (new Vector3Int (x, y - 1, 0)) != null &&
                    tilemapFoundation.GetTile (new Vector3Int (x, y + 1, 0)) != null &&
                    tilemapFoundation.GetTile (new Vector3Int (x, y - 1, 0)).name.ToString ().Contains ("Wall") == true &&
                    tilemapFoundation.GetTile (new Vector3Int (x, y + 1, 0)).name.ToString ().Contains ("Wall") == true && (
                        tilemapFoundation.GetTile (new Vector3Int (x, y, 0)) != null && (
                            tilemapFoundation.GetTile (new Vector3Int (x, y, 0)).name.ToString ().Contains ("Wall") == true ||
                            tilemapFoundation.GetTile (new Vector3Int (x, y, 0)).name.ToString ().Contains ("Wall") == false
                        ))) {
                    return true;
                } else {
                    Debug.LogError ("Can't place Door here");
                    return false;
                }
            }
        }
        return IsValidPosition (x, y);
    }
}