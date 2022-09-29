
using System.Reflection;

namespace OpenTK_Test;

public class Program
{
    public static IEnumerable<Type> Test_Types
        =>
        Assembly
        .GetExecutingAssembly()
        .GetTypes()
        .Where(t => t.IsSubclassOf(typeof(Test__Window)))
        ;

    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Private_Show__Types();
            return;
        }

        Type? selected_type =
            Test_Types
            .Where(t => t.Name == args[0])
            .FirstOrDefault();

        if (selected_type == null)
        {
            Console.WriteLine("Specified type: {0} was not found.", args[0]);
            Private_Show__Types();
            return;
        }

        Test__Window? instance =
            Activator.CreateInstance(selected_type) as Test__Window;

        if (instance == null)
        {
            Console.WriteLine("Failed to instanciate test_type: {0}.", selected_type);
            return;
        }

        instance.Handle__Arguments(args);
        instance.Run();
    }

    private static void Private_Show__Types()
    {
        Console.WriteLine("Specify one of the following as argument:");
        foreach(Type test_type in Test_Types)
            Console.WriteLine("\t{0}", test_type.Name);
    }
}
