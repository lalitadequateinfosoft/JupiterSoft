﻿using JupiterSoft.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.ViewModel
{
  public  class CheckWeigherViewModel: INotifyPropertyChanged
    {
        private bool _IsWeightEditEnabled;
        public bool IsWeightEditEnabled
        {
            get
            {
                return _IsWeightEditEnabled;
            }
            set
            {
                _IsWeightEditEnabled = value;
                OnPropertyChanged(nameof(IsWeightEditEnabled));
            }
        }


        private bool _IsWeightSaveEnabled;
        public bool IsWeightSaveEnabled
        {
            get
            {
                return _IsWeightSaveEnabled;
            }
            set
            {
                _IsWeightSaveEnabled = value;
                OnPropertyChanged(nameof(IsWeightSaveEnabled));
            }
        }

        private bool _IsWeightConfigured;
        public bool IsWeightConfigured
        {
            get
            {
                return _IsWeightConfigured;
            }
            set
            {
                _IsWeightConfigured = value;
                OnPropertyChanged(nameof(IsWeightConfigured));
            }
        }

        private bool _IsMotorEditEnabled;
        public bool IsMotorEditEnabled
        {
            get
            {
                return _IsMotorEditEnabled;
            }
            set
            {
                _IsMotorEditEnabled = value;
                OnPropertyChanged(nameof(IsMotorEditEnabled));
            }
        }

        private bool _IsMotorSaveEnabled;
        public bool IsMotorSaveEnabled
        {
            get
            {
                return _IsMotorSaveEnabled;
            }
            set
            {
                _IsMotorSaveEnabled = value;
                OnPropertyChanged(nameof(IsMotorSaveEnabled));
            }
        }

        private bool _IsMotorConfigured;
        public bool IsMotorConfigured
        {
            get
            {
                return _IsMotorConfigured;
            }
            set
            {
                _IsMotorConfigured = value;
                OnPropertyChanged(nameof(IsMotorConfigured));
            }
        }

        private bool _IsPushingEditEnabled;
        public bool IsPushingEditEnabled
        {
            get
            {
                return _IsPushingEditEnabled;
            }
            set
            {
                _IsPushingEditEnabled = value;
                OnPropertyChanged(nameof(IsPushingEditEnabled));
            }
        }

        private bool _IsPushingSaveEnabled;
        public bool IsPushingSaveEnabled
        {
            get
            {
                return _IsPushingSaveEnabled;
            }
            set
            {
                _IsPushingSaveEnabled = value;
                OnPropertyChanged(nameof(IsPushingSaveEnabled));
            }
        }

        private bool _IsPushingConfigured;
        public bool IsPushingConfigured
        {
            get
            {
                return _IsPushingConfigured;
            }
            set
            {
                _IsPushingConfigured = value;
                OnPropertyChanged(nameof(IsPushingConfigured));
            }
        }

        private bool _IsPhotoConfigured;
        public bool IsPhotoConfigured
        {
            get
            {
                return _IsPhotoConfigured;
            }
            set
            {
                _IsPhotoConfigured = value;
                OnPropertyChanged(nameof(IsPhotoConfigured));
            }
        }

        private bool _IsPaused;
        public bool IsPaused
        {
            get
            {
                return _IsPaused;
            }
            set
            {
                _IsPaused = value;
                OnPropertyChanged(nameof(IsPaused));
            }
        }

        private bool _IsRunning;
        public bool IsRunning
        {
            get
            {
                return _IsRunning;
            }
            set
            {
                _IsRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public CheckWeigherViewModel()
        {
            IsWeightEditEnabled = true;
            IsWeightSaveEnabled = false;
            IsWeightConfigured = false;

            IsMotorConfigured = false;
            IsMotorEditEnabled = true;
            IsMotorSaveEnabled = false;

            IsPushingConfigured = false;
            IsPushingEditEnabled = true;
            IsPushingSaveEnabled = false;

            IsPhotoConfigured = false;

            IsPaused = false;
            IsRunning = true;
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
