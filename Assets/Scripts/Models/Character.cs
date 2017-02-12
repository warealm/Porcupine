using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public class Character : IXmlSerializable
{

    public float X
    {
        get
        {
            return Mathf.Lerp(currTile.X, nextTile.X, movementPercentage);
        }
    }

    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, nextTile.Y, movementPercentage);
        }
    }


    float y;

    public Tile currTile { get; protected set; }
    Tile destTile;                      //If we aren't moving, then destTile = currTile
    Tile nextTile;                      //The next tile in the path finding sequence
    Path_AStar pathAStar;
    float movementPercentage;           //Goes from 0 to 1 as we move from currTile to destTile
    float speed = 10f;                   //Tiles per second

    Action<Character> cbCharacterChanged;
    Job myJob;

    public Character()
    {
        //use only for serialization
    }

    public Character(Tile _tile)
    {
        currTile = destTile = nextTile = _tile;

    }

    public void Update(float deltaTime)
    {
        Update_DoJob(deltaTime);
        Update_DoMovement(deltaTime);

        if (cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
        }
    }

    void Update_DoJob(float deltaTime)
    {
        //Do I have a job?

        if (myJob == null)
        {
            //if not then lets grab the first job from the queue
            myJob = currTile.world.jobQueue.Dequeue();

            if (myJob != null)
            {
                //check to see if the job is reachable

                //we have a job
                myJob.RegisterJobCancelCallback(OnJobEnded);
                myJob.RegisterJobCompleteCallback(OnJobEnded);
                destTile = myJob.tile;
            }
        }

        //Are we there yet?
        if (myJob != null && currTile == myJob.tile)
        {
                myJob.DoWork(deltaTime);
        }
    }


    public void AbandonJob()
    {
        nextTile = destTile = currTile;
        pathAStar = null;
        currTile.world.jobQueue.Enqueue(myJob);
        myJob = null;
    }

    void Update_DoMovement(float deltaTime)
    {
        if (currTile == destTile)
        {
            pathAStar = null;
            //we are already where we want to be.
            return;
        }

        //currTile = tile i'm currently in (and may be in the process of leaving)
        //nextTile = the tile i'm current entering
        //destTile = our final destination, we never walk here directly but isntead use it for pathfinding



        if (nextTile == null || nextTile == currTile)
        {
            //Get the next tile from the pathfinder
            if(pathAStar == null || pathAStar.Length() == 0)
            {
                //Generate a path to our destination
                pathAStar = new Path_AStar(currTile.world, currTile, destTile); //this will calculate a path from curr to dest
                if (pathAStar.Length() == 0)
                {
                    Debug.LogError("Path_AStar -- returned no path to destination!");
                    //fix me, job should be renqueued
                    AbandonJob();
                    pathAStar = null;
                    return;
                }

                //lets ifnore the first tile, because that's the tile we're currently in.
                nextTile = pathAStar.Dequeue();
            }


            //Grab the second tile from the pathing system
            nextTile = pathAStar.Dequeue();

            if(nextTile == currTile)
            {
                Debug.LogError("Update_DoMovement -- nextTile is currTile?");
            }

        }
        //At this point we should have a valid next tile to move to
        //What's the total distance from point A to point B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - nextTile.X, 2) + Mathf.Pow(currTile.Y - nextTile.Y, 2));

        if(nextTile.IsEnterable() == Enterability.Never)
        {
            //Most likely a wall was built while we were walking there, so we just need tor eset our pathfinding 
            //FIXME ideally when a wall gets sapwned, we should invalidate our path immediately
            //so that we won't waste a bunch of time walking towards a dead end
            //to save CPU, maybe we can only check every so often or maybe we should register a callback to the ontile changed event
            Debug.LogError(" FIX ME -- error, a character was trying to enter an unwalkable tile");
            nextTile = null;            //our next tile is a no go
            pathAStar = null;           //our pathfinding info is out of date
            return;
        }
        else if(nextTile.IsEnterable() == Enterability.Soon)
        {
            //we can't enter the tile nowm but we should be able to in the future
            //This is likely a door, so we don't bail on our movement/path but we do return now
            //and don't actually process the movement
            return;
        }


        //How much distance can we travel this update
        float distThisFrame = speed/nextTile.movementCost * deltaTime;

        //How much is that in terms of percentage to our destination
        float percThisFrame = distThisFrame / distToTravel;

        //Add that to overall percentage travelled
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1)
        {
            //Get the next tile from the pathfinding system
            //we have reached our destination
            currTile = nextTile;
            movementPercentage = 0;
        }
    }


    public void SetDestination(Tile _tile)
    {
        if (currTile.IsNeighbour(_tile, true) == false)
        {
            Debug.LogError("SetDestination -- our destination tile isn't actually our neighbour");
        }
        destTile = _tile;
    }


    public void RegisterOnChangedCallBack(Action<Character> callback)
    {
        cbCharacterChanged += callback;
    }

    public void UnregisterOnChangeedCallBack(Action<Character> callback)
    {
        cbCharacterChanged -= callback;
    }

    void OnJobEnded(Job j)
    {
        //Job completed or was cancelled
        if (j != myJob)
        {
            Debug.LogError("OnJobEnded -- character being told about job that isn't theirs");
            return;
        }
        myJob = null;
    }


    #region Saving and Loading
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", currTile.X.ToString());
        writer.WriteAttributeString("Y", currTile.Y.ToString());
    }

    public void ReadXml(XmlReader reader)
    {
        //X, Y and objecttype have already been set and we should ahve already been assigned to a tile
        //just read extra data      
        //movementCost = int.Parse(reader.GetAttribute("movementCost"));

    }
    #endregion

}
