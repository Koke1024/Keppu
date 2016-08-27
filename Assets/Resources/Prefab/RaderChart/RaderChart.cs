using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RaderChart : MonoBehaviour {

	private GameObject obj;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	[SerializeField]
	private Material mat;
	private float length;

	private float[] _values;
	private int _vertexCnt;
	private float[] _showValues;

	void Start() {
	}

	public void Init(int vertexCnt, float[] values, float scale) {
		length = scale;
		_vertexCnt = vertexCnt;
		_values = values;
		_showValues = values;
		//
		obj = new GameObject("BuildPlane");
		obj.transform.SetParent(transform);
		meshFilter = obj.AddComponent<MeshFilter>();
		meshRenderer = obj.AddComponent<MeshRenderer>();
		meshRenderer.material = mat;

		Mesh m = meshFilter.mesh;
		m.name = "newMesh";
		m.Clear();
		m.vertices = GetVertex();
		m.triangles = Triangulate(m.vertices);
		m.uv = GetUVVertex();
	}

	public void SetPosition(Vector2 position) {
		obj.transform.position = new Vector3(position.x, position.y, -1.0f);
    }

	public void UpdateValue(float[] values) {
		StartCoroutine("UpdateChart", values);
	}

	IEnumerator UpdateChart(float[] values) {
		float[] beforeValues = _values;
		_values = values;
		for (int i = 1; i <= 60; ++i) {
			for (int t = 0; t < _vertexCnt; ++t) {
				_showValues[t] = (beforeValues[t] * (60 - i) + _values[t] * i) / 60.0f;
			}
			Mesh m = meshFilter.mesh;
			m.Clear();
			m.vertices = GetVertex();
			m.triangles = Triangulate(m.vertices);
			m.uv = GetUVVertex();

			yield return null;
		}
	}

	Vector3[] GetVertex() {
		Vector3[] vertices = new Vector3[_vertexCnt];
		for (int i = 0; i < _vertexCnt; ++i) {
			float angle = -Mathf.PI * 2.0f / _vertexCnt;
			vertices[i] = new Vector3(
				length * 0.5f * Mathf.Cos(angle * i + (Mathf.PI / 2.0f)) * _showValues[i],
				length * 0.5f * Mathf.Sin(angle * i + (Mathf.PI / 2.0f)) * _showValues[i]
				);
		}
		return vertices;
	}

	Vector2[] GetUVVertex() {
		Vector2[] vertices = new Vector2[_vertexCnt];
		for (int i = 0; i < _vertexCnt; ++i) {
			float angle = -Mathf.PI * 2.0f / _vertexCnt;
			vertices[i] = new Vector2(
				0.5f * Mathf.Cos(angle * i + (Mathf.PI / 2.0f)) * _showValues[i] + 0.5f,
				0.5f * Mathf.Sin(angle * i + (Mathf.PI / 2.0f)) * _showValues[i] + 0.5f
				);
		}
		return vertices;
	}

	int[] Triangulate(Vector3[] m_points) {
		List<int> indices = new List<int>();

		int n = _vertexCnt;
		if (n < 3) {
			return indices.ToArray();
		}

		int[] V = new int[n];
		for (int v = 0; v < n; v++) {
			V[v] = v;
		}

		int nv = n;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2;) {
			if ((count--) <= 0) {
				break;
			}

			int u = v;
			if (nv <= u) {
				u = 0;
			}
			v = u + 1;
			if (nv <= v) {
				v = 0;
			}
			int w = v + 1;
			if (nv <= w) {
				w = 0;
			}

			if (!Snip(u, v, w, nv, V, m_points)) {
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++) {
					V[s] = V[t];
				}
				nv--;
				count = 2 * nv;
			}
		}
		bool reversFlag = true;
		int i0 = indices[0];
		int i1 = indices[1];
		int i2 = indices[2];
		Vector3 pos0 = new Vector3(m_points[i0].x, m_points[i0].y, 0f);
		Vector3 pos1 = new Vector3(m_points[i1].x, m_points[i1].y, 0f);
		Vector3 pos2 = new Vector3(m_points[i2].x, m_points[i2].y, 0f);
		Vector3 v1 = pos1 - pos0;
		Vector3 v2 = pos2 - pos1;
		Vector3 crossVec = Vector3.Cross(v1, v2);
		if (Vector3.Dot(Camera.main.transform.position, crossVec) > 0) {
			reversFlag = false;
		}
		if (reversFlag) {
			indices.Reverse();
		}

		return indices.ToArray();
	}

	private bool Snip(int u, int v, int w, int n, int[] V, Vector3[] m_points) {
		int p;
		Vector2 A = m_points[V[u]];
		Vector2 B = m_points[V[v]];
		Vector2 C = m_points[V[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x)))) {
			return false;
		}
		for (p = 0; p < n; p++) {
			if ((p == u) || (p == v) || (p == w)) {
				continue;
			}
			Vector2 P = m_points[V[p]];
			if (InsideTriangle(A, B, C, P)) {
				return false;
			}
		}
		return true;
	}

	private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		float cCROSSap, bCROSScp, aCROSSbp;

		ax = C.x - B.x; ay = C.y - B.y;
		bx = A.x - C.x; by = A.y - C.y;
		cx = B.x - A.x; cy = B.y - A.y;
		apx = P.x - A.x; apy = P.y - A.y;
		bpx = P.x - B.x; bpy = P.y - B.y;
		cpx = P.x - C.x; cpy = P.y - C.y;

		aCROSSbp = ax * bpy - ay * bpx;
		cCROSSap = cx * apy - cy * apx;
		bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}
}