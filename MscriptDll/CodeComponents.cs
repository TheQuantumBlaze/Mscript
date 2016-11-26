using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MscriptDll
{
    public class CodeComponents
    {
        public static string getFromComp(List<string> tokens, int i, out int skip)
        {
            string token = tokens[i];
            if (token == "function")
            {
                if (tokens[i + 2] == "main")
                {
                    skip = 4;
                    return "public static void Main(string[] args)";
                }
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

                if (hasreturns)
                {
                    string returns = "public static " + tokens[returnsvalue + 2] + " " + tokens[i + 2];
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
                            endcount = a + 1;
                            break;
                        }
                    }
                    skip = endcount - i;
                    return returns;
                }
            }
            if (token == "class")
            {
                string returns = "public partial class " + tokens[i + 2];
                Mscript.classes.Push(new ClassPair(tokens[i + 2], false, ""));
                skip = 3;
                return returns;
            }
            if (token == "called")
            {
                string returns = "namespace" + tokens[i + 2];
                skip = 3;
                return returns;
            }
            if (token == "as")
            {
                skip = 0;
                ClassPair cp = Mscript.classes.Pop();
                cp.UsingInheritance = true;
                cp.InheritanceClassName = tokens[i + 2];
                Mscript.classes.Push(cp);
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
                if (i - 2 >= 0)
                {
                    if (tokens[i - 2] == "virtual")
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
                    string returns = "public " + (isVirtual ? "virtual " : "") + (isOveride ? "override " : "") + tokens[returnsvalue + 2] + " " + tokens[i + 2];
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
                    if (tokens[i + 2] == "init")
                    {

                        string areturns = "public " + Mscript.classes.Peek().className;
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
                    string returns = "public " + (isVirtual ? "virtual " : "") + (isOveride ? "override " : "") + "void " + tokens[i + 2];
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
