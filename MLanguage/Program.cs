using System;  
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;

namespace MLanguage
{
    class Program
    {
        public static Stack<ClassPair> classes;
        static void Main(string[] args)
        {
            classes = new Stack<ClassPair>();

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
                    if (dir != "")
                    {
                        List<string> files = new List<string>();
                        classes.Clear();

                        foreach (string f in Directory.EnumerateFiles(dir))
                        {
                            if (f.EndsWith(".ms"))
                            {
                                using (TextReader tr = new StreamReader(f))
                                    files.Add(tr.ReadToEnd());
                            }
                        }

                        string compination = "";
                        foreach (string s in files)
                        {
                            compination += s;
                        }


                        string data = compination;
                        string inserteddata = "";
                        string headerData = "";

                        using (TextReader tr = new StreamReader(@"code\MLanguageFunctions.txt"))
                        {
                            inserteddata = tr.ReadToEnd();
                        }
                        using (TextReader tr = new StreamReader(@"code\header.txt"))
                        {
                            headerData = tr.ReadToEnd();
                        }
                        LanguageConverter lc = new LanguageConverter();
                        var otherdata = lc.convert(data);

                        string file = "";
                        file += headerData;
                        if (!otherdata.Contains("class"))
                        {
                            file += "namespace MainNamespace { public class MainCode {";
                        }
                        else
                        {
                            file += "namespace MainNamespace {";
                        }
                        for (int i = 0; i < otherdata.Count; i++)
                        {
                            int skip = 0;
                            string insert = CodeComponents.getFromComp(otherdata, i, out skip);
                            if (insert != "")
                            {
                                file += insert;
                                i += skip;
                            }
                            else
                            {
                                file += otherdata[i];
                            }
                        }
                        if (!otherdata.Contains("class"))
                        {
                            file += inserteddata;
                        }
                        else
                        {
                            foreach (ClassPair s in classes)
                            {
                                file += "public partial class " + s.className + (s.UsingInheritance ? " : " + s.InheritanceClassName : "") +" {" + inserteddata + "}";
                            }
                        }

                        if (!otherdata.Contains("class"))
                        {
                            file += "}}";
                        }
                        else
                        {
                            file += "}";
                        }

                        if (debug)
                        {
                            using (TextWriter sw = new StreamWriter(dir + "/conversion.txt"))
                            {
                                sw.WriteLine(file);
                            }
                        }
                        Directory.CreateDirectory(dir + "/Bin");

                        foreach (string s in Directory.EnumerateFiles("code"))
                        {
                            if (s.EndsWith(".dll"))
                            {
                                dlls.Add(s);
                            }
                        }

                        var paramsters = new CompilerParameters(dlls.ToArray(), dir + "/Bin/App.exe");
                        paramsters.GenerateExecutable = true;
                        using (var provider = new CSharpCodeProvider())
                        {
                            CompilerResults cr = provider.CompileAssemblyFromSource(paramsters, new string[] { file });
                            if (cr.Errors.Count > 0)
                            {
                                foreach (var a in cr.Errors)
                                {
                                    Console.WriteLine(a.ToString());
                                }
                            }
                        }

                        foreach (string s in dlls)
                        {
                            File.Copy(s, dir + "/Bin/" + Path.GetFileName(s), true);
                        }


                        Console.WriteLine("Finished Compilation");
                    }
                    else
                    {
                        Console.WriteLine("No Directory Specified Use \"dir location\"");
                    }
                }
            }
        }
    }

    public class LanguageConverter
    {
        List<string> tokens = new List<string>();
        public LanguageConverter()
        {
            tokens.Add("(");
            tokens.Add(")");
            tokens.Add("+");
            tokens.Add("-");
            tokens.Add("*");
            tokens.Add("/");
            tokens.Add("=");
            tokens.Add(">");
            tokens.Add("<");
            tokens.Add("!");
            tokens.Add("|");
            tokens.Add("&");
            tokens.Add("{");
            tokens.Add("}");
            tokens.Add("^");
            tokens.Add("%");
            tokens.Add(",");
            tokens.Add(".");
            tokens.Add(";");
            tokens.Add(":");
            tokens.Add(" ");
        }

        public List<string> convert(string input)
        {
            List<string> data = new List<string>();

            input = input.Replace("\n", " ");
            input = input.Replace("\t", "");

            string current = "";
            bool stringMode = false;
            bool escaped = false;

            foreach (char c in input)
            {
                bool completed = false;
                if (!escaped)
                {
                    if (!stringMode)
                    {
                        foreach (string toks in tokens)
                        {
                            if (c.ToString() == toks)
                            {
                                if (current != "")
                                {
                                    data.Add(current);
                                    current = "";
                                }
                                data.Add(c.ToString());
                                completed = true;
                                break;
                            }
                        }
                    }

                    if (completed)
                    {
                        continue;
                    }
                    if (c == '\\')
                    {
                        escaped = true;
                        current += "\\";
                        continue;
                    }

                    if (c == '\"')
                    {
                        stringMode = !stringMode;
                        if (stringMode)
                        {
                            if (current != "")
                            {
                                data.Add(current);
                                current = "";
                            }
                            current += "\"";
                            continue;
                        }
                        else
                        {
                            current += "\"";
                            data.Add(current);
                            current = "";
                            continue;
                        }
                    }
                }

                current += c.ToString();
                if (escaped)
                    escaped = false;
            }
            return data;
        }
    }

    public class ClassPair
    {
        public string className;
        public bool UsingInheritance;
        public string InheritanceClassName;

        public ClassPair(string n, bool i, string ic)
        {
            className = n;
            UsingInheritance = i;
            InheritanceClassName = ic;
        }
    }

    public static class CodeComponents
    {
        public static string getFromComp(List<string> tokens, int i, out int skip)
        {
            string token = tokens[i];
            if(token == "function")
            {
                if(tokens[i+2] == "main")
                {
                    skip = 4;
                    return "public static void Main(string[] args)";
                }
                bool hasreturns = false;
                int returnsvalue = 0;
                for(int a = i; a < tokens.Count; a++)
                {
                    if(tokens[a] == "returns")
                    {
                        returnsvalue = a;
                        hasreturns = true;
                        break;
                    }
                    if (tokens[a] == "{")
                        break;
                }

                if(hasreturns)
                {
                    string returns = "public static " + tokens[returnsvalue+2] + " " + tokens[i + 2];
                    for (int a = i + 3; a < tokens.Count; a++)
                    {
                        if (tokens[a] != ")")
                        {
                            returns += tokens[a];
                        }
                        else
                        {
                            returns += ")";
                            break;
                        }
                    }
                    skip = returnsvalue + 2 - i;
                    return returns;
                }
                else
                {
                    string returns = "public static void " + tokens[i + 2];
                    int endcount = 0;
                    for (int a = i + 3; a < tokens.Count; a++)
                    {
                        if (tokens[a] != ")")
                        {
                            returns += tokens[a];
                        }
                        else
                        {
                            returns += ")";
                            endcount = a+1;
                            break;
                        }
                    }
                    skip = endcount - i;
                    return returns;
                }
            }
            if(token == "class")
            {
                string returns = "public partial class " + tokens[i + 2];
                Program.classes.Push(new ClassPair (tokens[i + 2],false,""));
                skip = 3;
                return returns;
            }
            if (token == "as")
            {
                skip = 0;
                Program.classes.Peek().UsingInheritance = true;
                Program.classes.Peek().InheritanceClassName = tokens[i + 2];
                return " :";
            }
            if (token == "virtual")
            {
                skip = 0;
                return " ";
            }
            if (token == "override")
            {
                skip = 0;
                return " ";
            }
            if (token == "method")
            {
                bool hasreturns = false;
                int returnsvalue = 0;
                for (int a = i; a < tokens.Count; a++)
                {
                    if (tokens[a] == "returns")
                    {
                        returnsvalue = a;
                        hasreturns = true;
                        break;
                    }
                    if (tokens[a] == "{")
                        break;
                }

                bool isVirtual = false;
                if(i-2 >= 0)
                {
                    if(tokens[i-2] == "virtual")
                    {
                        isVirtual = true;
                    }
                }

                bool isOveride = false;
                if (i - 2 >= 0)
                {
                    if (tokens[i - 2] == "override")
                    {
                        isOveride = true;
                    }
                }

                if (hasreturns)
                {
                    string returns = "public " + (isVirtual ? "virtual ":"") + (isOveride ? "override " : "") + tokens[returnsvalue + 2] + " " + tokens[i + 2];
                    for (int a = i + 3; a < tokens.Count; a++)
                    {
                        if (tokens[a] != ")")
                        {
                            returns += tokens[a];
                        }
                        else
                        {
                            returns += ")";
                            break;
                        }
                    }
                    skip = returnsvalue + 2 - i;
                    return returns;
                }
                else
                {
                    if(tokens[i+2] == "init")
                    {

                        string areturns = "public " + Program.classes.Peek().className;
                        int aendcount = 0;
                        for (int a = i + 3; a < tokens.Count; a++)
                        {
                            if (tokens[a] != ")")
                            {
                                areturns += tokens[a];
                            }
                            else
                            {
                                areturns += ")";
                                aendcount = a + 1;
                                break;
                            }
                        }
                        skip = aendcount - i;
                        return areturns;
                    }
                    string returns = "public "  + (isVirtual ? "virtual " : "") + (isOveride ? "override " : "") + "void " + tokens[i + 2];
                    int endcount = 0;
                    for (int a = i + 3; a < tokens.Count; a++)
                    {
                        if (tokens[a] != ")")
                        {
                            returns += tokens[a];
                        }
                        else
                        {
                            returns += ")";
                            endcount = a + 1;
                            break;
                        }
                    }
                    skip = endcount - i;
                    return returns;
                }
            }
            skip = 0;
            return "";
        }
    }

}
