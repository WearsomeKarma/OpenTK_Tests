
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Test;

public class Test__Custom_Texture : Test__Window
{
    [AllowNull]
    private Shader SHADER;

    [AllowNull]
    private Texture TEXTURE;

    protected internal override void Handle__Arguments(string[] args)
    {
        int width, height;

        if (args.Length < 2 || !int.TryParse(args[1], out width))
            width = 10;
        else
            Console.WriteLine("Positional Argument[1]: ({0}) used as texture width.", width);
        if (args.Length < 3 || !int.TryParse(args[2], out height))
            height = 10;
        else
            Console.WriteLine("Positional Argument[2]: ({0}) used as texture height.", height);

        TEXTURE = 
            new Texture
            (
                width, height,
                //Texture.Normalized__Pixel_Initalizer.Default
                Texture.Unsigned_Byte__Pixel_Initalizer.Default
            );

        string source_vert = @"
#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPosition, 1);
    TexCoord = aTexCoord;
}
";
        string source_frag = @"
#version 330
        
out vec4 outputColor;

in vec2 TexCoord;

uniform sampler2D sample;

void main()
{
    outputColor = texture(sample, TexCoord);
    if (outputColor.x == 0) outputColor.x = TexCoord.x;
}
";

        bool err;
        SHADER = new Shader(source_vert, source_frag, out err);
        SHADER.Use();
        GL.Uniform1(SHADER.Get__Uniform("sampler"), 0);

        if (err) Close();
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        RENDER__DEFAULT(TEXTURE.TEXTURE_HANDLE);
        SwapBuffers();
    }

    protected internal override void Handle__Reset()
    {
        TEXTURE.Reinitalize__Texture();
    }
}
