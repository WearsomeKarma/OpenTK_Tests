
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Test;

public class Test__Uniform_Array : Test__Window
{
    private int _width = 10, _height = 10;
    private int? _seed = null;
    private int Width
    {
        get => _width;
        set 
        {
            _width = value;
            Private_Update__Texture();
        }
    }
    private int Height
    {
        get => _height;
        set
        {
            _height = value;
            Private_Update__Texture();
        }
    }
    private int? Seed
    {
        get => _seed;
        set
        {
            _seed = value;
            Private_Update__Texture();
        }
    }

    [AllowNull]
    private int[] TEXTURE;

    private Shader SHADER;

    public Test__Uniform_Array()
    {
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.BindVertexArray(SCREEN_RECT__VAO);        
        SHADER.Use();
        int res_x  = SHADER.Get__Uniform("res_x");
        int res_y  = SHADER.Get__Uniform("res_y");
        int buffer = SHADER.Get__Uniform("buffer");
        GL.Uniform1(res_x, Width);
        GL.Uniform1(res_y, Height);
        GL.Uniform1(buffer, TEXTURE.Length, TEXTURE);
        ErrorCode code;
        if ((code = GL.GetError()) != ErrorCode.NoError)
        {
            Console.WriteLine(code);
        }
        GL.DrawElements(PrimitiveType.Triangles, SCREEN_RECT__ELEMENTS.Length, DrawElementsType.UnsignedInt, 0);
        //GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        SwapBuffers();
    }

    protected internal override void Handle__Arguments(string[] args)
    {
        int width = 0, height = 0;

        void clamp(ref int v) => v = Math.Max(1, Math.Min(1000, v));

        if (args.Length >= 2)
        {
            if(int.TryParse(args[1], out width))
            {
                clamp(ref width);
                Console.WriteLine("Positional Argument[1]: ({0}) used as texture width.", width);
                _width = width;
            }
        }
        if (args.Length >= 3)
        {
            if(int.TryParse(args[2], out height))
            {
                clamp(ref height);
                Console.WriteLine("Positional Argument[2]: ({0}) used as texture height.", height);
                _height = height;
            }
        }

        string shader_vert = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    TexCoord = aTexCoord;
}
";
        string shader_frag = @"
#version 330

out vec4 outputColor;

in vec2 TexCoord;

uniform int res_x;
uniform int res_y;

uniform int buffer[" + $"{Width * Height}" + @"];

void main()
{
    int x = int(float(res_x) * TexCoord.x);
    int y = res_x * int(float(res_y) * TexCoord.y); 
    float pixel_code = float(buffer[x + y]);
    //outputColor = vec4(1.0 / float(x + y), 0, 0, 1);
    outputColor = vec4(1.0/pixel_code,0,0,1);
    //if (x < 6) outputColor = vec4(1,0,0,1);
    //if (pixel_code < 0.0) outputColor = vec4(1,0,0,1);
    //if (pixel_code == 0.0) outputColor = vec4(0,1,0,1);
    //if (pixel_code > 128.0) outputColor = vec4(0,0,1,1);
}
";

        int handle_vert = GL.CreateShader(ShaderType.VertexShader);
        int handle_frag = GL.CreateShader(ShaderType.FragmentShader);

        bool err;

        SHADER = new Shader
        (
            shader_vert,
            shader_frag,
            out err
        );

        if(err)
        {
            Close();
            return;
        }

        Private_Update__Texture();
    }

    protected internal override void Handle__Reset()
    {
        Private_Update__Texture();
    }

    private void Private_Update__Texture()
    {
        Random random = 
            (Seed != null)
            ? new Random((int)Seed)
            : new Random()
            ;

        TEXTURE = new int[Width * Height];

        for(int i=0;i<Width*Height;i++)
            TEXTURE[i] = random.Next(255);
    }
}
