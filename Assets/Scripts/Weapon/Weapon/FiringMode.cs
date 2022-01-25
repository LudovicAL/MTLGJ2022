using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class FiringMode : MonoBehaviour
{
    [Range(0.01f, 2400.0f)]
    [Tooltip("Rate of fire per minute (RPM)")]
    public float rateOfFire = 600.0f;

    protected bool Ready => Time.time > lastShotTime + (1 / (rateOfFire / 60.0f));

    protected float lastShotTime;
    protected bool sequenceQueued;
    protected bool sequenceStarted;

    public abstract bool CanStartSequence(float triggerTime);
    public abstract bool CanShoot();
    public abstract bool IsSequenceDone(bool forceEnd = false);
}
