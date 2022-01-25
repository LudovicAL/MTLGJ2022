using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Helper
{
    public static (Vector3 position, Vector3 direction) MouseData(Transform directionSource = null)
    {
        float mouseX = Mouse.current.position.ReadValue().x;
        float mouseY = Mouse.current.position.ReadValue().y;
        float mouseZ = Camera.main.nearClipPlane;

        Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(mouseX, mouseY, mouseZ));
        position.z = 0.0f;
        Vector3 direction = (directionSource != null) ? position - directionSource.position : Vector3.zero;
        return (position, direction.normalized);
    }

	public static Vector2 ComputeAccelerationToReachDestination(Vector2 destination, Transform transform, float maxSpeed, float brakeDistance, Rigidbody2D rigidbody2D, float timeToReachDesiredVelocity, bool allowBrake = true) {
		Vector2 accel = Vector2.zero;
		Vector2 toTarget = destination - (Vector2)transform.position;
		float targetSpeed = maxSpeed;
		// braking distance
		if (allowBrake && toTarget.sqrMagnitude <= brakeDistance * brakeDistance) {
			targetSpeed = maxSpeed * (toTarget.magnitude / brakeDistance);
		}
		// want to go towards target but not too fast
		Vector2 targetVelocity = toTarget.normalized * targetSpeed;
		// a = v/t
		accel = targetVelocity - rigidbody2D.velocity;
		accel *= 1 / timeToReachDesiredVelocity; // time in which we want to achieve desired velocity
		return accel;
	}

	public static float GetPitchForNote(int note) {
		if (note < 0 || note > 7) {
			Debug.LogWarning("You requested a pitch for a note outside of the allowed range. " + note.ToString() + " is not in the range 0 to 7.");
			note = 1;
		}
		return Mathf.Pow(2f, (float)note / 12);
	}
}
