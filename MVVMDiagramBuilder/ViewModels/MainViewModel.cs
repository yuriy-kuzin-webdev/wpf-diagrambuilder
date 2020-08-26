using MVVMDiagramBuilder.Components;
using MVVMDiagramBuilder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MVVMDiagramBuilder.ViewModels
{
    public class MainViewModel
    {
        public FunctionModel Function { get; private set; }
        public DiagramModel Diagram { get; private set; }
        public ICommand DrawCommand { get; set; }
        public MainViewModel()
        {
            Function = new FunctionModel();
            Diagram = new DiagramModel();
            DrawCommand = new RelayCommand(DrawImage, CheckExpression);
        }
        private void DrawImage()
            => Diagram.UpdateData(PrepareData().ToArray());
        private bool CheckExpression()
            => ExpressionParser.CheckSyntax(Function.Expression);

        private IEnumerable<(double x,double y)> PrepareData()
        {
            double coef = Function.MaxX / Diagram.Quantity;
            for (int i = 0; i <= Diagram.Quantity; i++)
                yield return (
                    x: i * coef,
                    y: ExpressionParser.Calculate(Function.Expression, i * coef));
        }
    }
}
