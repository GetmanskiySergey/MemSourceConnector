using MemSourceConnector.Api;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemSourceConnector
{
    /// <summary>
    /// Transmem databases wrapper
    /// </summary>
    public class TransmemItem : INotifyPropertyChanged
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                DoPropertyChanged();
            }
        }

        /// <summary>
        /// Transmem DB description
        /// </summary>
        public TM_Content Content { get; set; }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void DoPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion //INotifyPropertyChanged
    }
}
