using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Veg.Admin.Client
{
    public class ViewModel : INotifyPropertyChanged
    {
        public ViewModel()
        {
            KeepActive = true;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string propertyName = null, bool fileChanged = false)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public bool KeepActive { get; set; }
    }
}
