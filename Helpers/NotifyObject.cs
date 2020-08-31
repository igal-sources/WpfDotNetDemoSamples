﻿using System;
using System.ComponentModel;

namespace EventTypesDemo.Helpers
{
    [Serializable]
    public class NotifyObject : INotifyPropertyChanged
    {
        #region Events

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Event Handlers

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
