using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MscriptDll
{
    public struct ClassPair
    {
        public string className;
        public bool UsingInheritance;
        public string InheritanceClassName;
        public string namespaceName;

        public ClassPair(string n, bool i, string ic, string nn)
        {
            className = n;
            UsingInheritance = i;
            InheritanceClassName = ic;
            namespaceName = nn;
        }
    }
}
