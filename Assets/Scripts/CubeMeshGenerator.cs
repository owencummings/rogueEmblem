using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomGeometry;

public class CubeMeshGenerator : MonoBehaviour
{
	void Start () {
		CreateCube();
		float randomClamp = UnityEngine.Random.Range(0.0f, 1.0f);
		if (randomClamp < 0.1f){
			ClampMeshTopXZ();
		}
	}

	// TODO: Move this to CustomGeometry namespace, add resolution as param
	// TODO: Use some mappings to reduce repeat code
	private void CreateCube () {
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> nextVerts = new List<Vector3>();
		List<int> nextTris = new List<int>();

		// Top face
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), 10);
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.up * 0.5f;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);

		// Bottom
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), 10);
		Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(180, 0, 0));
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] = rot.MultiplyPoint3x4(nextVerts[i]);
		}
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.down * 0.5f;
		}
		for (int i=0; i<nextTris.Count; i++){
			nextTris[i] += vertices.Count;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);


		// Side1
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), 10);
		rot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] = rot.MultiplyPoint3x4(nextVerts[i]);
		}
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.forward * 0.5f;
		}
		for (int i=0; i<nextTris.Count; i++){
			nextTris[i] += vertices.Count;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);

		// Side2
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), 10);
		rot = Matrix4x4.Rotate(Quaternion.Euler(270, 0, 0));
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] = rot.MultiplyPoint3x4(nextVerts[i]);
		}
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.back * 0.5f;
		}
		for (int i=0; i<nextTris.Count; i++){
			nextTris[i] += vertices.Count;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);

		// Side3
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), 10);
		rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] = rot.MultiplyPoint3x4(nextVerts[i]);
		}
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.left * 0.5f;
		}
		for (int i=0; i<nextTris.Count; i++){
			nextTris[i] += vertices.Count;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);

		// Side4
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), 10);
		rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] = rot.MultiplyPoint3x4(nextVerts[i]);
		}
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.right * 0.5f;
		}
		for (int i=0; i<nextTris.Count; i++){
			nextTris[i] += vertices.Count;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);

		Vector3[] vertArray = vertices.ToArray();
		int[] triangleArray = triangles.ToArray();	



		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = vertArray;
		mesh.triangles = triangleArray;
		mesh.Optimize();
		mesh.RecalculateNormals();
	}

	private void ClampMeshTopXZ(){
		// This screws up the triangles
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] verts = mesh.vertices;
		for (int i=0; i<verts.Length; i++){
			if (verts[i].y > 0.35f){
				verts[i] = new Vector3(Mathf.Clamp(verts[i].x, -0.4f, 0.4f),
									verts[i].y, 
									Mathf.Clamp(verts[i].z, -0.4f, 0.4f));
				}
		}
		mesh.vertices = verts;
		mesh.Optimize();
		mesh.RecalculateNormals();
	}

}
