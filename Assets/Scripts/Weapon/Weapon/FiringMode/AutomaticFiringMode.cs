using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticFiringMode : FiringMode
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
        if (sequenceStarted && waitingForPlayerInput && Ready)
        {
            lastShotTime = Time.time;
            return true;
        }

        if (sequenceQueued)
        {
            sequenceQueued = false;
            sequenceStarted = true;
            waitingForPlayerInput = true;
            lastShotTime = Time.time;
            return true;
        }
        return false;
    }

    public override bool IsSequenceDone(bool forceEnd = false)
    {
        if ((sequenceStarted && !waitingForPlayerInput) || forceEnd)
        {
            sequenceQueued = false;
            sequenceStarted = false;
            waitingForPlayerInput = false;
            return true;
        }
        else
            return false;
    }
}
