//Quelle: https://www.youtube.com/watch?v=1HV8GbFnCik

//Library imports
using System;                           // fundamental system functionality
using System.Collections;               // interfaces and classes
using System.Collections.Generic;       // generic versions of the interfaces and classes
using System.Diagnostics;               // interaction with system processes, event logs, and performance counters
using UnityEngine;                      // core Unity functionality
using UnityEngine.Profiling;            // Unity's built-in performance profiler

//MonoBehaviour functionality: can be used as a component in a Unity GameObject
public class qaure : MonoBehaviour
{
    //public variables: can be accessed and modified from outside the class
    public int mDivisions;
    public float mSize;
    public float mHeight;

    //Vector3 array: stores the vertices of the terrain mesh
    Vector3[] mVerts;
    //stores the number of vertices in the terrain mesh
    int mVertCount;

    void Start()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();         //Variable "stopwatch": New Stopwatch object is created and started
        long memoryBefore = GC.GetTotalMemory(false);       //Variable "memoryBefore": Total memory usage in bytes before the terrain is created
        Profiler.BeginSample("Diamond Square Algorithm");   //Start of performance profiler sample

        //CreateTerrain method is called at the start
        CreateTerrain();

        stopwatch.Stop();                                   //Stops stopwatch
        long memoryAfter = GC.GetTotalMemory(false);        //Variable "memoryAfter": Total memory usage in bytes after the terrain is created
        long memoryUsed = memoryAfter - memoryBefore;       //Calculating the memory: Memory usage before - memory usage after.
        Profiler.EndSample();                               //Ends performance profiler sample
        
        UnityEngine.Debug.Log($"Memory used: {memoryUsed} bytes");
        UnityEngine.Debug.Log($"Time elapsed: {stopwatch.ElapsedMilliseconds} milliseconds");
        UnityEngine.Debug.Log("Finished");
    }

    //CreateTerrain method
    void CreateTerrain()
    { 
        mVertCount = (mDivisions + 1) * (mDivisions + 1);   //Calculate the total number of vertices in the terrain mesh by multiplying the number of divisions by itself and adding 1.
        mVerts = new Vector3[mVertCount];                   //Create a new Vector3 array with the number of elements equal to mVertCount and assigns it to mVerts
        Vector2[] uvs = new Vector2[mVertCount];            //Create a new Vector2 array with the number of elements equal to mVertCount and assigns it to "uvs"
        int[] tris = new int[mDivisions * mDivisions * 6];  //Create a new int array with the number of elements equal to the total number of triangles in the terrain mesh and assigns it to "tris"

        float halfSize = mSize * 0.5f;                      //Variable "halfSize": Calculate the half of the value of mSize
        float divisionSize = mSize / mDivisions;            //Variable "divisionSize": Calculate the size of each division by dividing the value of mSize by the value of mDivisions

        //Create a new Mesh object and gets the MeshFilter component
        Mesh mesh = new Mesh();                         
        GetComponent<MeshFilter>().mesh = mesh;

        int triOffset = 0;

        //For-Loop for iterating the number of divisions
        for(int i = 0; i <= mDivisions; i++) {
            for(int j = 0; j <= mDivisions; j++) {
                //Set the position of the vertex
                mVerts[i * (mDivisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                //Set the UV coordinate of the vertex
                uvs[i * (mDivisions + 1) + j] = new Vector2((float) i / mDivisions, (float) j / mDivisions);
                //Check if the current iteration of both for-loops is not at the last row or column.
                if(i < mDivisions && j < mDivisions){
                    int topLeft = i * (mDivisions + 1)+j;       //Calculate the index of the top-left vertex of the current square.
                    int botLeft = (i + 1)*(mDivisions + 1)+j;   //Calculate the index of the bottom-left vertex of the current square.

                    tris[triOffset] = topLeft;                  //Set first vertex of the first triangle of the current square to the top-left vertex
                    tris[triOffset + 1] = topLeft + 1;          //Set second vertex of the first triangle of the current square to the vertex to the right of the top-left vertex
                    tris[triOffset + 2] = botLeft + 1;          //Set third vertex of the first triangle of the current square to the vertex below and to the right of the bottom-left vertex
                    tris[triOffset + 3] = topLeft;              //Set first vertex of the second triangle of the current square to the top-left vertex
                    tris[triOffset + 4] = botLeft + 1;          //Set second vertex of the second triangle of the current square to the vertex below and to the right of the bottom-left vertex
                    tris[triOffset + 5] = botLeft;              //Set third vertex of the second triangle of the current square to the bottom-left vertex
                    
                    //Each square is made up of 2 triangles with 3 vertices each
                    triOffset += 6;                             //--> Increment the triOffset variable by 6 
                }
            }
        }
        //Set the y-coordinate of the top-left/right and bottom-left/right vertex to a random value between -mHeight and mHeight:
        mVerts[0].y = UnityEngine.Random.Range(-mHeight, mHeight);
        mVerts[mDivisions].y = UnityEngine.Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1].y = UnityEngine.Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1 -mDivisions].y = UnityEngine.Random.Range(-mHeight, mHeight);

        //Calculate the number of iterations needed for the Diamond-Square algorithm
        int iterations = (int)Mathf.Log(mDivisions, 2);         //log base 2 of the number of divisions
        int numSquares = 1;
        int squareSize = mDivisions;

        //Iterate the number of iterations
        for(int i = 0; i < iterations; i++){
            int row = 0;
            for(int j = 0; j < numSquares; j++){                //Iterate the number of squares
                int col = 0;
                for(int k = 0; k < numSquares; k++){
                    //Call the DiamondSquare method and pass arguments
                    DiamondSquare(row, col, squareSize, mHeight);
                    col += squareSize;
                }
                row += squareSize;
            }
            numSquares *= 2;
            squareSize /= 2;
            mHeight *= 0.5f;
        }
        mesh.vertices = mVerts;                                 //Set the vertices of the mesh to the mVerts array
        mesh.uv = uvs;                                          //Set the uv coordinates of the mesh to the uvs array
        mesh.triangles = tris;                                  //Set the triangles of the mesh to the tris array

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();    
    }

    //
    void DiamondSquare(int row, int col, int size, float offset) {
        int halfSize = (int)(size * 0.5f);
        int topLeft = row * (mDivisions + 1) + col;
        int botLeft = (row + size) * (mDivisions + 1) + col;

        int mid = (int)(row + halfSize) * (mDivisions + 1) + (int)(col + halfSize);
        mVerts[mid].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[botLeft].y + mVerts[botLeft + size].y) * 0.25f + UnityEngine.Random.Range(-offset, offset); 

        //
        mVerts[topLeft + halfSize].y = (mVerts[topLeft].y + mVerts[topLeft + size].y + mVerts[mid].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        mVerts[mid - halfSize].y = (mVerts[topLeft].y + mVerts[botLeft].y + mVerts[mid].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        mVerts[mid + halfSize].y = (mVerts[topLeft+size].y + mVerts[botLeft + size].y + mVerts[mid].y) / 3 + UnityEngine.Random.Range(-offset, offset);
        mVerts[botLeft + halfSize].y = (mVerts[botLeft].y + mVerts[botLeft + size].y + mVerts[mid].y) / 3 + UnityEngine.Random.Range(-offset, offset);
    }
}
