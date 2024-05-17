using System;
using System.IO;

namespace Interpreter
{
    public class Program
    {
        static void Main(string[] args)
        {

            RunTest("../../Resources/code.txt");

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void RunTest(string filePath)
        {
            try
            {
                var code = File.ReadAllText(filePath);
                Interpreter interpreter = new Interpreter();
                Console.WriteLine($"Running test: {filePath}");
                interpreter.Interpret(code);
                Console.WriteLine(); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading {filePath}: {ex.Message}");
            }
        }
    }
}