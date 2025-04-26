using OpenTK.Mathematics;
using OpenTKProject.objetos;

namespace OpenTKProject.Save
{
    public class Vector3Serializable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3Serializable() { }

        public Vector3Serializable(Vector3 v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(X, Y, Z);
        }
    }

    public class Objeto3DDTO
    {
        public Vector3Serializable Position { get; set; }
        public Vector3Serializable Rotation { get; set; } // Nueva propiedad
        public Vector3Serializable Scale { get; set; } // Nueva propiedad
        public Vector3Serializable CentroMasa { get; set; }
        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public Vector3Serializable Color { get; set; }

        public Objeto3DDTO() { }

        // Constructor que recibe un objeto Objeto3D y lo convierte a DTO
        public Objeto3DDTO(Objeto3D obj)
        {
            Position = new Vector3Serializable(obj.Position);
            Rotation = new Vector3Serializable(obj.Rotation); // Asignamos Rotation
            Scale = new Vector3Serializable(obj.Scale); // Asignamos Scale
            CentroMasa = new Vector3Serializable(obj.CentroMasa);
            Vertices = obj.Vertices;
            Indices = obj.Indices;
            Color = new Vector3Serializable(obj.Color);
        }

        // Método para convertir DTO de vuelta a Objeto3D
        public Objeto3D ToObjeto3D()
        {
            return new Objeto3D(
                Position.ToVector3(),
                CentroMasa.ToVector3(),
                Vertices,
                Indices,
                Color.ToVector3())
            {
                Rotation = Rotation.ToVector3(), // Asignamos Rotation
                Scale = Scale.ToVector3() // Asignamos Scale
            };
        }
    }
}
