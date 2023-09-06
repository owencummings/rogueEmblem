using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomGeometry {
    static class QuadGenerator {
        public static (List<Vector3>, List<int>) GenerateQuad(Vector2 size, int resolution){
            List<Vector3> verts = new List<Vector3>();
            float xPerStep = size.x/resolution;
            float yPerStep = size.y/resolution;
            float xVal;
            float yVal;
            int counter;
            // Incrementally add instead of multiplying to reduce float-arithmetic inconsistencies.
            for(int y=0; y<resolution+1; y++){
                for (int x=0; x<resolution+1; x++){
                    xVal = 0;
                    yVal = 0;
                    counter = 0;
                    while (counter < x){ xVal += xPerStep; counter += 1; }
                    counter = 0;
                    while (counter < y){ yVal += yPerStep; counter += 1; }
                    verts.Add(new Vector3(xVal-size.x/2f, 0, yVal - size.y/2f));
                }
            }

            List<int> triangles = new List<int>();
            for (int row=0; row < resolution; row++){
                for (int column = 0; column<resolution; column++){
                    int i = (row*resolution) + row + column;
                    triangles.Add(i);
                    triangles.Add(i + resolution + 1);
                    triangles.Add(i + resolution + 2);
                    triangles.Add(i);
                    triangles.Add(i + resolution + 2);
                    triangles.Add(i + 1);
                }
            }
            return (verts, triangles);
        }

        public static (List<Vector3>, List<int>) GenerateDownQuad(Vector2 size, int resolution)
        {
            List<Vector3> verts = new List<Vector3>();
            float xPerStep = size.x/resolution;
            float yPerStep = size.y/resolution;
            float xVal;
            float yVal;
            int counter;
            // Incrementally add instead of multiplying to reduce float-arithmetic inconsistencies.
            for(int y=0; y<resolution+1; y++){
                for (int x=0; x<resolution+1; x++){
                    xVal = 0;
                    yVal = 0;
                    counter = 0;
                    while (counter < x){ xVal += xPerStep; counter += 1; }
                    counter = 0;
                    while (counter < y){ yVal += yPerStep; counter += 1; }
                    verts.Add(new Vector3(xVal-size.x/2f, 0, yVal - size.y/2f));
                }
            }

            // TODO figure out triangulation...
            List<int> triangles = new List<int>();
            for (int row=0; row < resolution; row++){
                for (int column = 0; column<resolution; column++){
                    int i = (row*resolution) + row + column;
                    triangles.Add(i);
                    triangles.Add(i + resolution + 1);
                    triangles.Add(i + resolution + 2);
                    triangles.Add(i);
                    triangles.Add(i + resolution + 2);
                    triangles.Add(i + 1);
                }
            }
            return (verts, triangles);
        }

        public static (List<Vector3Int>, List<int>) GenerateFixedQuad(int resolution)
        {
            List<Vector3Int> verts = new List<Vector3Int>();
            for(int y=0; y<resolution+1; y++){
                for (int x=0; x<resolution+1; x++){
                    verts.Add(new Vector3Int(x, 0, y));
                }
            }

            List<int> tris = new List<int>();
            for (int row=0; row < resolution; row++){
                for (int column = 0; column<resolution; column++){
                    int i = (row*resolution) + row + column;
                    tris.Add(i);
                    tris.Add(i + resolution + 1);
                    tris.Add(i + resolution + 2);
                    tris.Add(i);
                    tris.Add(i + resolution + 2);
                    tris.Add(i + 1);
                }
            }
            return (verts, tris);
        }

    }

    // TODO: Add resolution eventually? For mesh deforming.
    static class TriGenerator {
        public static (List<Vector3>, List<int>) GenerateTri(Vector2 size){
            List<Vector3> verts = new List<Vector3>();
            List<int> triangles = new List<int>();
            verts.Add(new Vector3( 0.5f, 0f,  0.5f));
            verts.Add(new Vector3( 0.5f, 0f, -0.5f));
            verts.Add(new Vector3(-0.5f, 0f, -0.5f));
            triangles.Add(0);
            triangles.Add(1);
            triangles.Add(2);
            return (verts, triangles);
        }
    }
}
