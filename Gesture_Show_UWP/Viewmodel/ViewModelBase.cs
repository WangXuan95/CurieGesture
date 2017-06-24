using System.ComponentModel;

namespace BLEServiceBrowser.Viewmodel
{
    // Base viewmodel class used to override the INotifyPropertyChanged, this allows the UI to notified that data has changed, and to reflect the changes
    public class ViewModelBase : INotifyPropertyChanged
    {
        private bool isBusy;
        public bool IsBusy
        {
            get { return this.isBusy; }
            set
            {
                this.isBusy = value;
                this.RaisePropertyChanged("IsBusy");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void RaisePropertiesChanged(string[] props)
        {
            foreach (var item in props)
            {
                this.RaisePropertyChanged(item);
            }
        }

        protected virtual bool CheckForPropertyChanged<T>(ref T currentValue, T newValue, string propertyName)
        {
            if (currentValue != null && currentValue.Equals(newValue))
            {
                return false;
            }

            currentValue = newValue;

            this.RaisePropertyChanged(propertyName);

            return true;
        }
    }
}
