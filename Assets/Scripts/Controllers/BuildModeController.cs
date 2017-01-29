using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour {

    bool buildModeIsObjects = false;

    TileType buildModeTile= TileType.Floor;
    string buildModeObjectType;


	// Use this for initialization
	void Start ()
    {

	}

    public void SetMode_BuildFloor()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Floor;

    }

    public void SetMode_Bulldoze()
    {
        buildModeIsObjects = false;
        buildModeTile = TileType.Empty;

    }

    public void SetMode_BuildFurniture(string objectType)
    {
        buildModeIsObjects = true;
        buildModeObjectType = objectType;

        //Wall is not a tile, it is an installedOjbect that exits on top of a tile.
    }

    public void DoBuild( Tile t)
    {
        if (buildModeIsObjects == true)
        {
            // create the installed object and assign it to the tile
            //This instantly builds the furniture
            //WorldController.Instance.World.PlaceFurniture(buildModeObjectType, t);

            //Can we build the furniture in the selected tile
            //Run the validplacement function
            string furnitureType = buildModeObjectType;

            if (WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t) && t.pendingFurnitureJob == null)
            {
                //This tile position is valid for this furniture
                //Create a job for it to be built
                Job j = new Job(t, furnitureType, (theJob) => {
                    WorldController.Instance.world.PlaceFurniture(furnitureType, t);
                    t.pendingFurnitureJob = null;
                }
                );

                //Add this to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);
                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

            }
        }
        else
        {
            //we are in tile-changing mode
            t.Type = buildModeTile;
        }


    }

    public void PathfindingTest()
    {
        
        WorldController.Instance.world.SetupPathfindingExample();

        Path_Tilegraph tileGraph = new Path_Tilegraph(WorldController.Instance.world);

    }

}
