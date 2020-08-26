using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMDiagramBuilder.Models
{
    public class FunctionModel : ObservableObject
    {
        private string _expression;
        public string Expression
        {
            get => string.IsNullOrEmpty(_expression) ? string.Empty : _expression;
            set => OnPropertyChanged(ref _expression, value);
        }
        private double _maxX;
        public double MaxX
        {
            get => _maxX > 1.0 ? _maxX : 1.0;
            set => OnPropertyChanged(ref _maxX, value);
        }
        public FunctionModel()
        {
            _expression = "cos(x)";
            MaxX = 10.0;
        }
    }
}
