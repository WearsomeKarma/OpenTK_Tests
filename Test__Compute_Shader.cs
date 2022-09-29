
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Test;

public class Test__Compute_Shader : Test__Window
{
    [AllowNull]
    private Shader SHADER__COMPUTE;
    [AllowNull]
    private Texture TEXTURE;

    protected internal override void Handle__Arguments(string[] args)
    {
        Args_Parser pargs = new Args_Parser(args);

        int width = 10, height = 10;

        pargs.Try(1, ref width, " as width");
        pargs.Try(2, ref height, " as height");

        TEXTURE = new Texture(width, height);

        string source__compute = @"
#version 430 core
layout(local_size_x = 1, local_size_y = 1) in;
layout(rgba32f, binding = 0) uniform image2D img_output;

void main()
{
    vec4 pixel = vec4(gl_GlobalInvocationID.x / gl_WorkGroupSize.x, 0, 0, 1);
    ivec2 pixel_coords = ivec2(gl_GlobalInvocationID.xy);

    imageStore(img_output, pixel_coords, pixel);
}
";

        bool err = false;

        SHADER__COMPUTE =
            new Shader.Factory()
            .Begin()
            .Add__Shader(ShaderType.ComputeShader, source__compute, ref err)
            .Link()
            ;

        if (err) Close();
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, TEXTURE.TEXTURE_HANDLE);
        SHADER__COMPUTE.Use();
        GL.DispatchCompute(TEXTURE.Width, TEXTURE.Height, 1);

        // make sure write to img is finished.
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

        RENDER__DEFAULT();

        SwapBuffers();
    }
}
