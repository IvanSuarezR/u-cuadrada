using ClickableTransparentOverlay;
using ImGuiNET;
using LearnOpenTK.Common;
using OpenTK.Windowing.Desktop;
using OpenTKProject.objetos;
using Vector3N = System.Numerics.Vector3;
using Vector3TK = OpenTK.Mathematics.Vector3;
using Vector2TK = OpenTK.Mathematics.Vector2;

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

        private Vector3N prevEditPos = Vector3N.Zero;
        private Vector3N prevEditRot = Vector3N.Zero;
        private Vector3N prevEditScale = Vector3N.One;

        private enum TransformMode { Ninguno, Mover, Escalar, Rotar }
        private TransformMode currentMode;
        public string ModoGizmo => currentMode.ToString().ToLower();

        private Camera camera;
        private GameWindow window;

        private bool draggingPos = false;
        private Vector2TK previousMousePos = Vector2TK.Zero;

        public EditorOverlay(Escenario escenario, Camera camera, GameWindow window)
        {
            this.escenario = escenario;
            this.camera = camera;
            this.window = window;
        }

        protected override void Render()
        {
            ImGui.Begin("Editor de Escenario");

            bool escenarioSeleccionado = selectedIndex == -1;

            if (ImGui.TreeNode("Escenario"))
            {
                if (ImGui.Selectable("Transformar Escenario", escenarioSeleccionado))
                {
                    selectedIndex = -1;
                    editPos = Vector3N.Zero;
                    editRot = Vector3N.Zero;
                    editScale = Vector3N.One;
                    editingPos = editingRot = editingScale = false;
                }

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

                ImGui.TreePop();
            }

            // Mostrar propiedades del escenario seleccionado
            if (escenarioSeleccionado)
            {

                ImGui.Separator();
                ImGui.Text("Transformar todos los objetos:");

                var objetos = escenario.GetObjetos();

                static void ApplyDeltaToAll(List<Objeto3D> objs, Vector3N delta, Func<Objeto3D, Vector3TK> getter, Action<Objeto3D, Vector3TK> setter)
                {
                    foreach (var obj in objs)
                    {
                        var current = getter(obj);
                        setter(obj, new Vector3TK(
                            current.X + delta.X,
                            current.Y + delta.Y,
                            current.Z + delta.Z
                        ));
                    }
                }

                static Vector3N InputFloat3Delta(string label, ref Vector3N current, ref Vector3N previous)
                {
                    Vector3N temp = current;
                    if (ImGui.InputFloat3(label, ref temp))
                    {
                        Vector3N delta = temp - previous;
                        previous = temp;
                        current = temp;
                        return delta;
                    }
                    return Vector3N.Zero;
                }

                // POSICIÓN
                Vector3N deltaPos = InputFloat3Delta("Posición", ref editPos, ref prevEditPos);
                if (deltaPos != Vector3N.Zero)
                {
                    ApplyDeltaToAll(objetos, deltaPos, o => o.Position, (o, v) => o.Position = v);
                }

                // ROTACIÓN
                Vector3N deltaRot = InputFloat3Delta("Rotación", ref editRot, ref prevEditRot);
                if (deltaRot != Vector3N.Zero)
                {
                    ApplyDeltaToAll(objetos, deltaRot, o => o.Rotation, (o, v) => o.Rotation = v);
                }

                // ESCALA
                Vector3N deltaScale = InputFloat3Delta("Escala", ref editScale, ref prevEditScale);
                if (deltaScale != Vector3N.Zero)
                {
                    ApplyDeltaToAll(objetos, deltaScale, o => o.Scale, (o, v) => o.Scale = v);
                }


                if (objetos.Count > 0)
                {
                    // Posición (transformación acumulativa)
                    Vector3N deltaPos3 = InputFloat3Delta("Posición", ref editPos, ref prevEditPos);
                    if (deltaPos3 != Vector3N.Zero)
                    {
                        ApplyDeltaToAll(objetos, deltaPos3, o => o.Position, (o, v) => o.Position = v);
                    }

                    ImGui.PushID("GroupPos");
                    // Cuadro de Posición X
                    if (ImGui.InputFloat("Posición X", ref editPos.X, 0.1f, 1f))
                    {
                        Vector3N deltaX = new Vector3N(editPos.X - prevEditPos.X, 0, 0);
                        ApplyDeltaToAll(objetos, deltaX, o => o.Position, (o, v) => o.Position = v);
                        //prevEditPos.X = editPos.X; // Actualiza el valor previo
                    }

                    // Cuadro de Posición Y
                    if (ImGui.InputFloat("Posición Y", ref editPos.Y, 0.1f, 1f))
                    {
                        Vector3N deltaY = new Vector3N(0, editPos.Y - prevEditPos.Y, 0);
                        ApplyDeltaToAll(objetos, deltaY, o => o.Position, (o, v) => o.Position = v);
                        //prevEditPos.Y = editPos.Y; // Actualiza el valor previo
                    }

                    // Cuadro de Posición Z
                    if (ImGui.InputFloat("Posición Z", ref editPos.Z, 0.1f, 1f))
                    {
                        Vector3N deltaZ = new Vector3N(0, 0, editPos.Z - prevEditPos.Z);
                        ApplyDeltaToAll(objetos, deltaZ, o => o.Position, (o, v) => o.Position = v);
                        //prevEditPos.Z = editPos.Z; // Actualiza el valor previo
                    }
                    ImGui.PopID();

                    // Rotación (transformación acumulativa)
                    ImGui.PushID("GroupRot");
                    // Cuadro de Rotación X
                    if (ImGui.InputFloat("Rotación X", ref editRot.X, 2f, 2f))
                    {
                        Vector3N deltaRotX = new Vector3N(editRot.X - prevEditRot.X, 0, 0);
                        ApplyDeltaToAll(objetos, deltaRotX, o => o.Rotation, (o, v) =>
                        {
                            // Acumular la rotación en X
                            o.Rotation = ConvertToOpenTK(ConvertToNumerics(o.Rotation) + deltaRotX);
                        });
                        //prevEditRot.X = editRot.X; // Actualiza el valor previo
                    }

                    // Cuadro de Rotación Y
                    if (ImGui.InputFloat("Rotación Y", ref editRot.Y, 2f, 2f))
                    {
                        Vector3N deltaRotY = new Vector3N(0, editRot.Y - prevEditRot.Y, 0);
                        ApplyDeltaToAll(objetos, deltaRotY, o => o.Rotation, (o, v) =>
                        {
                            // Acumular la rotación en Y
                            o.Rotation = ConvertToOpenTK(ConvertToNumerics(o.Rotation) + deltaRotY);
                        });
                        //prevEditRot.Y = editRot.Y; // Actualiza el valor previo
                    }

                    // Cuadro de Rotación Z
                    if (ImGui.InputFloat("Rotación Z", ref editRot.Z, 2f, 2f))
                    {
                        Vector3N deltaRotZ = new Vector3N(0, 0, editRot.Z - prevEditRot.Z);
                        ApplyDeltaToAll(objetos, deltaRotZ, o => o.Rotation, (o, v) =>
                        {
                            // Acumular la rotación en Z
                            o.Rotation = ConvertToOpenTK(ConvertToNumerics(o.Rotation) + deltaRotZ);
                        });
                        //prevEditRot.Z = editRot.Z; // Actualiza el valor previo
                    }
                    ImGui.PopID();

                    // Escala (transformación acumulativa)
                    Vector3N deltaScale2 = InputFloat3Delta("Escala", ref editScale, ref prevEditScale);
                    if (deltaScale2 != Vector3N.Zero)
                    {
                        ApplyDeltaToAll(objetos, deltaScale2, o => o.Scale, (o, v) => o.Scale = v);
                    }

                    ImGui.PushID("GroupScale");
                    // Cuadro de Escala X
                    if (ImGui.InputFloat("Escala X", ref editScale.X, 0.1f, 1f))
                    {
                        Vector3N deltaScaleX = new Vector3N(editScale.X - prevEditScale.X, 0, 0);
                        ApplyDeltaToAll(objetos, deltaScaleX, o => o.Scale, (o, v) => o.Scale = v);
                    //    prevEditScale.X = editScale.X; // Actualiza el valor previo
                    }

                    // Cuadro de Escala Y
                    if (ImGui.InputFloat("Escala Y", ref editScale.Y, 0.1f, 1f))
                    {
                        Vector3N deltaScaleY = new Vector3N(0, editScale.Y - prevEditScale.Y, 0);
                        ApplyDeltaToAll(objetos, deltaScaleY, o => o.Scale, (o, v) => o.Scale = v);
                     //   prevEditScale.Y = editScale.Y; // Actualiza el valor previo
                    }

                    // Cuadro de Escala Z
                    if (ImGui.InputFloat("Escala Z", ref editScale.Z, 0.1f, 1f))
                    {
                        Vector3N deltaScaleZ = new Vector3N(0, 0, editScale.Z - prevEditScale.Z);
                        ApplyDeltaToAll(objetos, deltaScaleZ, o => o.Scale, (o, v) => o.Scale = v);
                   //     prevEditScale.Z = editScale.Z; // Actualiza el valor previo
                    }
                    ImGui.PopID();
                }




                else
                {
                    ImGui.Text("No hay objetos en el escenario.");
                }
            }
            else if (selectedIndex >= 0)
            {
                var obj = escenario.GetObjetos()[selectedIndex];

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
                    obj.Cleanup();
                    escenario.GetObjetos().RemoveAt(selectedIndex);
                    selectedIndex = -1;
                }

                ImGui.Text($"Modo actual: {ModoGizmo}");
            }

            ImGui.End();
        }

        private Vector3N ConvertToNumerics(Vector3TK v) => new Vector3N(v.X, v.Y, v.Z);
        private Vector3TK ConvertToOpenTK(Vector3N v) => new Vector3TK(v.X, v.Y, v.Z);
    }
}
