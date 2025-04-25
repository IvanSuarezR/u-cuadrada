using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

public class LineShader
{
    public int Handle { get; private set; }

    private int modelLoc;
    private int viewLoc;
    private int projectionLoc;
    private int colorLoc;

    private const string vertexShaderSource = @"
        #version 330 core
        layout(location = 0) in vec3 aPos;
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        void main()
        {
            gl_Position = projection * view * model * vec4(aPos, 1.0);
        }";

    private const string fragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        uniform vec3 color;
        void main()
        {
            FragColor = vec4(color, 1.0);
        }";

    public LineShader()
    {
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);
        CheckCompile(vertexShader, "vertex");

        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);
        CheckCompile(fragmentShader, "fragment");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
            throw new Exception($"Shader linking failed: {GL.GetProgramInfoLog(Handle)}");

        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        modelLoc = GL.GetUniformLocation(Handle, "model");
        viewLoc = GL.GetUniformLocation(Handle, "view");
        projectionLoc = GL.GetUniformLocation(Handle, "projection");
        colorLoc = GL.GetUniformLocation(Handle, "color");
    }

    private void CheckCompile(int shader, string type)
    {
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
        if (success == 0)
            throw new Exception($"{type} shader compilation failed: {GL.GetShaderInfoLog(shader)}");
    }

    public void Use(Matrix4 model, Matrix4 view, Matrix4 projection, Vector3 color)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(modelLoc, false, ref model);
        GL.UniformMatrix4(viewLoc, false, ref view);
        GL.UniformMatrix4(projectionLoc, false, ref projection);
        GL.Uniform3(colorLoc, ref color);
    }
}
