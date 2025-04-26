using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Mathematics;

public static class ObjLoader
{
    public static (float[] vertices, uint[] indices) LoadObj(string filePath, float scaleFactor = 0.1f)
    {
        List<Vector3> tempVertices = new List<Vector3>();
        List<uint> tempIndices = new List<uint>();

        foreach (var line in File.ReadLines(filePath))
        {
            if (line.StartsWith("v ")) // Vértice
            {
                var parts = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                tempVertices.Add(new Vector3(
                    float.Parse(parts[1]),
                    float.Parse(parts[2]),
                    float.Parse(parts[3])
                ));
            }
            else if (line.StartsWith("f ")) // Cara
            {
                var parts = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                // OBJ usa índices basados en 0
                tempIndices.Add(uint.Parse(parts[1]));
                tempIndices.Add(uint.Parse(parts[2]));
                tempIndices.Add(uint.Parse(parts[3]));
            }
        }

        // Convertir a arrays planos
        float[] vertices = new float[tempVertices.Count * 3];
        for (int i = 0; i < tempVertices.Count; i++)
        {
            vertices[i*3] = tempVertices[i].X;
            vertices[i*3+1] = tempVertices[i].Y;
            vertices[i*3+2] = tempVertices[i].Z;
        }

        // Aplicar escala al cargar
        for (int i = 0; i < tempVertices.Count; i++)
        {
        vertices[i*3] = tempVertices[i].X * scaleFactor;
        vertices[i*3+1] = tempVertices[i].Y * scaleFactor;
        vertices[i*3+2] = tempVertices[i].Z * scaleFactor;
        }       

    //      // Después de cargar todo:
    // Console.WriteLine("=== VÉRTICES CARGADOS ===");
    // for (int i = 0; i < tempVertices.Count; i++)
    // {
    //     Console.WriteLine($"Vértice {i}: ({tempVertices[i].X}, {tempVertices[i].Y}, {tempVertices[i].Z})");
    // }

    // Console.WriteLine("\n=== ÍNDICES CARGADOS ===");
    // for (int i = 0; i < tempIndices.Count; i += 3)
    // {
    //     Console.WriteLine($"Triángulo {i/3}: {tempIndices[i]}, {tempIndices[i+1]}, {tempIndices[i+2]}");
    // }   

        return (vertices, tempIndices.ToArray());
    }
}