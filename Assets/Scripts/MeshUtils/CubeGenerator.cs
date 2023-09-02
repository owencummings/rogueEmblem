using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomGeometry {
    static class CubeGenerator{
        public static void CreateCube(Mesh mesh) {
            int density = 2;
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();

            // Create each face
            CreateTop(vertices, triangles, density);
            CreateBottom(vertices, triangles, density);
            CreateForward(vertices, triangles, density);
            CreateBack(vertices, triangles, density);
            CreateLeft(vertices, triangles, density);
            CreateRight(vertices, triangles, density);

            Vector3[] vertArray = vertices.ToArray();
            int[] triangleArray = triangles.ToArray();

            RenderMesh(mesh, vertArray, triangleArray);
        }

        // TODO: generalize these plane-creation methods
        public static void CreateTop(List<Vector3> vertices, List<int> triangles, int density)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
            for (int i=0; i<nextVerts.Count; i++){
                nextVerts[i] += Vector3.up * 0.5f;
            }
            for (int i=0; i<nextTris.Count; i++){
                nextTris[i] += vertices.Count;
            }
            vertices.AddRange(nextVerts);
            triangles.AddRange(nextTris);
        }

        public static void CreateTop(List<Vector3> vertices, List<int> triangles, int density, Vector3 offset)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
            for (int i=0; i<nextVerts.Count; i++){
                nextVerts[i] += Vector3.right * offset.x + Vector3.up * offset.y + Vector3.forward * offset.z;
            }
            for (int i=0; i<nextTris.Count; i++){
                nextTris[i] += vertices.Count;
            }
            vertices.AddRange(nextVerts);
            triangles.AddRange(nextTris);
        }

        public static void CreateBottom(List<Vector3> vertices, List<int> triangles, int density){
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
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
        }

        public static void CreateForward(List<Vector3> vertices, List<int> triangles, int density)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
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
        }

        public static void CreateBack(List<Vector3> vertices, List<int> triangles, int density)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(270, 0, 0));
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
        }

        public static void CreateLeft(List<Vector3> vertices, List<int> triangles, int density)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
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
        }

        public static void CreateRight(List<Vector3> vertices, List<int> triangles, int density)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateQuad(new Vector2(1,1), density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));
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
        }

        public static void ClampMeshTopXZ(Mesh mesh)
        {
            // This screws up the triangles
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
    
        public static void ShrinkMeshTop(Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            for (int i=0; i<verts.Length; i++)
            {
                if (verts[i].y > 0f){
                    verts[i][1] *= 0.9f;
                }
            }
            mesh.vertices = verts;
            mesh.Optimize();
            mesh.RecalculateNormals();
        }

        public static void RenderMesh(Mesh mesh, Vector3[] vertices, int[] triangles)
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.Optimize();
            mesh.RecalculateNormals();
        }
    
    
    }

    /*
    static class FixedCubeGenerator
    {
        public static void CreateTop(List<Vector3Int> vertices, List<int> triangles, int density)
        {
            List<Vector3Int> nextVerts = new List<Vector3Int>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateFixedQuad(density);
            for (int i=0; i<nextVerts.Count; i++){
                nextVerts[i] += Vector3Int.up * density;
            }
            for (int i=0; i<nextTris.Count; i++){
                nextTris[i] += vertices.Count;
            }
            vertices.AddRange(nextVerts);
            triangles.AddRange(nextTris);
        }

        public static void CreateBottom(List<Vector3Int> vertices, List<int> triangles, int density){
            List<Vector3Int> nextVerts = new List<Vector3Int>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateFixedQuad(density);
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
        }

        public static void CreateForward(List<Vector3Int> vertices, List<int> triangles, int density)
        {
            List<Vector3Int> nextVerts = new List<Vector3Int>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateFixedQuad(density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(90, 0, 0));
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
        }

        public static void CreateBack(List<Vector3Int> vertices, List<int> triangles, int density)
        {
            List<Vector3Int> nextVerts = new List<Vector3Int>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateFixedQuad(density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(270, 0, 0));
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
        }

        public static void CreateLeft(List<Vector3Int> vertices, List<int> triangles, int density)
        {
            List<Vector3Int> nextVerts = new List<Vector3Int>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateFixedQuad(density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90));
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
        }

        public static void CreateRight(List<Vector3Int> vertices, List<int> triangles, int density)
        {
            List<Vector3> nextVerts = new List<Vector3>();
            List<int> nextTris = new List<int>();
            (nextVerts, nextTris) = QuadGenerator.GenerateFixedQuad(density);
            Matrix4x4 rot = Matrix4x4.Rotate(Quaternion.Euler(0, 0, 270));
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
        }

    }
    */
}
