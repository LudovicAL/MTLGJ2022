using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBulletExploder : MonoBehaviour
{
    private Vector2 m_CachedVelocity;
    public float m_ImpactForce = 0.0f;
    public GameObject m_ExplosionVFX;
    public GameObject m_Sprite;
    public int m_LayerToSwitchToWhenPopping = 0;
    private int m_OriginalLayer = 0;

    public bool m_RadialExplosion = false;
    public float m_ExplosionRadius = 1.0f;

    private int m_TimeSinceExplosion = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        m_CachedVelocity = GetComponent<Rigidbody2D>().velocity;

        if (m_Sprite.activeSelf == false)
        {
            if (m_ExplosionVFX.GetComponent<ParticleSystem>().isStopped)
            {
                m_Sprite.SetActive(true);
                gameObject.layer = m_OriginalLayer;
                gameObject.SetActive(false);
                m_TimeSinceExplosion = 0;
            }
            else
            {
                m_TimeSinceExplosion++;
                if (m_TimeSinceExplosion == 5 && m_RadialExplosion)
                {
                    Collider2D[] explosionTargets = Physics2D.OverlapCircleAll(transform.position, m_ExplosionRadius);
                    foreach (Collider2D colliderHit in explosionTargets)
                    {
                        Rigidbody2D targetRigidBody = colliderHit.GetComponent<Rigidbody2D>();
                        {
                            float forceMultiplier = 1.0f;
                            GK.BreakableSurface bs = colliderHit.GetComponent<GK.BreakableSurface>();
                            if (bs)
                            {
                                forceMultiplier = 0.5f;
                            }
                            Vector2 thrustDirection = (colliderHit.transform.position - transform.position).normalized;
                            targetRigidBody.AddForce(thrustDirection * m_ImpactForce * forceMultiplier);
                        }
                    }
                }
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_RadialExplosion)
        {
            Collider2D[] explosionTargets = Physics2D.OverlapCircleAll(transform.position, m_ExplosionRadius);
            foreach (Collider2D colliderHit in explosionTargets)
            {
                GK.BreakableSurface bs = colliderHit.GetComponent<GK.BreakableSurface>();
                if (bs)
                {
                    bs.CallBreakExternal();
                }
            }
        }

        float forceMultiplier = 1.0f;
        GK.BreakableSurface localBs = collision.transform.GetComponent<GK.BreakableSurface>();
        if (localBs)
        {
            forceMultiplier = 0.5f;
        }

		if (collision.transform.GetComponent<Rigidbody2D>()) {
			collision.transform.GetComponent<Rigidbody2D>().AddForce(m_CachedVelocity.normalized * m_ImpactForce * forceMultiplier);
		}
        m_ExplosionVFX.GetComponent<ParticleSystem>().Play();
        m_Sprite.SetActive(false);
        m_OriginalLayer = gameObject.layer;
        gameObject.layer = m_LayerToSwitchToWhenPopping;
    }
}
