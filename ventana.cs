using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using LearnOpenTK.Common;
using U3DObjeto;
using Vortice.Mathematics;
using OpenTK.Windowing.Common.Input;

public class UWindow : GameWindow
{
    private Shader shaderProgram2;
    private int modelLoc, viewLoc, projLoc, colorLoc;
    private Camera camera;
    private float cameraSpeed = 2.5f;
    private float sensitivity = 60f; // velocidad angular

    private Escenario escenario;
    private Vector2 lastMousePos;
    private bool firstMove = true;

    private GizmoRenderer gizmoRenderer;

    private bool isDragging = false;
    private Vector3 dragStart;
    private Vector3 axisMove;

    private bool isHoveringX, isHoveringY, isHoveringZ;
    private MouseCursor currentCursor = MouseCursor.Default;



    private EditorOverlay uiManager;
    public UWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings) { }

    public void AddObject(Objeto3D obj)
    {
        obj.Initialize();
        escenario.AddObjeto(obj);
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.Enable(EnableCap.DepthTest);
        //GL.Disable(EnableCap.DepthTest);
        // Crear cámara con posición inicial y relación de aspecto
        camera = new Camera(new Vector3(0f, 0f, 5f), Size.X / (float)Size.Y);
        

        escenario = new Escenario();
        uiManager = new EditorOverlay(escenario, camera, this);

        shaderProgram2 = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
        shaderProgram2.Use();

        modelLoc = shaderProgram2.GetUniformLocation("model");
        viewLoc = shaderProgram2.GetUniformLocation("view");
        projLoc = shaderProgram2.GetUniformLocation("projection");
        colorLoc = shaderProgram2.GetUniformLocation("uColor");

        gizmoRenderer = new GizmoRenderer();


        Matrix4 projection = camera.GetProjectionMatrix();
        GL.UniformMatrix4(projLoc, false, ref projection);
        
        //escenario.Cargar();
        uiManager.Run(); // Lanza la ventana ImGui  
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float deltaTime = (float)args.Time;

        var input = KeyboardState;

        // Bloqueo del movimiento de cámara mientras se arrastra un gizmo
        bool bloqueadoPorGizmo = isDragging;

        if (!bloqueadoPorGizmo)
        {
            // Movimiento con WASD + QE
            if (input.IsKeyDown(Keys.W))
                camera.Position += camera.Front * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.S))
                camera.Position -= camera.Front * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.A))
                camera.Position -= camera.Right * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.D))
                camera.Position += camera.Right * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.Q))
                camera.Position -= camera.Up * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.E))
                camera.Position += camera.Up * cameraSpeed * deltaTime;

            // Movimiento con mouse (clic derecho)
            if (MouseState.IsButtonDown(MouseButton.Right))
            {
                CursorState = CursorState.Grabbed;
                var mouse = MouseState.Position;

                if (firstMove)
                {
                    lastMousePos = mouse;
                    firstMove = false;
                }
                else
                {
                    var deltaX = mouse.X - lastMousePos.X;
                    var deltaY = mouse.Y - lastMousePos.Y;
                    lastMousePos = mouse;

                    camera.Yaw += deltaX * sensitivity * deltaTime;
                    camera.Pitch -= deltaY * sensitivity * deltaTime;
                }
            }
            else
            {
                CursorState = CursorState.Normal;
                firstMove = true;
            }

            // Zoom con la rueda
            float scroll = MouseState.ScrollDelta.Y;
            if (scroll != 0)
                camera.Position += camera.Front * scroll * cameraSpeed * deltaTime * 60f;
        }

        // Comandos
        if (input.IsKeyPressed(Keys.Space)) CrearObjetoEnCamara();
        if (input.IsKeyPressed(Keys.O)) escenario.Guardar();
        if (input.IsKeyPressed(Keys.K)) escenario.Cargar();

        isHoveringX = false;
        isHoveringY = false;
        isHoveringZ = false;

        if (uiManager.SelectedIndex >= 0)
        {
            var selectedObj = escenario.GetObjetos()[uiManager.SelectedIndex];
            Matrix4 model = selectedObj.GetModelMatrix();

            // Hover sobre ejes
            float _;
            isHoveringX = IsMouseOverAxis(Vector3.UnitX, model, out _);
            isHoveringY = IsMouseOverAxis(Vector3.UnitY, model, out _);
            isHoveringZ = IsMouseOverAxis(Vector3.UnitZ, model, out _);

            // Comenzar arrastre
            if (MouseState.IsButtonPressed(MouseButton.Left) && !isDragging)
            {
                if (isHoveringX)
                {
                    isDragging = true;
                    axisMove = Vector3.UnitX;
                }
                else if (isHoveringY)
                {
                    isDragging = true;
                    axisMove = Vector3.UnitY;
                }
                else if (isHoveringZ)
                {
                    isDragging = true;
                    axisMove = Vector3.UnitZ;
                }

                if (isDragging)
                {
                    // Guardar posición inicial del mouse
                    lastMousePos = MouseState.Position;
                }
            }

            // Movimiento durante el drag
            if (MouseState.IsButtonDown(MouseButton.Left) && isDragging)
            {
                Vector2 currentMouse = MouseState.Position;
                Vector2 delta = currentMouse - lastMousePos;
                lastMousePos = currentMouse;

                // Dirección transformada del eje en espacio mundial
                Vector3 axisWorld = Vector3.Normalize(Vector3.TransformNormal(axisMove, model));

                // Movimiento basado en delta del mouse X
                float moveAmount = delta.X * 0.01f;
                selectedObj.Position += axisWorld * moveAmount;
            }

            // Fin del arrastre
            if (MouseState.IsButtonReleased(MouseButton.Left))
            {
                isDragging = false;
            }
        }
        //Console.WriteLine(escenario.GetObjetos().LongCount());

    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shaderProgram2.Use();

        Matrix4 view = camera.GetViewMatrix();
        GL.UniformMatrix4(viewLoc, false, ref view);

        Matrix4 projection = camera.GetProjectionMatrix();
        GL.UniformMatrix4(projLoc, false, ref projection);

        // Renderizar objetos 3D
        foreach (var obj in escenario.GetObjetos())
        {
            Matrix4 model = obj.GetModelMatrix();
            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.Uniform3(colorLoc, obj.Color);
            obj.Draw();
        }

        int hoveredAxis = -1;

        if (uiManager.SelectedIndex >= 0)
        {
            var selectedObj = escenario.GetObjetos()[uiManager.SelectedIndex];
            Matrix4 model = selectedObj.GetModelMatrix();

            if (IsMouseOverAxis(Vector3.UnitX, model, out _))
                hoveredAxis = 0;
            else if (IsMouseOverAxis(Vector3.UnitY, model, out _))
                hoveredAxis = 1;
            else if (IsMouseOverAxis(Vector3.UnitZ, model, out _))
                hoveredAxis = 2;

            gizmoRenderer.Render(camera.GetViewMatrix(), camera.GetProjectionMatrix(), model, hoveredAxis);
        }



        SwapBuffers();
    }



    protected override void OnUnload()
    {
        base.OnUnload();
        foreach (var obj in escenario.GetObjetos())
            obj.Cleanup();
    }

    public void CrearObjetoEnCamara()
    {
        var (vertices, indices) = ObjLoader.LoadObj("letraU.obj", 0.1f);
        var random = new Random();

        var nuevoObj = new Objeto3D(
            position: camera.Position + camera.Front * 3f,
            centroMasa: Vector3.Zero,
            vertices: vertices,
            indices: indices,
            color: new Vector3(
                (float)random.NextDouble(),
                (float)random.NextDouble(),
                (float)random.NextDouble())
        );

        //nuevoObj.Initialize();
        escenario.AddObjeto(nuevoObj);

        // Guardar la lista completa en JSON después de agregar el nuevo objeto
        //JsonEscenario.Guardar(objects.GetObjetos());
    }
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, Size.X, Size.Y);
    }
    private bool IsMouseOverAxis(Vector3 axisDirection, Matrix4 modelMatrix, out float axisHitFactor)
    {
        Vector2 mousePos = MouseState.Position;
        Ray ray = GetMouseRay(mousePos);

        Vector3 gizmoOrigin = modelMatrix.ExtractTranslation();
        
        Vector3 axisWorldDir = Vector3.Normalize(Vector3.TransformNormal(axisDirection, modelMatrix));
        Vector3 axisEnd = gizmoOrigin + axisWorldDir * 1.5f; // alarga el eje para mejor detección
        
        float distance = DistanceBetweenRayAndSegment(ray.Origin, ray.Direction, gizmoOrigin, axisEnd, out axisHitFactor) - 0.20f;
        //Console.WriteLine($"Eje: {axisWorldDir}, Origen: {gizmoOrigin}, Fin: {axisEnd}, Distance: {distance}, Mouse: {ray.Origin}");
        //Console.WriteLine($"Eje: {axisWorldDir}, Origen: {gizmoOrigin}, Fin: {axisEnd}, Distance: {distance}");
        //Console.WriteLine($"Mouse: {mousePos}");
        //return distance > 0f && distance < 0.15f; // sensibilidad más precisa
        if (axisWorldDir.X == 1)
        {
            return distance > -0.05f && distance < 0.5f;
        }
        else if (axisWorldDir.Y == 1)
        {
            return distance < 0.1f;
        }
        else if (axisWorldDir.Z == 1)
        {
            return distance > -0.05f && distance < 0.5f;
        }
        
        return distance < 0.1f;
    }
    // Distancia mínima entre un rayo (origen + dirección) y un segmento (p1, p2)
    private float DistanceBetweenRayAndSegment(Vector3 rayOrigin, Vector3 rayDir, Vector3 segStart, Vector3 segEnd, out float segT)
    {
        Vector3 v = rayDir;
        Vector3 w = segEnd - segStart;
        Vector3 u = rayOrigin - segStart;

        float a = Vector3.Dot(v, v); // == 1 si rayDir está normalizado
        float b = Vector3.Dot(v, w);
        float c = Vector3.Dot(w, w);
        float d = Vector3.Dot(v, u);
        float e = Vector3.Dot(w, u);

        float denom = a * c - b * b;

        float s, t;

        if (denom != 0.0f)
            s = (b * e - c * d) / denom;
        else
            s = 0.0f; // paralelo

        t = (a * e - b * d) / denom;
        segT = Math.Clamp(t, 0f, 1f); // clamp para que se mantenga sobre el segmento

        Vector3 closestRayPoint = rayOrigin + v * Math.Max(s, 0);
        Vector3 closestSegPoint = segStart + w * segT;

        return (closestRayPoint - closestSegPoint).Length;
    }
    private Ray GetMouseRay(Vector2 mousePosition)
    {
        Vector2 ndc = new Vector2(
            (2.0f * mousePosition.X) / Size.X - 1.0f,
            1.0f - (2.0f * mousePosition.Y) / Size.Y
        );

        Vector4 clipNear = new Vector4(ndc.X, ndc.Y, -1.0f, 1.0f);
        Vector4 clipFar = new Vector4(ndc.X, ndc.Y, 1.0f, 1.0f);

        Matrix4 invViewProj = (camera.GetViewMatrix() * camera.GetProjectionMatrix()).Inverted();

        Vector4 worldNear = Vector4.TransformRow(clipNear, invViewProj);
        Vector4 worldFar = Vector4.TransformRow(clipFar, invViewProj);


        worldNear /= worldNear.W;
        worldFar /= worldFar.W;

        Vector3 origin = new Vector3(worldNear.X, worldNear.Y, worldNear.Z);
        Vector3 direction = Vector3.Normalize(new Vector3(worldFar.X, worldFar.Y, worldFar.Z) - origin);

        return new Ray(origin, direction);
    }


    private struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }
    }


}
