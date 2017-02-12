using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour
{

    //This barebones controller is mostly just going to piggyback
    //on FurnitureSpriteController because we don't yet fully know
    //what our job system is going to look like in the end

    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;

    // Use this for initialization
    void Start()
    {

        fsc = FindObjectOfType<FurnitureSpriteController>();
        Debug.Log("found");
        jobGameObjectMap = new Dictionary<Job, GameObject>();
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);
        
    }

    void OnJobCreated( Job job)
    {


        //add our tile/gameobject pair to the dictionary.
        if (jobGameObjectMap.ContainsKey(job))
        {
            Debug.LogError("OnJobCreated -- for a jobGO that already exists, mostliekly a job being requeued");
            return;
        }
        //We can only do furniture-building jobs
        GameObject job_go = new GameObject();

        jobGameObjectMap.Add(job, job_go);


        //group the objects in the worldcontroller
        job_go.transform.SetParent(this.transform, true);
        job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Y;
        job_go.transform.position = new Vector3(job.tile.X, job.tile.Y, 0);


        //This hardcoding is not ideal
        if (job.jobObjectType == "Door")
        {
            //By default the door graphic is meant for walls to the east and west
            //check to see if we actually have a wall north/south, and it so then rotate this GO by 90 degrees
            Tile northTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y + 1);
            Tile southTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y - 1);

            if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null && northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall")
            {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }

        }

        //add a sprite renderer, but don't bother setting a sprite because all the tiles are empty atm
        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        sr.sprite = fsc.GetSpriteForFurnitureO(job.jobObjectType);
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        job_go.GetComponent<SpriteRenderer>().sortingLayerName = "Jobs";

        job.RegisterJobCompleteCallback(OnJobEnded);
        job.RegisterJobCancelCallback(OnJobEnded);
    }

    void OnJobEnded(Job job)
    {
        //This executes whether a job was Completed or Canceled

        GameObject job_go = jobGameObjectMap[job];
        job.UnregisterJobCancelCallback(OnJobEnded);
        job.UnregisterJobCompleteCallback(OnJobEnded);

        Destroy(job_go);

    }



}
