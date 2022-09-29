
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace OpenTK_Test;

public class Shader
{
    public int PROGRAM_HANDLE { get; private set; }

    private Shader(int handle)
    {
        PROGRAM_HANDLE = handle;
    }

    public Shader
    (
        string vert,
        string frag,
        out bool error
    )
    {
        int handle_vert = GL.CreateShader(ShaderType.VertexShader);
        int handle_frag = GL.CreateShader(ShaderType.FragmentShader);

        GL.ShaderSource(handle_vert, vert);
        GL.ShaderSource(handle_frag, frag);
        GL.CompileShader(handle_vert);
        string err;
        Console.WriteLine("SHADER-VERT:\n{0}", err = GL.GetShaderInfoLog(handle_vert));
        error = err != string.Empty;
        GL.CompileShader(handle_frag);
        Console.WriteLine("SHADER-FRAG:\n{0}", err = GL.GetShaderInfoLog(handle_frag));
        error = error || err != string.Empty;
        PROGRAM_HANDLE = GL.CreateProgram();
        GL.AttachShader(PROGRAM_HANDLE, handle_vert);
        GL.AttachShader(PROGRAM_HANDLE, handle_frag);
        GL.LinkProgram(PROGRAM_HANDLE);
        GL.DeleteShader(handle_vert);
        GL.DeleteShader(handle_frag);
    }

    public void Use()
    {
        GL.UseProgram(PROGRAM_HANDLE);
    }

    public int Get__Uniform(string uniform_name)
        => GL.GetUniformLocation(PROGRAM_HANDLE, uniform_name);

    public class Factory
    {
        [AllowNull]
        private Shader shader;
        private List<int> handles = new List<int>();

        public Factory Begin()
        {
            if (shader != null) return this;
            if (handles.Count != 0) return this;

            shader = new Shader(GL.CreateProgram());

            return this;
        }

        public Factory Add__Shader(ShaderType shader_type, string source, ref bool err)
        {
            if (err) return this;

            int handle = GL.CreateShader(shader_type);
            GL.ShaderSource(handle, source);
            GL.CompileShader(handle);
            string error = GL.GetShaderInfoLog(handle);
            err = error != string.Empty;

            if (err)
            {
                Console.WriteLine("SHADER[{0}]: \n{1}", shader_type, error);
                return this;
            }

            handles.Add(handle);
            GL.AttachShader(shader.PROGRAM_HANDLE, handle);
            return this;
        }

        public Shader Link()
        {
            GL.LinkProgram(shader.PROGRAM_HANDLE);
            foreach(int handle in handles)
            {
                GL.DetachShader(shader.PROGRAM_HANDLE, handle);
                GL.DeleteShader(handle);
            }
            return shader;
        }
    }
}
