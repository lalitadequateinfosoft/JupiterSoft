using JupiterSoft.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.ViewModel
{
   public class CalibrationHMIViewModel : INotifyPropertyChanged
    {
        public CalibrationHMIViewModel()
        {
            IsNotRunning = true;
            Zero = 0;
            Span = 0;
            Weight = 0;
            CalculateSpan = false;
            IsRunning = false;
        }

        private bool _isNotRunning;

        public bool IsNotRunning
        {
            get => _isNotRunning;
            set
            {
                _isNotRunning = value;
                OnPropertyChanged(nameof(IsNotRunning));
            }
        }

        private bool _iRunning;

        public bool IsRunning
        {
            get => _iRunning;
            set
            {
                _iRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private bool _CalculateSpan;

        public bool CalculateSpan
        {
            get => _CalculateSpan;
            set
            {
                _CalculateSpan = value;
                OnPropertyChanged(nameof(CalculateSpan));
            }
        }

        



        private decimal _Zero;
        public decimal Zero
        {
            get => _Zero;
            set
            {
                _Zero = value;
                OnPropertyChanged(nameof(Zero));
            }
        }

        private decimal _Span;
        public decimal Span
        {
            get => _Span;
            set
            {
                _Span = value;
                OnPropertyChanged(nameof(Span));
            }
        }

        private decimal _factor;
        public decimal Factor
        {
            get => _factor;
            set
            {
                _factor = value;
                OnPropertyChanged(nameof(Factor));
            }
        }

       

        private decimal _Weight;
        public decimal Weight
        {
            get => _Weight;
            set
            {
                _Weight = value;
                OnPropertyChanged(nameof(Weight));
            }
        }

        private string _Unit;
        public string Unit
        {
            get => _Unit;
            set
            {
                _Unit = value;
                OnPropertyChanged(nameof(Unit));
            }
        }

















        #region property changed event

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
