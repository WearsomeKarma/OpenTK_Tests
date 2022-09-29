
namespace OpenTK_Test;

public class Args_Parser
{
    private readonly string[] ARGS;

    public Args_Parser(string[] args)
    {
        ARGS = args.ToArray();
    }

    public bool Try(int index, ref int val, string? append=null)
    {
        bool ret = 
            index < ARGS.Length 
            && 
            int.TryParse(ARGS[index], out val);

        if (ret)
            Console.WriteLine("Positional Argument[{0}]: ({1}) used{2}.", index, val, append);

        return ret;
    }
}
