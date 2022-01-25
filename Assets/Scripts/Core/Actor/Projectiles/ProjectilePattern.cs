using UnityEngine;

public abstract class ProjectilePattern : MonoBehaviour
{
    public AudioClip _pewpewSound;
	[Range(0, 7)]
	public int _pewpewPitch;
	public bool changePitchOnShot;
	public bool randomizePitchOnChange;

    public abstract void Execute(Transform origin, int projectileTypeId, GameObject target = null);

    protected void LaunchProjectile(Vector3 origin, Vector3 direction, GameObject projectile)
    {
        if (projectile != null)
        {
            projectile.SetActive(true);
            Vector3 projectileVelocity = direction;
            projectile.transform.position = origin;
            projectile.transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(projectileVelocity.y, projectileVelocity.x) * Mathf.Rad2Deg, Vector3.forward);
            projectile.GetComponent<Rigidbody2D>().AddForce(projectileVelocity, ForceMode2D.Impulse);

			if (AudioManager.Instance && _pewpewSound) {
				float pitch = Helper.GetPitchForNote(_pewpewPitch);
				AudioManager.Instance.PlayClip(_pewpewSound, false, 0, 1.5f, pitch - 0.02f, pitch + 0.02f);
				if (changePitchOnShot) {
					if (randomizePitchOnChange) {
						_pewpewPitch = Random.Range(0, 7);
					} else {
						_pewpewPitch++;
						if (_pewpewPitch > 7) {
							_pewpewPitch = 0;
						}
					}
				}
			}
        }

    }
}
