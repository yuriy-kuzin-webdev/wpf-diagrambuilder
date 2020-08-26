using org.mariuszgromada.math.mxparser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMDiagramBuilder.Components
{
    //https://github.com/mariuszgromada/MathParser.org-mXparser
    public static class ExpressionParser
    {
        public static bool CheckSyntax(string expression)
            => new Expression(expression, new Argument("x = 0")).checkSyntax();
        public static double Calculate(string expression, double param)
            => new Expression(expression, new Argument($"x = {param}")).calculate();  
    }
}
