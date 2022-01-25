using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleFiringMode : FiringMode
{
    private bool inSequence;

    public override bool CanStartSequence(float triggerTime)
    {
        if (!sequenceQueued && !inSequence && Ready)
        {
            sequenceQueued = true;
            return true;
        } else
        {
            inSequence = false;
        }
        return false;
    }

    public override bool CanShoot()
    {
        if (sequenceQueued)
        {
            sequenceQueued = false;
            inSequence = true;
            lastShotTime = Time.time;
            return true;
        }
        else
            return false;
    }

    public override bool IsSequenceDone(bool forceEnd = false)
    {
        if (!inSequence || forceEnd)
        {
            lastShotTime = Time.time;
            sequenceQueued = false;
            inSequence = false;
            return true;
        } else
        {
            return false;
        }
    }
}
