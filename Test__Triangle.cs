
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace OpenTK_Test;

public class Test__Triangle : Test__Window
{
    private readonly float[] _vertices =
    {
        -0.5f, -0.5f, 0.0f, // Bottom-left vertex
         0.5f, -0.5f, 0.0f, // Bottom-right vertex
         0.0f,  0.5f, 0.0f  // Top vertex
    };

    private int _vertexBufferObject;

    private int _vertexArrayObject;

    private Shader SHADER;

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        string source_vert = @"
#version 330 core

layout(location = 0) in vec3 aPosition;

void main(void)
{
gl_Position = vec4(aPosition, 1.0);
}
";
        string source_frag = @"
#version 330

out vec4 outputColor;

void main()
{
outputColor = vec4(1.0, 1.0, 0.0, 1.0);
}
";
    
        bool err;
        SHADER = new Shader(source_vert, source_frag, out err);
        if (err) Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.Clear(ClearBufferMask.ColorBufferBit);


        SHADER.Use();

        GL.BindVertexArray(_vertexArrayObject);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        
        SwapBuffers();
    }
}
