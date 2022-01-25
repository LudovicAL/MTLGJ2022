using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstFiringMode : FiringMode
{
    [Range(2, 10)]
    [SerializeField] private int bulletsInSequence;
    [SerializeField] private float burstRof;

    private int bulletsRemaining;
    private bool inSequence;
    

    public override bool CanStartSequence(float triggerTime)
    {
        if (!sequenceQueued && IsReady)
        {
            bulletsRemaining = bulletsInSequence;
            sequenceQueued = true;
            return true;
        }
        else if (inSequence)
        {
            return true;
        }
        return false;
    }

    public override bool CanShoot()
    {
        if (sequenceQueued)
        {
            sequenceQueued = false;
            inSequence = true;
            bulletsRemaining--;
            lastShotTime = Time.time;
            return true;
        }
        else if (
            inSequence 
            && Time.time > lastShotTime + burstRof
            && bulletsRemaining > 0)
        {
            lastShotTime = Time.time;
            bulletsRemaining--;
            return true;
        }
            return false;
    }

    public override bool IsSequenceDone(bool forceEnd = false)
    {
        if ((inSequence && bulletsRemaining <= 0) || forceEnd)
        {
            lastShotTime = Time.time;
            sequenceQueued = false;
            inSequence = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool IsReady => Time.time > lastShotTime + (1 / (rateOfFire / 60.0f) * bulletsInSequence);
}
