using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomGeometry;

public class RampMeshGenerator : MonoBehaviour
{
	void Start () {
		CreateRamp();

	}

    // TODO: Move this to CustomGeometry namespace, add resolution as param
    // TODO: Use some mappings to reduce repeat code
	private void CreateRamp () {
		int density = 2;
		List<Vector3> vertices = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector3> nextVerts = new List<Vector3>();
		List<int> nextTris = new List<int>();

		// Top face
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] += Vector3.up * -1f * nextVerts[i].z;
		}
		vertices.AddRange(nextVerts);
		triangles.AddRange(nextTris);

		// Bottom
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
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



        // It's possible we don't even need tris on the sides... and instead should make the cubes have some tri-sides instead of full square sides.
        // Gives a cool papercraft feel.
        /*
        // Side1
		(nextVerts, nextTris) = TriGenerator.GenerateTri(new Vector2(1,1));
		rot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
		Matrix4x4 rot2 = Matrix4x4.Rotate(Quaternion.Euler(0, 270, 0));
		for (int i=0; i<nextVerts.Count; i++){
			nextVerts[i] = rot2.MultiplyPoint3x4(nextVerts[i]);
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
        */

		// Side2
		(nextVerts, nextTris) = TriGenerator.GenerateTri(new Vector2(1,1));
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

		// Side4
		(nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
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

		Vector3[] vertArray = vertices.ToArray();
		int[] triangleArray = triangles.ToArray();	



		Mesh mesh = GetComponent<MeshFilter>().mesh;
		mesh.Clear();
		mesh.vertices = vertArray;
		mesh.triangles = triangleArray;
		mesh.Optimize();
		mesh.RecalculateNormals();
        if (this.TryGetComponent<MeshCollider>(out MeshCollider coll)){
            coll.sharedMesh = mesh;
        }
	}

}
