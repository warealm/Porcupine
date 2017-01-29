using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class WorldController : MonoBehaviour {


    public static WorldController Instance { get; protected set; }      //create an instance so that we can access the worldcontroller
    public World world { get; protected set; }      //create a world, has getter property but cannot be set outside

	// Use this for initialization
	void OnEnable ()
    {
        //Create a world with Empty tiles
        world = new World();

        if (Instance != null)
        {
            Debug.LogError("there should not be two world controllers");
        }

        Instance = this;                            

        //Center the camera
        Camera.main.transform.position = new Vector3(world.Width / 2, world.Height / 2, Camera.main.transform.position.z);

    }

    private void Update()
    {
        world.Update(Time.deltaTime);
    }


    public Tile GetTileAtWorldCoord(Vector3 coord)
    {
        int x = Mathf.RoundToInt(coord.x);
        int y = Mathf.RoundToInt(coord.y);

        return world.GetTileAt(x, y);
    }

}
