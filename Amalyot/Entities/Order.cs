using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amalyot.Entities
{
    public class Order : INotifyPropertyChanged
    {
        private int _count; 
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Count
        {
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }
        public double Rate { get; set; } = 5.0;
        public int Desc { get; set; } = 0;
        public decimal Price { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
