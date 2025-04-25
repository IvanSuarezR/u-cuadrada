using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace YourNamespace
{
    public class EmbeddedShader
    {
        public readonly int Handle;
        private readonly Dictionary<string, int> _uniformLocations;

        public EmbeddedShader()
        {
            string vertexShaderSource = @"
                #version 330 core
                layout(location = 0) in vec3 aPos;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    gl_Position = projection * view * model * vec4(aPos, 1.0);
                }
            ";

            string fragmentShaderSource = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(1.0, 0.0, 0.0, 1.0);
                }
            ";

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            CompileShader(fragmentShader);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);
            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                throw new Exception("Shader linking failed: " + infoLog);
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            _uniformLocations = new Dictionary<string, int>();
            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);
            for (int i = 0; i < numberOfUniforms; i++)
            {
                string key = GL.GetActiveUniform(Handle, i, out _, out _);
                int location = GL.GetUniformLocation(Handle, key);
                _uniformLocations[key] = location;
            }
        }

        private void CompileShader(int shader)
        {
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception("Shader compilation failed: " + infoLog);
            }
        }

        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void SetMatrix4(string name, Matrix4 matrix)
        {
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.UniformMatrix4(location, false, ref matrix); // false = no transponer
            }
        }

        public void SetVector3(string name, Vector3 vec)
        {
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform3(location, vec);
            }
        }

        public void SetFloat(string name, float value)
        {
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform1(location, value);
            }
        }

        public void SetInt(string name, int value)
        {
            if (_uniformLocations.TryGetValue(name, out int location))
            {
                GL.Uniform1(location, value);
            }
        }
    }
}
