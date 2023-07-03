using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomGeometry;

public class CubeMeshGenerator : MonoBehaviour
{
	void Start () {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		CubeGenerator.CreateCube(mesh);
		float randomClamp = UnityEngine.Random.Range(0.0f, 1.0f);
		if (randomClamp < 0.1f){
			CubeGenerator.ClampMeshTopXZ(mesh);
		}
	}
}
