using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Character {

    public float X
    {
        get
        {
            return Mathf.Lerp(currTile.X, destTile.X, movementPercentage);
        }
    }

    public float Y
    {
        get
        {
            return Mathf.Lerp(currTile.Y, destTile.Y, movementPercentage);
        }
    }


    float y;

    public Tile currTile { get; protected set; }
    Tile destTile;                      //If we aren't moving, then destTile = currTile
    float movementPercentage;           //Goes from 0 to 1 as we move from currTile to destTile
    float speed = 2f;                   //Tiles per second

    Action<Character> cbCharacterChanged;
    Job myJob;

    public Character(Tile _tile)
    {
        currTile = destTile = _tile;

    }

    public void Update(float deltaTime)
    {
        //Do I have a job?

        if(myJob == null)
        {
            //if not then lets grab the first job from the queue
            myJob = currTile.world.jobQueue.Dequeue();

            if (myJob != null)
            {
                //we have a job
                myJob.RegisterJobCancelCallback(OnJobEnded);
                myJob.RegisterJobCompleteCallback(OnJobEnded);
                destTile = myJob.tile;
            }
        }

        //Are we there yet?
        if (currTile == destTile)
        {
            if (myJob != null)
            {
                myJob.DoWork(deltaTime);
            }

            return;
        }

        //What's the total distance from point A to point B
        float distToTravel = Mathf.Sqrt(Mathf.Pow(currTile.X - destTile.X, 2) + Mathf.Pow(currTile.Y - destTile.Y, 2));

        //How much distance can we travel this update
        float distThisFrame = speed * deltaTime;

        //How much is that in terms of percentage to our destination
        float percThisFrame = distThisFrame / distToTravel;

        //Add that to overall percentage travelled
        movementPercentage += percThisFrame;

        if (movementPercentage >= 1)
        {

            //Get the next tile from the pathfinding system

            //we have reached our destination
            currTile = destTile;
            movementPercentage = 0;
        }

        if (cbCharacterChanged != null)
        {
            cbCharacterChanged(this);
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

}
