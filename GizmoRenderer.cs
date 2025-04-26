using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class GizmoRenderer
{
    private readonly int vao, vbo;
    private readonly LineShader lineShader;
    private int circleVbo;
    private int circleVao;

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

        circleVao = GL.GenVertexArray();
        circleVbo = GL.GenBuffer();

        GL.BindVertexArray(circleVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, circleVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * 100, IntPtr.Zero, BufferUsageHint.DynamicDraw); // reserva espacio

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);
        GL.BindVertexArray(0);
    }

    public void Render(Matrix4 view, Matrix4 projection, Matrix4 model, int hoveredAxis, string mode)
    {
        if (mode == "mover")
        {
            RenderTranslateGizmo(view, projection, model, hoveredAxis);
        }
        else if (mode == "rotar")
        {
            RenderRotateGizmo(view, projection, model, hoveredAxis);
        }
        else if (mode == "escalar")
        {
            RenderScaleGizmo(view, projection, model, hoveredAxis);
        }
    }

    private void RenderTranslateGizmo(Matrix4 view, Matrix4 projection, Matrix4 model, int hoveredAxis)
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

    private void RenderRotateGizmo(Matrix4 view, Matrix4 projection, Matrix4 model, int hoveredAxis)
    {
        const int segments = 64;
        const float radius = 1.5f;

        for (int axis = 0; axis < 3; axis++)
        {
            Vector3 color = axis switch
            {
                0 => hoveredAxis == 0 ? new Vector3(1f, 0.5f, 0.5f) : new Vector3(1f, 0f, 0f), // X
                1 => hoveredAxis == 1 ? new Vector3(0.5f, 1f, 0.5f) : new Vector3(0f, 1f, 0f), // Y
                _ => hoveredAxis == 2 ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0f, 0f, 1f)  // Z
            };

            Vector3[] circle = new Vector3[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float angle = MathHelper.TwoPi * i / segments;
                float x = radius * MathF.Cos(angle);
                float y = radius * MathF.Sin(angle);

                circle[i] = axis switch
                {
                    0 => new Vector3(0, x, y), // X (YZ)
                    1 => new Vector3(x, 0, y), // Y (XZ)
                    _ => new Vector3(x, y, 0)  // Z (XY)
                };
            }

            Vector3[] transformed = new Vector3[circle.Length];
            for (int i = 0; i < circle.Length; i++)
            {
                Vector4 v = new Vector4(circle[i], 1.0f);
                v = v * model;
                transformed[i] = v.Xyz;
            }

            // Cargar vértices al buffer dinámico
            GL.BindBuffer(BufferTarget.ArrayBuffer, circleVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * transformed.Length, transformed, BufferUsageHint.DynamicDraw);

            GL.BindVertexArray(circleVao);
            lineShader.Use(Matrix4.Identity, view, projection, color);
            GL.DrawArrays(PrimitiveType.LineStrip, 0, transformed.Length);
            GL.BindVertexArray(0);
        }
    }

    private void RenderScaleGizmo(Matrix4 view, Matrix4 projection, Matrix4 model, int hoveredAxis)
    {
        const float scaleSize = 1.5f;

        // Definir los ejes del gizmo de escala
        Vector3[] scaleGizmo = new Vector3[]
        {
        new Vector3(0f, 0f, 0f), new Vector3(scaleSize, 0f, 0f), // Eje X
        new Vector3(0f, 0f, 0f), new Vector3(0f, scaleSize, 0f), // Eje Y
        new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, scaleSize), // Eje Z
        };

        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * scaleGizmo.Length, scaleGizmo, BufferUsageHint.StaticDraw);

        // Eje X
        var colorX = hoveredAxis == 0 ? new Vector3(1f, 0.5f, 0.5f) : new Vector3(1f, 0f, 0f); // rojo
        lineShader.Use(model, view, projection, colorX);
        GL.DrawArrays(PrimitiveType.Lines, 0, 2);

        // Eje Y
        var colorY = hoveredAxis == 1 ? new Vector3(0.5f, 1f, 0.5f) : new Vector3(0f, 1f, 0f); // verde
        lineShader.Use(model, view, projection, colorY);
        GL.DrawArrays(PrimitiveType.Lines, 2, 2);

        // Eje Z
        var colorZ = hoveredAxis == 2 ? new Vector3(0.5f, 0.5f, 1f) : new Vector3(0f, 0f, 1f); // azul
        lineShader.Use(model, view, projection, colorZ);
        GL.DrawArrays(PrimitiveType.Lines, 4, 2);

        GL.BindVertexArray(0);
    }


    public void DrawDebugLine(Vector3 start, Vector3 end, Vector3 color, Matrix4 view, Matrix4 projection)
    {
        Vector3[] vertices = new Vector3[] { start, end };

        int tempVao = GL.GenVertexArray();
        int tempVbo = GL.GenBuffer();

        GL.BindVertexArray(tempVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, tempVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, Vector3.SizeInBytes * vertices.Length, vertices, BufferUsageHint.DynamicDraw);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vector3.SizeInBytes, 0);
        GL.EnableVertexAttribArray(0);

        GL.LineWidth(2f);
        lineShader.Use(Matrix4.Identity, view, projection, color);
        GL.DrawArrays(PrimitiveType.Lines, 0, 2);

        GL.BindVertexArray(0);
        GL.DeleteBuffer(tempVbo);
        GL.DeleteVertexArray(tempVao);
    }
}

