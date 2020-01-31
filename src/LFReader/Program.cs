using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LFReader
{
    public class Program
    {
        private static string[] header = new[]{
            @".____   ___________ __________                   .___            ",
            @"|    |  \_   _____/ \______   \ ____ _____     __| _/___________ ",
            @"|    |   |    __)    |       _// __ \\__  \   / __ |/ __ \_  __ \",
            @"|    |___|     \     |    |   \  ___/ / __ \_/ /_/ \  ___/|  | \/",
            @"|_______ \___  /     |____|_  /\___  >____  /\____ |\___  >__|   ",
            @"        \/   \/             \/     \/     \/      \/    \/       "
        };

        private static Assembly assembly = typeof(Program).Assembly;

        private static string inputFile;
        private static string outputPath;
        private static int lines = 400;
        private static int offset = 1;

        private static bool printHelp = false;
        private static bool execProgram = false;

        private static void printHeader(string[] args)
        {
            Console.SetBufferSize(640, 480);
            //Console.SetWindowSize(640, 480);

            foreach (var item in header)
                Console.WriteLine(item);

            var data = assembly.GetCustomAttributes().OfType<AssemblyFileVersionAttribute>().FirstOrDefault();

            var vLine = $"Version: v{data?.Version.ToString()}";
            Console.WriteLine("-".PadLeft(vLine.Length, '-'));
            Console.WriteLine(vLine);
            Console.WriteLine("-".PadLeft(vLine.Length, '-'));

            Console.WriteLine();

            /* Args Validations */

            args = args.Select(m => m.ToLower()).ToArray();

            try
            {
                if (args != null && args.Any())
                {
                    inputFile = args[0];
                    outputPath = args[1];
                    printHelp = (args.Contains("-h") || args.Contains("--help"));
                    if (!printHelp)
                    {
                        if (args.Contains("-l") || args.Contains("--lines"))
                        {
                            var largs = args.ToList();
                            var index = largs.IndexOf("-l");
                            if (index == -1) index = largs.IndexOf("--lines");
                            if (largs.Count > index + 1)
                            {
                                var value = largs[index + 1];
                                int.TryParse(value, out int intValue);
                                if (intValue > 0) lines = intValue;
                            }
                        }
                        if (args.Contains("-o") || args.Contains("--offset"))
                        {
                            var largs = args.ToList();
                            var index = largs.IndexOf("-o");
                            if (index == -1) index = largs.IndexOf("--offset");
                            if (largs.Count > index + 1)
                            {
                                var value = largs[index + 1];
                                int.TryParse(value, out int intValue);
                                if (intValue > 0) offset = intValue;
                            }
                        }
                        execProgram = true;
                    }

                    if (!File.Exists(inputFile)) throw new Exception();
                    try
                    {
                        if (Directory.Exists(outputPath)) Directory.Delete(outputPath, true);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"WARNING: {ex.Message}");
                        Console.ResetColor();
                    }

                    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                }

                if (!execProgram)
                    throw new Exception();
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Executing with:");
                    Console.WriteLine($" - Lines per file:  {lines}");
                    Console.WriteLine($" - Start line:      {offset}");
                    Console.ResetColor();
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: Missing arguments");
                printHelp = true;
                execProgram = false;
            }

            if (printHelp)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("");
                Console.WriteLine($"Usage: {assembly.GetName().Name} <input_file> <output_dir> [options]");
                Console.WriteLine("");
                Console.WriteLine("Options:");
                Console.WriteLine(" -h|--help       Display help.");
                Console.WriteLine(" -l|--lines      Set number of lines to split file. (Default 400)");
                Console.WriteLine(" -o|--offset     Set number of start lines. (Default 1)");
            }

            Console.ResetColor();
        }

        private static void Main(string[] args)
        {
            Console.ResetColor();

            printHeader(args);

            if (execProgram)
            {
                Console.WriteLine("");
                try
                {
                    int fileCounter = 1;
                    int counter = 0;
                    string line;

                    var outputFilePath = Path.GetFileNameWithoutExtension(inputFile);
                    var outputFileExt = Path.GetExtension(inputFile);

                    Console.Write(Environment.NewLine);

                    using (var file = new StreamReader(inputFile))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write($"File {fileCounter.ToString().PadLeft(6, '0')}:     ");
                        while ((line = file.ReadLine()) != null)
                        {
                            if (counter >= offset - 1)
                            {
                                var newOutputFilePath = Path.Combine(outputPath, $"{outputFilePath}_{fileCounter.ToString().PadLeft(6, '0')}{outputFileExt}");
                                using (var outputFile = new StreamWriter(newOutputFilePath, true))
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.Write("▒");
                                    Console.ResetColor();
                                    outputFile.WriteLine(line);
                                    outputFile.Close();
                                    counter++;
                                    if (counter == lines)
                                    {
                                        counter = 0;
                                        fileCounter++;
                                        Console.WriteLine("");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.Write($"File {fileCounter.ToString().PadLeft(6, '0')}:     ");
                                    }
                                }
                            }
                            else
                                counter++;
                        }

                        file.Close();
                    }
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine("Operation completed!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                }
            }

            Console.ReadKey();
        }
    }
}