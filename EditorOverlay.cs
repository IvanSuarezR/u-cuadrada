using ClickableTransparentOverlay;
using ImGuiNET;
using LearnOpenTK.Common;
using OpenTK.Windowing.Desktop;
using Vector3N = System.Numerics.Vector3;
using Vector3TK = OpenTK.Mathematics.Vector3;

namespace U3DObjeto
{
    public class EditorOverlay : Overlay
    {
        private Escenario escenario;
        private int selectedIndex = -1;
        public int SelectedIndex => selectedIndex;

        private Vector3N editPos = Vector3N.Zero;
        private Vector3N editRot = Vector3N.Zero;
        private Vector3N editScale = Vector3N.One;

        private bool editingPos = false;
        private bool editingRot = false;
        private bool editingScale = false;

        private enum TransformMode { Ninguno, Mover, Escalar, Rotar }
        private TransformMode currentMode;
        public string ModoGizmo => currentMode.ToString().ToLower();

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
                    editPos = ConvertToNumerics(obj.Position);
                    editRot = ConvertToNumerics(obj.Rotation);
                    editScale = ConvertToNumerics(obj.Scale);
                    editingPos = editingRot = editingScale = false;
                }
            }

            if (selectedIndex >= 0 && selectedIndex < objetos.Count)
            {
                ImGui.Separator();
                ImGui.Text("Transformaciones:");

                var obj = objetos[selectedIndex];

                // Actualizar valores en tiempo real mientras no se está editando
                if (!editingPos)
                    editPos = ConvertToNumerics(obj.Position);
                if (!editingRot)
                    editRot = ConvertToNumerics(obj.Rotation);
                if (!editingScale)
                    editScale = ConvertToNumerics(obj.Scale);

                // Posición
                ImGui.PushID("Pos");
                ImGui.InputFloat3("Posición", ref editPos);
                if (ImGui.IsItemActivated()) editingPos = true;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    obj.Position = ConvertToOpenTK(editPos);
                    editingPos = false;
                }
                ImGui.PopID();

                // Rotación
                ImGui.PushID("Rot");
                ImGui.InputFloat3("Rotación", ref editRot);
                if (ImGui.IsItemActivated()) editingRot = true;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    obj.Rotation = ConvertToOpenTK(editRot);
                    editingRot = false;
                }
                ImGui.PopID();

                // Escala
                ImGui.PushID("Esc");
                ImGui.InputFloat3("Escala", ref editScale);
                if (ImGui.IsItemActivated()) editingScale = true;
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    obj.Scale = ConvertToOpenTK(editScale);
                    editingScale = false;
                }
                ImGui.PopID();

                if (ImGui.Button("Mover"))
                    currentMode = TransformMode.Mover;
                if (ImGui.Button("Escalar"))
                    currentMode = TransformMode.Escalar;
                if (ImGui.Button("Rotar"))
                    currentMode = TransformMode.Rotar;

                if (ImGui.Button("Eliminar Objeto"))
                {
                    objetos[selectedIndex].Cleanup();
                    objetos.RemoveAt(selectedIndex);
                    selectedIndex = -1;
                }

                ImGui.Text($"Modo actual: {ModoGizmo}");
            }

            ImGui.End();
        }

        private Vector3N ConvertToNumerics(Vector3TK v) =>
            new Vector3N(v.X, v.Y, v.Z);

        private Vector3TK ConvertToOpenTK(Vector3N v) =>
            new Vector3TK(v.X, v.Y, v.Z);
    }
}
