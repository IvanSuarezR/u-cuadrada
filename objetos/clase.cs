using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using System.Text.Json.Serialization;

namespace OpenTKProject.objetos
{
    public class Objeto3D
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero; // Rotación en grados (X, Y, Z)
        public Vector3 Scale { get; set; } = Vector3.One;
        public Vector3 CentroMasa { get; set; } = Vector3.Zero;

        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public Vector3 Color { get; set; } = Vector3.One;

        [JsonIgnore] private int vao, vbo, ebo;

        public Objeto3D() { }

        public Objeto3D(Vector3 position, Vector3 centroMasa, float[] vertices, uint[] indices, Vector3 color)
        {
            Position = position;
            CentroMasa = centroMasa;
            Vertices = vertices;
            Indices = indices;
            Color = color;
        }

        public void Initialize()
        {
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindVertexArray(0);
        }

        public Matrix4 GetModelMatrix()
        {
            var translationMatrix = Matrix4.CreateTranslation(Position - CentroMasa);
            var rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X));
            var rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y));
            var rotationZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z));
            var scaleMatrix = Matrix4.CreateScale(Scale);

            // Escala -> Rotación -> Traslación
            return scaleMatrix * rotationZ * rotationY * rotationX * translationMatrix;
        }

        public void Draw()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
        }
    }
}
