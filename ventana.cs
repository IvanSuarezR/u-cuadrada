using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using U3DObjeto;


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

        private Vector3 rotacion;


        
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
            rotacion = cameraFront;
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

        public void CrearObjetoEnCamara()
        {
            // Cargamos el modelo y escalamos (ajusta la escala si es necesario)
            var (vertices, indices) = ObjLoader.LoadObj("letraU.obj", 0.1f);
            var random = new Random();
            var nuevoObj = new Objeto3D(
                position: cameraPos+(0,0,-3f),
                CentroM: Vector3.Zero,  // Ajusta este valor si tu modelo no está centrado
                vertices: vertices,
                indices: indices,
                color: new Vector3(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble())
            );

            AddObject(nuevoObj);
        }

    }