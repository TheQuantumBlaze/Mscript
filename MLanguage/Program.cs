﻿using System;  
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using MscriptDll;

namespace MLanguage
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Mscript Compiler");
            string dir = "";
            bool debug = false;
            List<string> dlls = new List<string>();
            while (true)
            {
                string command = Console.ReadLine();
                string[] splitCommand = command.Split(' ');
                if (splitCommand[0] == "cd" && splitCommand.Length > 1)
                {
                    dir = splitCommand[1];
                    Console.WriteLine("Dir changed to " + splitCommand[1]);
                }
                else if (splitCommand[0] == "cd" && splitCommand.Length <= 1)
                {
                    Console.WriteLine("No Directory specified");
                }
                if (splitCommand[0] == "exit")
                {
                    break;
                }
                if (splitCommand[0] == "dlls")
                {
                    if (splitCommand[1] == "list")
                    {
                        for (int i = 0; i < dlls.Count; i++)
                        {
                            Console.WriteLine(i + ") " + dlls[i]);
                        }
                    }
                    if (splitCommand[1] == "add" && splitCommand.Length > 2)
                    {
                        dlls.Add(splitCommand[2]);
                    }
                    if (splitCommand[1] == "remove" && splitCommand.Length > 2)
                    {
                        int a = int.Parse(splitCommand[2]);
                        if (a < dlls.Count)
                        {
                            dlls.RemoveAt(a);
                        }
                    }
                }
                if (splitCommand[0] == "debug")
                {
                    debug = !debug;
                    Console.WriteLine("Debug: " + debug.ToString());
                }
                if (splitCommand[0] == "compile")
                {
                    new Mscript(dir, debug, false);
                }
            }
        }
    }
}
