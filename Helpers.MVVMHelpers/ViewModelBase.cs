using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Helpers.MVVMHelpers
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region Property INotifyPropertyChanged
        /// <summary>
        /// For signaling that a property has changed on the class.  
        /// Fire when any public property has changed or you want wachers to update.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises a PropertyChanged event on the given Attribute.  
        /// If no attribute is given it uses the name of the property that called this function. 
        /// </summary>
        /// <param name="propertyName">Name of property that changed.  If empty uses the calling property's name.</param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
