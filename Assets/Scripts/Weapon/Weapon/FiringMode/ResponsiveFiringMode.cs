using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResponsiveFiringMode : FiringMode
{
    private bool waitingForPlayerInput;

    public override bool CanStartSequence(float triggerTime)
    {
        if (sequenceStarted && waitingForPlayerInput)
        {
            waitingForPlayerInput = false;
            return true;
        }
            

        if (!sequenceQueued && Ready)
        {
            sequenceQueued = true;
            return true;
        }
        return false;
    }

    public override bool CanShoot()
    {
        if (waitingForPlayerInput)
            return false;

        if (sequenceQueued) {
            sequenceQueued = false;
            sequenceStarted = true;
            waitingForPlayerInput = true;
            return true;
        } else if (sequenceStarted) {
            waitingForPlayerInput = false;
            return true;
        }
        return false;
    }

    public override bool IsSequenceDone(bool forceEnd = false)
    {
        if ((sequenceStarted && !waitingForPlayerInput) || forceEnd)
        {
            lastShotTime = Time.time;
            sequenceStarted = false;
            waitingForPlayerInput = false;
            sequenceQueued = false;
            return true;
        }
        else
            return false;
    }
}
