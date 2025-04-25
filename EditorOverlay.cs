using ClickableTransparentOverlay;
using ImGuiNET;
using LearnOpenTK.Common;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Collections.Generic;
using System.Numerics;
using Vector3N = System.Numerics.Vector3;
using Vector3TK = OpenTK.Mathematics.Vector3;

namespace U3DObjeto
{
    public class EditorOverlay : Overlay
    {
        private Escenario escenario;
        private int selectedIndex = -1;
        public int SelectedIndex => selectedIndex;

        private Vector3TK position = Vector3TK.Zero;
        private Vector3TK rotation = Vector3TK.Zero;
        private Vector3TK scale = Vector3TK.One;



        // Propiedades adicionales para la transformación (que no se han eliminado)
        private enum TransformMode { Move, Scale, Rotate }
        private TransformMode currentMode;

        // Cámara y ventana (si necesitas hacer algo relacionado)
        private Camera camera;
        private GameWindow window;

        public EditorOverlay(Escenario escenario, Camera camera, GameWindow window)
        {
            this.escenario = escenario;
            this.camera = camera;
            this.window = window;
        }

        protected override void Render()
        {
            ImGui.Begin("Editor de Objetos");

            var objetos = escenario.GetObjetos();
            for (int i = 0; i < objetos.Count; i++)
            {
                bool isSelected = selectedIndex == i;
                if (ImGui.Selectable($"Objeto {i}", isSelected))
                {
                    selectedIndex = i;
                    var obj = objetos[i];
                    position = obj.Position;
                    rotation = obj.Rotation;
                    scale = obj.Scale;
                }
            }

            if (selectedIndex >= 0 && selectedIndex < objetos.Count)
            {
                ImGui.Separator();
                ImGui.Text("Transformaciones:");

                var obj = objetos[selectedIndex];

                // Mostrar directamente la posición actual
                Vector3N pos = ConvertToNumerics(obj.Position);
                Vector3N rot = ConvertToNumerics(obj.Rotation);
                Vector3N scl = ConvertToNumerics(obj.Scale);

                // Modificables desde la UI
                if (ImGui.InputFloat3("Posición", ref pos))
                    obj.Position = ConvertToOpenTK(pos);

                if (ImGui.InputFloat3("Rotación", ref rot))
                    obj.Rotation = ConvertToOpenTK(rot);

                if (ImGui.InputFloat3("Escala", ref scl))
                    obj.Scale = ConvertToOpenTK(scl);

                if (ImGui.Button("Mover"))
                    currentMode = TransformMode.Move;

                if (ImGui.Button("Escalar"))
                    currentMode = TransformMode.Scale;

                if (ImGui.Button("Rotar"))
                    currentMode = TransformMode.Rotate;

                // Botón para eliminar el objeto seleccionado
                if (ImGui.Button("Eliminar Objeto"))
                {
                    objetos[selectedIndex].Cleanup();
                    objetos.RemoveAt(selectedIndex);
                    selectedIndex = -1;
                }


            }


            ImGui.End();
        }

        private Vector3N ConvertToNumerics(Vector3TK v) =>
            new Vector3N(v.X, v.Y, v.Z);

        private Vector3TK ConvertToOpenTK(Vector3N v) =>
            new Vector3TK(v.X, v.Y, v.Z);
    }
}
