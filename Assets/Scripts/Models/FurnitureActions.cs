﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FurnitureActions {

    public static void Door_UpdateAction(Furniture furn, float deltaTime)
    {


        if(furn.GetParameter("is_opening") >= 1)
        {
            furn.ChangeParameter("openness", deltaTime*4); //maybe a door open speed parameter?

            if(furn.GetParameter("openness") >= 1)
            {
                furn.SetParameter("is_opening", 0);
            }


        }
        else
        {
            furn.ChangeParameter("openness", deltaTime*-4);
        }

        furn.SetParameter("openness", Mathf.Clamp01(furn.GetParameter("openness")));

        if (furn.cbOnChanged != null)
        {
            furn.cbOnChanged(furn);
        }
        
        
    }

    public static Enterability Door_IsEnterable(Furniture furn)
    {
        furn.SetParameter("is_opening", 1);


        if(furn.GetParameter("openness") >= 1)
        {
            return Enterability.Yes;
        }


        return Enterability.Soon;
    }


}
