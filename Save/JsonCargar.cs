using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using OpenTK.Mathematics;
using OpenTKProject.objetos;

namespace OpenTKProject.Save
{
    public static class JsonEscenario
    {
        private static string jsonPath = "escenario.json";

        public static void Guardar(List<Objeto3D> objetos)
        {
            var dtos = new List<Objeto3DDTO>();
            foreach (var obj in objetos)
                dtos.Add(new Objeto3DDTO(obj));

            var opciones = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(dtos, opciones);
            File.WriteAllText(jsonPath, json);
        }

        public static List<Objeto3D> Cargar()
        {
            if (!File.Exists(jsonPath)) return new List<Objeto3D>();

            var json = File.ReadAllText(jsonPath);
            var dtos = JsonSerializer.Deserialize<List<Objeto3DDTO>>(json);

            var objetos = new List<Objeto3D>();
            foreach (var dto in dtos!)
                objetos.Add(dto.ToObjeto3D());

            return objetos;
        }
    }
}
