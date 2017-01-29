using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Job {

    //this class holds infor for a queeud up job, which can include
    //things like placing furniture, moving stored inventory,
    //working at a desk, and maybe even fighting enemies.

    public Tile tile { get; protected set; }
    float jobTime;

    //Fix me: hard coding a parameter for a furniture, change this later
    public string jobObjectType { get; protected set; }

    Action<Job> cbJobComplete;          //Action for a job has been completed
    Action<Job> cbJobCancel;            //ACtion when a job get's cancelled

    public Job(Tile _tile, string _jobObjectType, Action<Job> _cbJobComplete, float _jobTime = 1f)
    {
        tile = _tile;
        jobObjectType = _jobObjectType;
        cbJobComplete += _cbJobComplete;
        jobTime = _jobTime;

    }

    public void RegisterJobCompleteCallback(Action<Job> callback)
    {
        cbJobComplete += callback;
    }

    public void RegisterJobCancelCallback(Action<Job> callback)
    {
        cbJobCancel += callback;
    }

    public void UnregisterJobCompleteCallback(Action<Job> callback)
    {
        cbJobComplete -= callback;
    }

    public void UnregisterJobCancelCallback(Action<Job> callback)
    {
        cbJobCancel -= callback;
    }

    public void DoWork(float workTime)
    {
        jobTime -= workTime;
        if (jobTime <= 0)
        {
            if (cbJobComplete != null)
            {
                cbJobComplete(this);
            }
            
        }
    }

    public void CancelJob()
    {
        if (cbJobCancel != null)
        {
            cbJobCancel(this);
        }

    }

}
