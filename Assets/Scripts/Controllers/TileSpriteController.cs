using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class TileSpriteController : MonoBehaviour {


    public Sprite floorSprite;                      //sprite to be used for the floor
    public Sprite emptySprite;
    Dictionary<Tile, GameObject> tileGameObjectMap;

    World world { get { return WorldController.Instance.world; } }

	// Use this for initialization
	void Start ()
    {

        //instantiate our dictionary that tracks which Gameobject is rendering which Tile data
        tileGameObjectMap = new Dictionary<Tile, GameObject>();
        //create a GameObject for each of our tiles, so they show visually.
        for (int x = 0; x < world.Width; x++)
        {
            for (int y = 0; y < world.Height; y++)
            {
                Tile tile_data = world.GetTileAt(x, y);
                GameObject tile_go = new GameObject();

                //add our tile/gameobject pair to the dictionary.
                tileGameObjectMap.Add(tile_data, tile_go);

                //group the objects in the worldcontroller
                tile_go.transform.SetParent(this.transform, true);
                tile_go.name = "Tile_" + x + "_" + y;
                tile_go.transform.position = new Vector3(tile_data.X, tile_data.Y, 0);

                //add a sprite renderer, and add a default sprite for empty tile
                SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                sr.sprite = emptySprite;
                sr.sortingLayerName = "Tiles";
            }
        }

        //register our callback so that our GO gets updated whenever tile's data changes
        world.RegisterTileChanged(OnTileChanged);

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
            tile_data.UnregisterTileTypeChangedCallback(OnTileChanged);

            //destroy the visual gameobject
            Destroy(tile_go);
        }
        //after this function is called, we'd be calling another function to build all the go's for the tiles on the new floor.
    }

    //This function should be called automatically whenever a tile type gets changed
    void OnTileChanged(Tile tile_data)
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
            tile_go.GetComponent<SpriteRenderer>().sprite = emptySprite;
        }
        else
        {
            Debug.LogError("OnTileTypeChanged - Unrecognised tile type");
        }
    }
}
