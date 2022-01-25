/**
 * Copyright 2019 Oskar Sigvardsson
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GK {
	public class BreakableSurface : MonoBehaviour {

		public MeshFilter Filter     { get; private set; }
		public MeshRenderer Renderer { get; private set; }

		public Vector2 startingPhysicsForce;
		public float startingTorque;

		public List<Vector2> Polygon;
		private Vector3[] m_LinePoints = new Vector3[0];

		public float MinBreakArea = 0.01f;
		public float MinImpactToBreak = 50.0f;

		public int m_LayerToSwitchToWhenPopping = 0;
		private int m_OriginalLayer = 0;

		private bool m_IsFirstPass = true;
		private Vector2 m_ImpactPoint;
		private List<Vector2> m_RadialSites = new List<Vector2>();
		private List<Vector2> m_GridSites = new List<Vector2>();

		public List<Material> m_InteriorAsteroidMaterials;

		float _Area = -1.0f;
		public int age;

		private bool m_AllowSubDivide = true;
		private float m_NewMassRatio = 1.0f;
		private float m_NewMass = 0.0f;
		private Vector2 m_ParentVelocity = new Vector2(0.0f, 0.0f);
		private Vector2 m_CachedVelocity = new Vector2(0.0f, 0.0f);
		[SerializeField]
		private GameObject goldNuggetPrefab;
		private GameObject goldNuggetsContainer;
		[SerializeField]
		private AudioClip kaboumClip;
		[SerializeField]
		private AudioClip boingClip;

		public float Area {
			get {
				if (_Area < 0.0f) {
					_Area = Geom.Area(Polygon);
				}

				return _Area;
			}
		}

		void Start() {
			goldNuggetsContainer = GameObject.Find("GoldNuggetsContainer");

			age = 0;

			Reload();

			if (m_AllowSubDivide) //Cheap check to see if we are top level parent
            {
				if (m_InteriorAsteroidMaterials.Count > 0)
				{
					gameObject.GetComponent<LineRenderer>().material = m_InteriorAsteroidMaterials[m_InteriorAsteroidMaterials.Count-2];//[Random.Range(0, m_InteriorAsteroidMaterials.Count)];
				}
			}
		}

        public void OnDrawGizmos()
        {
			if (m_ImpactPoint == Vector2.zero)
				return;

			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(m_ImpactPoint, 0.15f);

			Gizmos.color = Color.red;
			for (int i = 0; i < m_RadialSites.Count; ++i)
            {
				Gizmos.DrawWireSphere(m_RadialSites[i], 0.15f);
				Gizmos.DrawLine(m_RadialSites[i], m_ImpactPoint);
			}

			Gizmos.color = Color.blue;
			for (int i = 0; i < m_GridSites.Count; ++i)
			{
				Gizmos.DrawWireSphere(m_GridSites[i], 0.15f);
			}
		}

        public void Reload() {
			var pos = transform.position;

			if (Filter == null) Filter = GetComponent<MeshFilter>();
			if (Renderer == null) Renderer = GetComponent<MeshRenderer>();

			if (Polygon.Count == 0) {
				// Random asteroid!

				var scale = 5.5f * transform.localScale;

				int spokes = 12;

				float segmentAngle = (Mathf.PI * 2.0f) / spokes;

				for(int i = 0; i < spokes; ++i)
                {
					float wiggledAngle = segmentAngle + Random.Range(0.0f, segmentAngle * 0.1f);
					Vector2 polyPoint = Random.Range(0.7f, 1.5f) * scale * new Vector2(Mathf.Cos(wiggledAngle * i), Mathf.Sin(wiggledAngle * i));
					Polygon.Add(polyPoint);
				}

				//Polygon.Add(new Vector2(-scale.x, -scale.y));
				//Polygon.Add(new Vector2(scale.x, -scale.y));
				//Polygon.Add(new Vector2(scale.x, scale.y));
				//Polygon.Add(new Vector2(-scale.x, scale.y));

				transform.localScale = Vector3.one;
			}

			Filter.sharedMesh = MeshFromPolygon(Polygon);

			PolygonCollider2D polyCollider = GetComponent<PolygonCollider2D>();
			if (polyCollider != null)
			{
				polyCollider.SetPath(0, Polygon);
			}
		}

		void FixedUpdate() {
			var pos = transform.position;

			age++;

			LineRenderer myLineRenderer = gameObject.GetComponent<LineRenderer>();
			if (myLineRenderer)
			{
				if (m_LinePoints.Length == 0 && Polygon.Count > 0)
                {
					m_LinePoints = new Vector3[Polygon.Count + 1];
					myLineRenderer.positionCount = Polygon.Count + 1;
				}

				float zOffset = -0.1f;
				int index = 0;
				foreach (Vector2 polyVector in Polygon)
				{
					m_LinePoints[index] = new Vector3(polyVector.x, polyVector.y, zOffset);
					++index;
				}
				//Adding the first point again, as the last point, to close the poly.
				m_LinePoints[Polygon.Count] = new Vector3(Polygon[0].x, Polygon[0].y, zOffset);

				myLineRenderer.SetPositions(m_LinePoints);
			}


			Rigidbody2D myRigidBody = gameObject.GetComponent<Rigidbody2D>();

			if (age == 2)
            {
				if (m_OriginalLayer != 0)
                {
					gameObject.layer = m_OriginalLayer;
				}

				if (startingPhysicsForce != Vector2.zero)
				{
					myRigidBody.AddForce(startingPhysicsForce);
					startingPhysicsForce = Vector2.zero;
				}
				else if (m_ParentVelocity != Vector2.zero)
                {
					float acceleration = m_ParentVelocity.magnitude / Time.deltaTime;
					Vector2 forceToApply = m_ParentVelocity.normalized * (myRigidBody.mass * acceleration);

					myRigidBody.AddForce(forceToApply);

					m_ParentVelocity = Vector2.zero;
				}

				if (startingTorque != 0.0f)
                {
					myRigidBody.AddTorque(startingTorque);
					startingTorque = 0.0f;
				}
				
			}
			else if (age == 3)
            {
				if (m_NewMass != 0.0f)
                {
					gameObject.GetComponent<Rigidbody2D>().mass = m_NewMass;
				}
			}

			//if (pos.magnitude > 1000.0f) {
			//	DestroyImmediate(gameObject);
			//}

			m_CachedVelocity = myRigidBody.velocity;
		}

		public void CallBreakExternal()
        {
			if (age > 5 && m_AllowSubDivide)
			{
				m_AllowSubDivide = false;
				//coll.relativeVelocity.magnitude > MinImpactToBreak) {
				Break((Vector2)transform.InverseTransformPoint(Vector2.zero));
			}
		}	
		
		void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.layer == 7 || collision.gameObject.layer == 10) // other asteroids or AI
				return;

			if (collision.gameObject.layer == 6) {  //Player
				AudioManager.Instance.PlayClip(boingClip);
				return;
			}

			if (age > 5 && m_AllowSubDivide)
			{
				m_AllowSubDivide = false;
				//coll.relativeVelocity.magnitude > MinImpactToBreak) {
				var pnt = collision.contacts[0].point;
				Break((Vector2)transform.InverseTransformPoint(pnt));
			}
		}

		static float NormalizedRandom(float mean, float stddev) {
			var u1 = UnityEngine.Random.value;
			var u2 = UnityEngine.Random.value;

			var randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
				Mathf.Sin(2.0f * Mathf.PI * u2);

			return mean + stddev * randStdNormal;
		}

		public void Break(Vector2 position) {

			var area = Area;

			//Debug.Log("Min:" + MinBreakArea + ", Area:" + area);

			if (area < MinBreakArea)
			{
				return;
			}

			m_OriginalLayer = gameObject.layer;
			gameObject.layer = m_LayerToSwitchToWhenPopping;

			{
				var calc = new VoronoiCalculator();
				var clip = new VoronoiClipper();

				var sites = new Vector2[30]; //30
				int numberOfRadialSites = 15;

				for (int i = 0; i < sites.Length; i++) {
					if (i < numberOfRadialSites)
                    {
						var dist = Mathf.Abs(NormalizedRandom(1.5f, 1.0f / 2.0f)); //Magic numbers go brrr //5.5
						var angle = 2.0f * Mathf.PI * Random.value;

						sites[i] = position + new Vector2(
								dist * Mathf.Cos(angle),
								dist * Mathf.Sin(angle));

						if (m_IsFirstPass)
						{
							m_RadialSites.Add((Vector2)transform.TransformPoint(sites[i]));
						}
					}
					else
                    {
						sites[i] = /*position + */new Vector2(Random.Range(-3.0f, 3.0f), Random.Range(-3.0f, 3.0f));

						if (m_IsFirstPass)
						{
							m_GridSites.Add((Vector2)transform.TransformPoint(sites[i]));
						}
					}



				}

				if (m_IsFirstPass)
				{
					m_ImpactPoint = (Vector2)transform.TransformPoint(position);
					m_IsFirstPass = false;
				}

				var diagram = calc.CalculateDiagram(sites);

				var clipped = new List<Vector2>();

				var myRigidBody = gameObject.GetComponent<Rigidbody2D>();

				for (int i = 0; i < sites.Length; i++) {
					clip.ClipSite(diagram, Polygon, i, ref clipped);

					if (clipped.Count > 0) {
						var newGo = Instantiate(gameObject, transform.parent);

						newGo.transform.localPosition = transform.localPosition;
						newGo.transform.localRotation = transform.localRotation;

						var lr = newGo.GetComponent<LineRenderer>();
						lr.startWidth *= 0.75f;
						lr.endWidth *= 0.75f;

						var bs = newGo.GetComponent<BreakableSurface>();
						bs.m_AllowSubDivide = false; //prevent recursive break!

						bs.Polygon.Clear();
						bs.Polygon.AddRange(clipped);

						var childArea = bs.Area;

						bs.m_NewMass = myRigidBody.mass * (childArea / area);
						bs.m_NewMassRatio = childArea / area;
						bs.m_ParentVelocity = m_CachedVelocity;
						bs.m_OriginalLayer = m_OriginalLayer;

						float massRatio = bs.m_NewMass / 0.3f; // 5 max, but lots of very tiny masses
						int index = Mathf.Clamp((int) massRatio,  0, m_InteriorAsteroidMaterials.Count-1);
						lr.material = m_InteriorAsteroidMaterials[index];//[Random.Range(0, m_InteriorAsteroidMaterials.Count)];
					}
				}

				//Spawn a gold nugget
				if (goldNuggetPrefab && goldNuggetsContainer) {
					Instantiate(goldNuggetPrefab, transform.position, Quaternion.identity, goldNuggetsContainer.transform);
				}

				//Play a sound
				if (AudioManager.Instance && kaboumClip) {
					AudioManager.Instance.PlayClip(kaboumClip);
				}

				//gameObject.transform.position = new Vector2(1000.0f, 1000.0f);
				gameObject.SetActive(false);
				Destroy(gameObject);
			}
		}

		static Mesh MeshFromPolygon(List<Vector2> polygon) {
			var count = polygon.Count;
			// TODO: cache these things to avoid garbage
			var verts = new Vector3[6 * count];
			var norms = new Vector3[6 * count];
			var tris = new int[3 * (4 * count - 4)];
			// TODO: add UVs

			var vi = 0;
			var ni = 0;
			var ti = 0;

			// Bottom
			for (int i = 0; i < count; i++) {
				verts[vi++] = new Vector3(polygon[i].x, polygon[i].y, 0);
				norms[ni++] = Vector3.back;
			}

			for (int vert = 2; vert < count; vert++) {
				tris[ti++] = 0;
				tris[ti++] = vert - 1;
				tris[ti++] = vert;
			}

			for (int vert = 2; vert < count; vert++) {
				tris[ti++] = count;
				tris[ti++] = count + vert;
				tris[ti++] = count + vert - 1;
			}

			for (int vert = 0; vert < count; vert++) {
				var si = 2*count + 4*vert;

				tris[ti++] = si;
				tris[ti++] = si + 1;
				tris[ti++] = si + 2;

				tris[ti++] = si;
				tris[ti++] = si + 2;
				tris[ti++] = si + 3;
			}

			//Debug.Assert(ti == tris.Length);
			//Debug.Assert(vi == verts.Length);

			var mesh = new Mesh();


			mesh.vertices = verts;
			mesh.triangles = tris;
			mesh.normals = norms;

			return mesh;
		}
	}
}
