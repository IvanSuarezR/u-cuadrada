using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;



namespace U3DObjeto
{
    // Clase que encapsula un objeto 3D
    public class Objeto3D
    {
        public Vector3 Position { get; set; }
        public Vector3 CentroMasa { get; set; }
        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public Vector3 Color { get; set; }
        //public Quaternion Rotation { get; set; } = Quaternion.Identity; // Nueva propiedad para rotación

        private int vao, vbo, ebo;

        // Constructores (actualizados para incluir rotación opcional)
        public Objeto3D(Vector3 position, Vector3 CentroM, float[] vertices, uint[] indices, Vector3 color, Quaternion? rotation = null)
        {
            Position = position;
            CentroMasa = CentroM;
            Vertices = vertices;
            Indices = indices;
            Color = color;
            //Rotation = rotation ?? Quaternion.Identity;
        }

        public Objeto3D(Vector3 position, Vector3 color, float[] vertices, uint[] indices, Quaternion? rotation = null)
        {
            Position = position;
            Color = color;
            Vertices = vertices;
            Indices = indices;
            //Rotation = rotation ?? Quaternion.Identity;
        }

        // Resto de métodos sin cambios hasta GetModelMatrix()

        
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
        return Matrix4.CreateTranslation(Position-CentroMasa); 
    
        //   * Matrix4.CreateFromQuaternion(Rotation) * 
        //   Matrix4.CreateTranslation(-CentroMasa);
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