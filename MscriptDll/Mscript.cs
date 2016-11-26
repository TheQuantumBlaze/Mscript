using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace MscriptDll
{
    public class Mscript
    {
        public static Stack<ClassPair> classes;
        public static string LatestNamespace = "";
        public Mscript(string dir, bool debug, bool isDll, List<string> importedDlls, string name)
        {
            classes = new Stack<ClassPair>();
            List<string> dlls = new List<string>();
            dlls.AddRange(importedDlls);
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

                if(!otherdata.Contains("package"))
                {
                    file += "namespace MainNamespace {";
                    LatestNamespace = "MainNamespace";
                }
                if (!otherdata.Contains("class"))
                {
                    file += "public class MainCode {";
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
                        if (s.namespaceName != "MainNamespace")
                        {
                            file += "namespace " + s.namespaceName + "{ public partial class " + s.className + (s.UsingInheritance ? " : " + s.InheritanceClassName : "") + " {" + inserteddata + "}}";
                        }
                        else
                        {
                            file += "public partial class " + s.className + (s.UsingInheritance ? " : " + s.InheritanceClassName : "") + " {" + inserteddata + "}";
                        }
                    }
                }

                if (!otherdata.Contains("class"))
                {
                    file += "}";
                }
                if(!otherdata.Contains("package"))
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
                if (!isDll)
                {
                    var paramsters = new CompilerParameters(dlls.ToArray(), dir + "/Bin/" + name + ".exe");
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
                }
                else
                {
                    var paramsters = new CompilerParameters(dlls.ToArray(), dir + "/Bin/" + name + ".dll");
                    paramsters.GenerateExecutable = false;
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

        public static void addToClasses(ClassPair cp)
        {
            classes.Push(cp);
        }
    }
}
