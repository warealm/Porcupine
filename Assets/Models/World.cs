using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class World {

    Tile[,] tiles;

    public Dictionary<string, InstalledObject> installedObjectPrototypes;


    int width;
    public int Width
    {
        get
        {
            return width;
        }
    }

    int height;
    public int Height
    {
        get
        {
            return height;
        }
    }


    Action<InstalledObject> cbInstalledObjectCreated;


    public World(int width = 100, int height = 100)
    {
        this.width = width;
        this.height = height;

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y] = new Tile(this, x, y);
            }
        }

        Debug.Log("world created with " + width*height + " Tiles");

        CreateInstalledObjectPrototypes();

    }

    void CreateInstalledObjectPrototypes()
    {
        installedObjectPrototypes = new Dictionary<string, InstalledObject>();
        installedObjectPrototypes.Add("Wall", InstalledObject.CreatePrototype("Wall", 0, 1, 1));
    }


    public void RandomizeTiles()
    {
        Debug.Log("tile randomised");
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = TileType.Empty;
                    
                }
                else
                {
                    tiles[x, y].Type = TileType.Floor;
                }
            }
        }
    }


    public Tile GetTileAt(int x, int y)
    {

        if (x > Width || x < 0||y>Height||y<0)
        {
            Debug.LogError("Tile " + x +","+ y + " is out of range");
            return null;
        }


        return tiles[x,y];
    }

    public void PlaceInstalledObject(string objectType, Tile t)
    {
        //Debug.Log("PlaceInstallObject");
        //ATM this assumes 1x1 tiles
        if (installedObjectPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("installedobjectprototype doens't contain proto for key " + objectType);
            return;
        }

        InstalledObject obj = InstalledObject.PlaceInstance(installedObjectPrototypes[objectType], t);

        if (obj == null)
        {
            //failed to place object, most likely there was already something there
            return;
        }


        if (cbInstalledObjectCreated != null)
        {
            cbInstalledObjectCreated(obj);
        }

    }

    public void RegisterInstalledObjectCreated(Action<InstalledObject> callbackfunc)
    {
        cbInstalledObjectCreated += callbackfunc;
    }

    public void UnregisterInstalledObjectCreated(Action<InstalledObject> callbackfunc)
    {
        cbInstalledObjectCreated -= callbackfunc;
    }

}
