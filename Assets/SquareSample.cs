using UnityEngine;
using System.Collections;

public class SquareSample : MonoBehaviour {

	private GameObject obj;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Material material;
	private Vector3[] Vert;

	void Start() {
		initPlane();
	}

	void Update() {

		//それぞれの頂点を適当に動かす為の座標を決めて
		Vert[0] = new Vector3(Mathf.Sin(Time.frameCount * 0.05f), 0.5f, 0.0f);
		Vert[1] = new Vector3(Mathf.Sin(Time.frameCount * -0.05f), -0.5f, 0.0f);
		Vert[2] = new Vector3(Mathf.Sin(Time.frameCount * -0.05f), 0.5f, 0.0f);
		Vert[3] = new Vector3(Mathf.Sin(Time.frameCount * 0.05f), -0.5f, 0.0f);
		//meshに数値突っ込んで変化を確認
		meshFilter.mesh.vertices = Vert;

	}

	void initPlane() {
		//
		obj = new GameObject("BuildPlane");
		meshFilter = obj.AddComponent<MeshFilter>();
		meshRenderer = obj.AddComponent<MeshRenderer>();
		material = meshRenderer.material;

		Mesh m = meshFilter.mesh;
		m.name = "newMesh";

		m.Clear();

		Vert = new Vector3[]{ //Objectの原点を0,0,0として
new Vector3( 0.5f, 0.5f, 0.0f), //0　右上の頂点位置
new Vector3(-0.5f,-0.5f, 0.0f), //1　左下の頂点位置
new Vector3(-0.5f, 0.5f, 0.0f), //2　左上の頂点位置
new Vector3( 0.5f,-0.5f, 0.0f) //3　右下の頂点位置
};
		m.vertices = Vert;

		int[] triangles = new int[]{
0, 1, 2, //0,1,2の三角　右上＞左下＞左上の三角　時計回り側がポリゴン正面
3, 1, 0 //3,1,0の三角　右下＞左下＞右上の三角　時計回り側がポリゴン正面
};
		m.triangles = triangles;

		Vector2[] uv = new Vector2[]{//UVは左下原点が0,0
new Vector2(1.0f,1.0f), // 右上の頂点のUV位置　1,1
new Vector2(0.0f,0.0f), // 左下の頂点のUV位置　0,0
new Vector2(0.0f,1.0f), // 左上の頂点のUV位置　0,1
new Vector2(1.0f,0.0f) // 右下の頂点のUV位置　1,0
};
		m.uv = uv;

		//適当にMaterialを設定
		material = new Material(Shader.Find("Standard"));
		material.color = Color.white;
	}

}