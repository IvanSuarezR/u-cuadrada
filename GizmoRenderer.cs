using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class GizmoRenderer
{
    private readonly int vao, vbo;
    private readonly LineShader lineShader;

    private readonly Vector3[] gizmoLines = new Vector3[]
    {
        // Línea eje X (rojo)
        new Vector3(0f, 0f, 0f), new Vector3(1.5f, 0f, 0f),
        // Línea eje Y (verde)
        new Vector3(0f, 0f, 0f), new Vector3(0f, 1.5f, 0f),
        // Línea eje Z (azul)
        new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1.5f),
    };

    public GizmoRenderer()
    {
        lineShader = new LineShader();

        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * gizmoLines.Length, gizmoLines, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }

    public void Render(Matrix4 view, Matrix4 projection, Matrix4 model, int hoveredAxis = -1)
    {
        GL.BindVertexArray(vao);
        GL.LineWidth(3f);

        // Eje X
        var colorX = hoveredAxis == 0 ? new Vector3(1f, 0.5f, 0.5f) : new Vector3(1f, 0f, 0f);
        lineShader.Use(model, view, projection, colorX);
        GL.DrawArrays(PrimitiveType.Lines, 0, 2);

        // Eje Y
        var colorY = hoveredAxis == 1 ? new Vector3(0.5f, 1f, 0.5f) : new Vector3(0f, 1f, 0f);
        lineShader.Use(model, view, projection, colorY);
        GL.DrawArrays(PrimitiveType.Lines, 2, 2);

        // Eje Z
        var colorZ = hoveredAxis == 2 ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0f, 0f, 1f);
        lineShader.Use(model, view, projection, colorZ);
        GL.DrawArrays(PrimitiveType.Lines, 4, 2);

        GL.BindVertexArray(0);
    }



}
