using System;

namespace HeroesManager
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter your \"Data.jmp\" path to extract: ");
            var input = Console.ReadLine();
            Console.WriteLine("Enter output directory: ");
            var output = Console.ReadLine();

            JMPFile file = null;
            if (input.ToLower().Contains("data.jmp"))
                file = new JMPFile(input, output);
               else
                file = new JMPFile(input + @"\data.jmp", output);

            Console.ReadKey();
        }
    }
}
