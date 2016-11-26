using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MscriptDll
{
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
}
