using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorldController : MonoBehaviour {


    public static WorldController Instance { get; protected set; }      //create an instance so that we can access the worldcontroller

    public Sprite floorSprite;                      //sprite to be used for the floor
    public Sprite wallSprite;                       //sprite to be used for the wall
    public World World { get; protected set; }      //create a world, has getter property but cannot be set outside
    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<InstalledObject, GameObject> installedObjectGameObjectMap;

	// Use this for initialization
	void Start ()
    {
        //Create a world with Empty tiles
        World = new World();

        World.RegisterInstalledObjectCreated(OnInstalledObjectCreated);

        //instantiate our dictionary that tracks which Gameobject is rendering which Tile data
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        installedObjectGameObjectMap = new Dictionary<InstalledObject, GameObject>();

        if (Instance != null)
        {
            Debug.LogError("there should not be two world controllers");
        }

        Instance = this;                            

        //create a GameObject for each of our tiles, so they show visually.
        for (int x = 0; x < World.Width; x++)
        {
            for (int y = 0; y < World.Height; y++)
            {
                Tile tile_data = World.GetTileAt(x, y);
                GameObject tile_go = new GameObject();
                //add our tile/gameobject pair to the dictionary.
                tileGameObjectMap.Add(tile_data, tile_go);
                //group the objects in the worldcontroller
                tile_go.transform.SetParent(this.transform, true);
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);

                //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
                tile_go.AddComponent<SpriteRenderer>();
                //register our callback so that our GO gets updated whenever tiletype changes
                tile_data.RegisterTileTypeChangedCallback(OnTileTypeChanged);
            }
        }

        World.RandomizeTiles();
        
    }


	
	// Update is called once per frame
	void Update ()
    {
	
	}

    //Example, not used
    void DestroyAllTileGameObjects()
    {
        //this function might get called when we are changing floors/levels
        //we need to destroy all visual gameobjects, but not the actual file data

        while(tileGameObjectMap.Count > 0)
        {
            Tile tile_data = tileGameObjectMap.Keys.First();
            GameObject tile_go = tileGameObjectMap[tile_data];

            //remove the pair from the map
            tileGameObjectMap.Remove(tile_data);

            //unregister the callback
            tile_data.UnregisterTileTypeChangedCallback(OnTileTypeChanged);

            //destroy the visual gameobject
            Destroy(tile_go);
        }

        //after this function is called, we'd be calling another function to build all the go's for the tiles on the new floor.

    }

    //This function should be called automatically whenever a tile type gets changed
    void OnTileTypeChanged(Tile tile_data)
    {
        if (tileGameObjectMap.ContainsKey(tile_data) == false)
        {
            Debug.LogError("Doesn't contain the tile data, did you forget to add the tile to the dictionary?");
            return;
        }
        //map the tile_go to the dictionary
        GameObject tile_go = tileGameObjectMap[tile_data];

        if (tile_go == null)
        {
            Debug.LogError("Doesn't contain the tile gameobject, did you forget to add the tile to the dictionary?");
            return;
        }

        if (tile_data.Type == TileType.Floor)
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = floorSprite;
        }
        else if ((tile_data.Type == TileType.Empty))
        {
            tile_go.GetComponent<SpriteRenderer>().sprite = null;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognised tile type");
        }
    }

    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        return World.GetTileAt(x, y);
    }

    public void OnInstalledObjectCreated(InstalledObject obj)
    {
        //Debug.Log("OnInstalledObjectCreated");
        //create a visual GO linked to this data

        //does not consider multi-tile objects nor ortated objects

        GameObject obj_go = new GameObject();
        //add our tile/gameobject pair to the dictionary.
        installedObjectGameObjectMap.Add(obj, obj_go);
        //group the objects in the worldcontroller
        obj_go.transform.SetParent(this.transform, true);
        obj_go.name = obj.objectType + "_" + obj.tile.X + "_" + obj.tile.Y;
        obj_go.transform.position = new Vector3(obj.tile.X, obj.tile.Y, 0);

        //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
        obj_go.AddComponent<SpriteRenderer>().sprite = wallSprite;
        obj_go.GetComponent<SpriteRenderer>().sortingOrder = 1;
        //register our callback so that our GO gets updated whenever tiletype changes
        obj.RegisterOnChangedCallback(OnInstallObjectChanged);

    }

    void OnInstallObjectChanged(InstalledObject obj)
    {
        Debug.LogError("OnsisntalledObjectChanged -- NOT IMPLEMENTED");
    }


}
