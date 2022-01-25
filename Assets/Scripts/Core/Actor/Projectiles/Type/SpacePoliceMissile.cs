using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpacePoliceMissile : ProjectileType
{
    private GameObject m_TargetObj;
    [SerializeField] private int projectileId = -1;

    public float m_TurnSpeed = 0.0f;
    public float m_TimeToFullAcceleration = 0.0f;
    public float m_MaxSpeed = 0.0f;

    public override int ProjectileId
    {
        get
        {
            if (projectileId == -1)
                return this.gameObject.GetInstanceID();
            else
                return projectileId;
        }
        set
        {
            projectileId = value;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_TargetObj == null)
        {
            m_TargetObj = GameObject.FindWithTag("Player");

            Vector3 directionVector = (m_TargetObj.transform.position - transform.position).normalized;
            float angle = (Mathf.Atan2(directionVector.x, directionVector.y) * Mathf.Rad2Deg) + 90.0f;
            transform.Rotate(Vector3.forward, angle);

            Rigidbody2D myRigidBody = GetComponent<Rigidbody2D>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody2D myRigidBody = GetComponent<Rigidbody2D>();
        Vector2 desiredAcceleration = transform.right * Helper.ComputeAccelerationToReachDestination(transform.right * 100.0f, transform, m_MaxSpeed, 0.0f, myRigidBody, m_TimeToFullAcceleration, false).magnitude;
        myRigidBody.AddForce(desiredAcceleration);
        Vector2 directionToTarget = (m_TargetObj.transform.position - transform.position).normalized;

        //Undesired velocity
        Vector2 antiVelocity = -1.0f * (myRigidBody.velocity - (Vector2)Vector3.Project(myRigidBody.velocity, directionToTarget));
        float requiredAcceleration = (antiVelocity.magnitude / Time.deltaTime)/2.0f;

        float angleToTarget = Vector2.SignedAngle(-transform.right + (Vector3)(antiVelocity * requiredAcceleration), directionToTarget);
        if (angleToTarget > 0.0f)
        {
            myRigidBody.rotation += -m_TurnSpeed * Time.deltaTime;
        }
        else if (angleToTarget < 0.0f)
        {
            myRigidBody.rotation += m_TurnSpeed * Time.deltaTime;
        }
    }
}
