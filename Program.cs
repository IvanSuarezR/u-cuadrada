using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace U3DExample
{
    // Clase que encapsula un objeto 3D
    public class Objeto3D
    {
        public Vector3 Position { get; set; }
        public Vector3 CentroMasa { get; set; }
        public float[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public Vector3 Color { get; set; }

        private int vao, vbo, ebo;

        public Objeto3D(Vector3 position, Vector3 CentroM, float[] vertices, uint[] indices, Vector3 color)
        {
            Position = position;
            CentroMasa = CentroM;
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
            return Matrix4.CreateTranslation(Position - CentroMasa);
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

    public class UWindow : GameWindow
    {
        private int shaderProgram;
        private int modelLoc, viewLoc, projLoc, colorLoc;
        private Matrix4 projection;
        private List<Objeto3D> objects = new List<Objeto3D>();

        private Vector3 cameraPos = new Vector3(0f, 0f, 5f);
        private Vector3 cameraFront = new Vector3(0f, 0f, -1f);
        private Vector3 cameraUp = new Vector3(0f, 1f, 0f);
        private float yaw = -90f;
        private float pitch = 0f;
        private float cameraSpeed = 2.5f;
        private float sensitivity = 0.1f;

        private readonly string vertexShaderSource = @"
            #version 330 core
            layout(location = 0) in vec3 aPos;
            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            void main()
            {
                gl_Position = projection * view * model * vec4(aPos, 1.0);
            }";

        private readonly string fragmentShaderSource = @"
            #version 330 core
            out vec4 FragColor;
            uniform vec3 uColor;
            void main()
            {
                FragColor = vec4(uColor, 1.0);
            }";

        public UWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings) { }

        public void AddObject(Objeto3D obj)
        {
            obj.Initialize();
            objects.Add(obj);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.Enable(EnableCap.DepthTest);

            shaderProgram = CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
            GL.UseProgram(shaderProgram);

            modelLoc = GL.GetUniformLocation(shaderProgram, "model");
            viewLoc = GL.GetUniformLocation(shaderProgram, "view");
            projLoc = GL.GetUniformLocation(shaderProgram, "projection");
            colorLoc = GL.GetUniformLocation(shaderProgram, "uColor");

            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y, 0.1f, 100f);
            GL.UniformMatrix4(projLoc, false, ref projection);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            float deltaTime = (float)args.Time;
            KeyboardState input = KeyboardState;

            // Movimiento horizontal y adelante/atrás (WASD)
            if (input.IsKeyDown(Keys.W))
                cameraPos += cameraFront * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.S))
                cameraPos -= cameraFront * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.A))
                cameraPos -= Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed * deltaTime;
            if (input.IsKeyDown(Keys.D))
                cameraPos += Vector3.Normalize(Vector3.Cross(cameraFront, cameraUp)) * cameraSpeed * deltaTime;

            // Movimiento vertical (Q/E)
            if (input.IsKeyDown(Keys.Q))
                cameraPos -= cameraUp * cameraSpeed * deltaTime; // Bajar
            if (input.IsKeyDown(Keys.E))
                cameraPos += cameraUp * cameraSpeed * deltaTime; // Subir

            // Rotación de cámara (flechas)
            if (input.IsKeyDown(Keys.Left))
                yaw -= sensitivity;
            if (input.IsKeyDown(Keys.Right))
                yaw += sensitivity;
            if (input.IsKeyDown(Keys.Up))
                pitch += sensitivity;
            if (input.IsKeyDown(Keys.Down))
                pitch -= sensitivity;
            pitch = MathHelper.Clamp(pitch, -89f, 89f);

            // Actualizar dirección de la cámara
            cameraFront.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            cameraFront.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            cameraFront.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            cameraFront = Vector3.Normalize(cameraFront);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            Matrix4 view = Matrix4.LookAt(cameraPos, cameraPos + cameraFront, cameraUp);
            GL.UniformMatrix4(viewLoc, false, ref view);

            foreach (var obj in objects)
            {
                Matrix4 model = obj.GetModelMatrix();
                GL.UniformMatrix4(modelLoc, false, ref model);
                GL.Uniform3(colorLoc, obj.Color);
                obj.Draw();
            }

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            foreach (var obj in objects)
            {
                obj.Cleanup();
            }
            GL.DeleteProgram(shaderProgram);
        }

        private int CompileShader(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Console.WriteLine(info);
            }
            return shader;
        }

        private int CreateShaderProgram(string vertexSource, string fragmentSource)
        {
            int vertexShader = CompileShader(ShaderType.VertexShader, vertexSource);
            int fragmentShader = CompileShader(ShaderType.FragmentShader, fragmentSource);
            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }
    }

        class Program
    {
        static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 600),
                Title = "Letra U cuadrada"
            };

            using (var window = new UWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                // Definimos todos los vértices de la U completa
                float[] uVertices = {
                    // Palo izquierdo (frontal)
                    -1.1f, -1.0f,  0.2f,  // 0
                    -0.8f, -1.0f,  0.2f,  // 1
                    -0.8f,  1.0f,  0.2f,  // 2
                    -1.1f,  1.0f,  0.2f,  // 3
                    
                    // Palo izquierdo (trasero)
                    -1.1f, -1.0f, -0.2f,  // 4
                    -0.8f, -1.0f, -0.2f,  // 5
                    -0.8f,  1.0f, -0.2f,  // 6
                    -1.1f,  1.0f, -0.2f,  // 7
                    
                    // Base inferior (frontal)
                    -0.8f, -1.0f,  0.2f,  // 8 (mismo que 1)
                    0.5f, -1.0f,  0.2f,  // 9
                    0.5f, -0.8f,  0.2f,  // 10
                    -0.8f, -0.8f,  0.2f,  // 11
                    
                    // Base inferior (trasera)
                    -0.8f, -1.0f, -0.2f,  // 12 (mismo que 5)
                    0.5f, -1.0f, -0.2f,  // 13
                    0.5f, -0.8f, -0.2f,  // 14
                    -0.8f, -0.8f, -0.2f,  // 15
                    
                    // Palo derecho (frontal)
                    0.5f, -1.0f,  0.2f,  // 16 (mismo que 10)
                    0.8f, -1.0f,  0.2f,  // 17
                    0.8f,  1.0f,  0.2f,  // 18
                    0.5f,  1.0f,  0.2f,  // 19
                    
                    // Palo derecho (trasero)
                    0.5f, -1.0f, -0.2f,  // 20 (mismo que 14)
                    0.8f, -1.0f, -0.2f,  // 21
                    0.8f,  1.0f, -0.2f,  // 22
                    0.5f,  1.0f, -0.2f   // 23
                };

                // Índices para todas las caras de la U completa
                uint[] indices = {
                    // Palo izquierdo
                    0, 1, 2, 2, 3, 0,   // Frontal
                    4, 5, 6, 6, 7, 4,   // Trasero
                    4, 0, 3, 3, 7, 4,   // Izquierda
                    1, 5, 6, 6, 2, 1,   // Derecha
                    4, 5, 1, 1, 0, 4,   // Inferior
                    3, 2, 6, 6, 7, 3,   // Superior
                    
                    // Base inferior
                    8, 9, 10, 10, 11, 8,  // Frontal
                    12, 13, 14, 14, 15, 12,  // Trasero
                    12, 8, 11, 11, 15, 12,  // Inferior
                    9, 13, 14, 14, 10, 9,  // Superior
                    8, 12, 13, 13, 9, 8,   // Izquierda
                    11, 15, 14, 14, 10, 11, // Derecha
                    
                    // Palo derecho
                    16, 17, 18, 18, 19, 16,  // Frontal
                    20, 21, 22, 22, 23, 20,  // Trasero
                    20, 16, 19, 19, 23, 20,  // Izquierda
                    17, 21, 22, 22, 18, 17,  // Derecha
                    20, 21, 17, 17, 16, 20,  // Inferior
                    19, 18, 22, 22, 23, 19   // Superior
                };

                Vector3 color = new Vector3(1.0f, 2.0f, 0.0f); // Azul
                Vector3 CentroMasa = new Vector3(0.0f, 0.0f, 0.0f); // Centro de masa en el origen

                var uComplete = new Objeto3D(Vector3.Zero, CentroMasa, uVertices, indices, color);
                var uComplete2 = new Objeto3D(Vector3.Zero, new Vector3(3.0f, 0.0f, 0.0f), uVertices, indices, color);
                window.AddObject(uComplete);
                window.AddObject(uComplete2);

                window.Run();
            }
        }
    }
}