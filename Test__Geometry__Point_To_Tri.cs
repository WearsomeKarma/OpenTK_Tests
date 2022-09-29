
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Test;

public class Test__Geometry__Point_To_Tri : Test__Window
{
    private int CELLS_VAO__ONE;

    private int CELL_DATA_SIZE;

    private int CELLS_BUFFER__ONE;
    private int CELLS_BUFFER__TWO;

    [AllowNull]
    private Shader SHADER__UPDATE;
    [AllowNull]
    private Shader SHADER__DRAW;

    private int width = 10, height = 10;

    private void Private_Create__Cells()
    {
        float[] cells = new float[CELL_DATA_SIZE];
        
        Random random = new Random();

        for(int i=0;i<CELL_DATA_SIZE;i+=3)
        {
            cells[i  ] = ((i/3) % width);
            cells[i+1] = ((i/3) / width);
            cells[i+2] = (random.Next(16) == 0) ? 1 : 0;
            //cells[i+2] = (float)random.Next(16);
            //cells[i+2] = 1;
            //cells[i+2] = (i/3) / width;
        }

        GL.BindBuffer(BufferTarget.ArrayBuffer, CELLS_BUFFER__ONE);
        GL.BufferData(BufferTarget.ArrayBuffer, CELL_DATA_SIZE * sizeof(float), cells, BufferUsageHint.StaticDraw);
    }

    protected internal override void Handle__Arguments(string[] args)
    {
        int seed = -1;

        Args_Parser pargs = new Args_Parser(args);

        pargs.Try(1, ref width, " as width");
        pargs.Try(2, ref height, " as height");
        pargs.Try(3, ref seed, " as seed");

        CELLS_VAO__ONE = GL.GenVertexArray();
        GL.BindVertexArray(CELLS_VAO__ONE);

        CELLS_BUFFER__ONE = GL.GenBuffer();

        CELL_DATA_SIZE = width * height * 3; // x,y,state

        Private_Create__Cells();
        
        //GL.BindBuffer(BufferTarget.ArrayBuffer, CELLS_BUFFER__TWO);
        //GL.BufferData(BufferTarget.ArrayBuffer, cell_data_size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
        
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

/*
        string source__update_cell = @"
#version 420 core
layout(location = 0) in ivec3 aPosition;

uniform int width;

struct cell
{
    int x;
    int y;
    int state;
};

layout(std140, binding = 3) uniform cell_buffer
{
    cell cells[];
};

int check_neighbor(int index)
{
    if (cells[index].state == 0)
        return 0;
    return 1;
}

void main()
{
    int index = aPosition.x + (aPosition.y * width);

    // if 2 or less neighbors die.
    // if 5 or more neighbors die.

    int neighbors = 0;
    neighbors += check_neighbor(index + 1); // right
    neighbors += check_neighbor(index - 1); // left
    neighbors += check_neighbor(index - width); // top
    neighbors += check_neighbor(index + width); // bottom
    neighbors += check_neighbor(index - width - 1); // top left
    neighbors += check_neighbor(index - width + 1); // top right
    neighbors += check_neighbor(index + width - 1); // bottom left
    neighbors += check_neighbor(index + width + 1); // bottom right

    if (neighbors < 3 || neighbors > 4) cells[index] = 0;
    else cells[index] = 1;
}
";
*/

        bool err = false;
        /*
        SHADER__UPDATE = 
            new Shader.Factory()
            .Begin()
            .Add__Shader(ShaderType.VertexShader, source__update_cell, ref err)
            .Link();
        */
        if (err) { Close(); return; }

        string source__draw_vert = @"
#version 420 core
layout(location = 0) in vec3 cell;

void main()
{
    gl_Position = vec4(cell.xy, cell.z, 1);
}
";
        string source__draw_geom = @"
#version 330
layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

flat out int state;

uniform float width;
uniform float height;

void main()
{
    vec4 pos = gl_in[0].gl_Position ;// - vec4(2,2,0,0);
    state = int(pos.z);

    float mod = 0.9 + (0.1 * state);

    float cell_width = (1 / width ) * mod;
    float cell_height = (1 / height) * mod;

    pos = vec4(pos.x * 2 / width, pos.y * 2 / height, 0, 1) - vec4(1 - cell_width / mod, 1 - cell_height / mod, 0, 0);

    //pos = vec4(pos.x / width * 6.0, (pos.y / height) + (0.9 * pos.y), 0, 1) - vec4(2,2,0,0);
    //pos = vec4(width / pos.x, height / pos.y, 0, 1) - vec4(2, 2, 0, 0);

    gl_Position = pos + vec4(-cell_width, -cell_height, 0, 0);
    //gl_Position = vec4(-0.5,0.5,0,1);
    EmitVertex();

    gl_Position = pos + vec4( cell_width, -cell_height, 0, 0);
    //gl_Position = vec4(0.5,0.5,0,1);
    EmitVertex();

    gl_Position = pos + vec4(-cell_width,  cell_height, 0, 0);
    //gl_Position = vec4(-0.5,-0.5,0,1);
    EmitVertex();

    gl_Position = pos + vec4( cell_width,  cell_height, 0, 0);
    //gl_Position = vec4(0.5,-0.5,0,1);
    EmitVertex();

    EndPrimitive();
}
";
        string source__draw_frag = @"
#version 330

out vec4 output_color;
flat in int state;

void main()
{
    output_color = vec4(0.05, state, 0.05, 1);
}
";

        err = false;
        SHADER__DRAW =
            new Shader.Factory()
            .Begin()
            .Add__Shader(ShaderType.VertexShader, source__draw_vert, ref err)
            .Add__Shader(ShaderType.GeometryShader, source__draw_geom, ref err)
            .Add__Shader(ShaderType.FragmentShader, source__draw_frag, ref err)
            .Link()
            ;

        if (err) { Close(); return; }
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.Viewport(0, 0, Size.X, Size.Y);
        SHADER__DRAW.Use();
        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)width);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)height);
        GL.BindVertexArray(CELLS_VAO__ONE);
        // we have CELL_DATA_SIZE number of floats, but only CELL_DATA_SIZE/3 number of POINTS.
        GL.DrawArrays(PrimitiveType.Points, 0, CELL_DATA_SIZE / 3);
        SwapBuffers();
        ErrorCode error;
        if ((error = GL.GetError()) != ErrorCode.NoError)
            Console.WriteLine(error);
    }

    protected internal override void Handle__Reset()
    {
        Private_Create__Cells();
    }
}
