using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissileType : ProjectileType
{
    //Public variables
    public float m_MaxLifetime = 2.0f;
    public float m_BaseMaxSpeed = 1.0f;
    public float m_BaseMaxSpeedVariance = 0.1f;
    public float m_BaseAcceleration = 1.0f;
    public float m_BaseAccelerationVariance = 0.1f;
    //public float m_BaseRotationSpeed = 1.0f;
    //public float m_BaseRotationSpeedVariance = 0.1f;
    public float m_DirectionJitter = 0.1f;
    public float m_MaxRotationSpeed = 0.5f;
    public float m_MaxJitterAngle = 20.0f;

    //Private variables
    private GameObject m_TargetObj = null;
    //private SpriteRenderer m_MySprite;
    //private Color m_MyColor;
    //private float m_Lifetime = 0.0f;
    //private float m_Speed = 0.0f;
    private float m_MaxSpeed = 0.0f;
    private float m_Acceleration = 0.0f;
    private float m_RotationSpeed = 0.0f;

    private float m_TotalRotationVelocity = 0.0f;
    private float m_TotalRotation = 0.0f;

    private Vector2 m_Direction;
    private Vector2 m_Velocity;

    List<Vector3> m_PreviousPositions = new List<Vector3>();
    private float m_PreviousTime;

    [SerializeField] private int projectileId = -1;
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
            m_TargetObj = GameObject.FindWithTag("Player");

        //m_MySprite = GetComponent<SpriteRenderer>();
        //m_MyColor = Color.red;

        //m_Monsters = GameObject.FindGameObjectsWithTag("Monster");

        m_MaxSpeed = m_BaseMaxSpeed * (1.0f + UnityEngine.Random.Range(-m_BaseMaxSpeedVariance, m_BaseMaxSpeedVariance));
        m_Acceleration = m_BaseAcceleration * (1.0f + UnityEngine.Random.Range(-m_BaseAccelerationVariance, m_BaseAccelerationVariance));
        //m_RotationSpeed = m_BaseRotationSpeed * (1.0f + UnityEngine.Random.Range(-m_BaseRotationSpeedVariance, m_BaseRotationSpeedVariance));

        m_Direction = (m_TargetObj.transform.position - transform.position).normalized;
        m_Velocity = Vector2.zero;

        m_PreviousTime = Time.time;

        float maxTotalRotation = 10.0f * (transform.position - m_TargetObj.transform.position).magnitude;
        m_TotalRotation = UnityEngine.Random.Range(-maxTotalRotation, maxTotalRotation);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 directionToTarget = (m_TargetObj.transform.position - transform.position).normalized;
        bool isLeftOfHeading = (-m_Direction.x * directionToTarget.y + m_Direction.y * directionToTarget.x < 0);

        float angleToTarget = Vector3.Angle((Vector3)m_Direction, (Vector3)directionToTarget);

        //NEEDS A SPRING DAMPER
        if (isLeftOfHeading && angleToTarget > m_MaxJitterAngle)
        {
            m_RotationSpeed *= 0.98f;
            m_RotationSpeed += m_DirectionJitter * Time.deltaTime;
        }
        else if (!isLeftOfHeading && angleToTarget > m_MaxJitterAngle)
        {
            m_RotationSpeed *= 0.98f;
            m_RotationSpeed -= m_DirectionJitter * Time.deltaTime;
        }
        else
        {
            m_RotationSpeed += UnityEngine.Random.Range(-m_DirectionJitter, m_DirectionJitter) * Time.deltaTime;
        }

        m_RotationSpeed = Mathf.Clamp(m_RotationSpeed, -m_MaxRotationSpeed, m_MaxRotationSpeed);

        //m_RotationSpeed = 0.5f;

        Quaternion rotation = Quaternion.AngleAxis(m_RotationSpeed, Vector3.forward);
        Vector3 direction = (Vector3)m_Direction;
        Vector3 newDirection = (rotation * direction).normalized;
        //m_Direction = (Vector2)newDirection;

        m_TotalRotationVelocity += UnityEngine.Random.Range(-100.0f, 100.0f) * Time.deltaTime;
        m_TotalRotationVelocity = Mathf.Clamp(m_TotalRotationVelocity, -500.0f, 500.0f);

        m_TotalRotation += m_TotalRotationVelocity;

        float maxTotalRotation = 10.0f * (transform.position - m_TargetObj.transform.position).magnitude;
        maxTotalRotation = Mathf.Clamp(maxTotalRotation, 0.0f, 50.0f);
        if (Mathf.Abs(m_TotalRotation) > maxTotalRotation)
        {
            m_TotalRotationVelocity = 0.0f;
        }
        m_TotalRotation = Mathf.Clamp(m_TotalRotation, -maxTotalRotation, maxTotalRotation);

        Quaternion rotation2 = Quaternion.AngleAxis(m_TotalRotation, Vector3.forward);
        Vector3 direction2 = (Vector3)directionToTarget;
        Vector3 newDirection2 = (rotation2 * direction2).normalized;
        m_Velocity += Time.deltaTime * (Vector2)newDirection2 * m_Acceleration;

        //m_Velocity += Time.deltaTime * m_Direction * m_Acceleration;
        if (m_Velocity.magnitude > m_MaxSpeed)
        {
            m_Velocity = m_Velocity.normalized * m_MaxSpeed;
        }

        transform.position += (Vector3)m_Velocity * Time.deltaTime;

        // Debug.Log(Time.time - m_PreviousTime);
        if (Time.time - m_PreviousTime > 0.01f)
        {
            //Debug.Log(Time.time - m_PreviousTime);
            m_PreviousTime = Time.time;
            m_PreviousPositions.Add(transform.position);
            if (m_PreviousPositions.Count > 50)
            {
                m_PreviousPositions.RemoveAt(0);
            }
        }

        if ((transform.position - m_TargetObj.transform.position).magnitude < 0.25f)
        {
            Destroy(this);
        }
    }
}
