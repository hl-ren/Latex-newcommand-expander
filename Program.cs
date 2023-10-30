using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LatexNewCommandExpansion
{
    class Program
    {
        public static bool isclosing = false;
        public static bool allclose = false;

        static void Main(string[] args)
        {
            PrintTitle();
            Console.CancelKeyPress += new ConsoleCancelEventHandler(Console_CancelKeyPress);
            //Console.WriteLine("CTRL+C,CTRL+BREAK or suppress the application to exit");
            if(args.Length==0)
            {
                Console.WriteLine("Error, no file or directory is found!");
                Console.WriteLine("Please specify the file or the directory contains the files!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }
            foreach (var path in args)
            {
                if (File.Exists(path))
                {
                    // This path is a file
                    ProcessFile(path);
                }
                else if (Directory.Exists(path))
                {
                    // This path is a directory
                    ProcessDirectory(path);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }
            Console.WriteLine("Please press any key to exit!");
            Console.ReadKey();
        }
        static void MainFun()
        {
            Console.WriteLine("Hello, I come from function in class program.");
        }
        static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Canceling");
            if (e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                isclosing = true;
                e.Cancel = true;
            }
        }
        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);
            //// Recurse into subdirectories of this directory.
            //string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            //foreach (string subdirectory in subdirectoryEntries)
            //    ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            //Console.WriteLine("Processed file '{0}'.", path);
            //var directory = Path.GetFullPath(path) + Path.DirectorySeparatorChar;
            var filename = Path.GetFileName(path);
            //var directory = path.Substring(0, path.LastIndexOf('.')) + Path.DirectorySeparatorChar;
            //if (!Directory.Exists(directory))
            //{
            //    Directory.CreateDirectory(directory);
            //}
            //RunFile(path, directory);
            if (filename.Contains(".tex"))
            {
                LatexNewCommandExpansion le = new LatexNewCommandExpansion(path);
                le.Process();
                path = path.Replace(".tex", "_New.tex");
                le.OutputLatex(path);
                //Console.WriteLine("Congratulations, the latex newcommands have been expanded!");
                Console.WriteLine("The new latex file is : " + Path.GetFileName(path) +" of the same directory");
                GC.Collect();
                Console.WriteLine();
            }
        }
        public static bool NotTargetFile(string filename)
        {
            if (filename.Contains("hf-") || filename.Contains("pd-"))
            {
                Console.WriteLine("Effective keyword file");
                return false;
            }
            return true;
        }
        public static void RunFile(string filename,string directory)
        {
            Console.WriteLine("Run file " + filename + " in " + directory);
            var f1 = directory + Path.GetFileName(filename);
            if (File.Exists(f1))
                File.Delete(f1);
            File.Copy(filename, f1);
        }
        public static void PrintTitle()
        {
            Console.WriteLine("******************************************************");
            Console.WriteLine();
            Console.WriteLine("                Latex Newcommands Expander            ");
            Console.WriteLine("                      Version 1.0        ");
            Console.WriteLine();
            Console.WriteLine("  Author: Huilong Ren");
            Console.WriteLine("   Email: Huilongren2012@gmail.com");
            Console.WriteLine("Language: C#");
            Console.WriteLine("  Usages: ");
            Console.WriteLine("    1. Drag the latex file on the exe.");
            Console.WriteLine("    2. LatexNewcommandExpansion.exe file1 file2 ...");
            Console.WriteLine("       where file1 can be latex file or directory");
            Console.WriteLine("       contain latex files");
            Console.WriteLine(" Enjoy!");
            Console.WriteLine("******************************************************");
        }
    }
}
