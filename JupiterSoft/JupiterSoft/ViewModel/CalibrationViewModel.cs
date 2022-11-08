using JupiterSoft.Annotations;
using JupiterSoft.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JupiterSoft.ViewModel
{
   public class CalibrationViewModel:INotifyPropertyChanged
    {
        private ObservableCollection<CalibrationModel> logs;

        public ObservableCollection<CalibrationModel> Logs
        {
            get => logs;
            set
            {
                logs = value;
                OnPropertyChanged(nameof(Logs));
            }
        }

        private ObservableCollection<FunctionModel> _functionModels;
        public ObservableCollection<FunctionModel> FunctionModels
        {
            get => _functionModels;
            set
            {
                
                _functionModels = value;
                OnPropertyChanged(nameof(FunctionModels));
            }
        }

        private List<CalibrationModel> _Itemist;

        public List<CalibrationModel> Itemist
        {
            get => _Itemist;
            set
            {
                _Itemist = value;
                OnPropertyChanged(nameof(Itemist));
            }
        }

       public CalibrationViewModel()
        {
            Itemist = new List<CalibrationModel>();
            Logs = new ObservableCollection<CalibrationModel>();
            FunctionModels = new ObservableCollection<FunctionModel>();
            FunctionModels.Add(new FunctionModel { Ftype = (int)functionConstant.Add, FText = functionConstant.Add.ToString().ToUpper() });
            FunctionModels.Add(new FunctionModel { Ftype = (int)functionConstant.Subtract, FText = functionConstant.Subtract.ToString().ToUpper() });
            FunctionModels.Add(new FunctionModel { Ftype = (int)functionConstant.Multiply, FText = functionConstant.Multiply.ToString().ToUpper() });
            FunctionModels.Add(new FunctionModel { Ftype = (int)functionConstant.Divide, FText = functionConstant.Divide.ToString().ToUpper() });
        }

        public void addcalibrationrow()
        {
            if(Itemist!=null && Itemist.Count()>0)
            {
                var newitem = new CalibrationModel();
                newitem.id = Itemist.OrderByDescending(x=>x.id).FirstOrDefault().id+1;
                newitem.OutPutCom = "STEP " + newitem.id;
                newitem.mVal = 0;
                newitem.command = 0;
                newitem.CommandText ="";
                Itemist.Add(newitem);
            }
            else
            {
                var newitem = new CalibrationModel();
                newitem.id = 1;
                newitem.OutPutCom = "STEP " + 1;
                newitem.mVal = 0;
                newitem.command = 0;
                newitem.CommandText = "";
                Itemist.Add(newitem);
            }

            loadItem();
        }

        public void removeCalibration(int id)
        {
            foreach(var item in Itemist.ToList())
            {
                if(item.id==id)
                {
                    Itemist.Remove(item);
                }
            }

            loadItem();
        }

        public void updateCalibrationVal(int id,decimal mval)
        {
            Itemist.Where(x => x.id == id).ToList().ForEach(x => x.mVal = mval);
            loadItem();
        }
        public void updateCalibrationCommand(int id, int command)
        {
            Itemist.Where(x => x.id == id).ToList().ForEach(x => x.command = command);
            switch(command)
            {
                case (int)functionConstant.Add:
                    Itemist.Where(x => x.id == id).ToList().ForEach(x => x.CommandText = functionConstant.Add.ToString());
                    break;
                case (int)functionConstant.Subtract:
                    Itemist.Where(x => x.id == id).ToList().ForEach(x => x.CommandText = functionConstant.Subtract.ToString());
                    break;
                case (int)functionConstant.Multiply:
                    Itemist.Where(x => x.id == id).ToList().ForEach(x => x.CommandText = functionConstant.Multiply.ToString());
                    break;
                case (int)functionConstant.Divide:
                    Itemist.Where(x => x.id == id).ToList().ForEach(x => x.CommandText = functionConstant.Divide.ToString());
                    break;
            }
            loadItem();
        }

        public void loadItem()
        {
            if(Itemist!=null && Itemist.Count()>0)
            {
                Logs = new ObservableCollection<CalibrationModel>();
                int order = 1;
                foreach (var item in Itemist.OrderBy(x=>x.id))
                {
                    item.id = order;
                    item.OutPutCom = "STEP " + order;
                    Logs.Add(item);
                    order++;
                }
            }
            else
            {
                Logs = new ObservableCollection<CalibrationModel>();
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
