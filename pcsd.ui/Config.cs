using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using pcsd.ui.Annotations;

namespace pcsd.ui
{
    class Config: INotifyPropertyChanged
    {
        private string _displayName;    

        [Category("Name")]
        [DisplayName(@"Display name")]
        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                _displayName = value; 
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        [Category("Settings")]
        [DisplayName(@"PureCloud Client-Id")]
        public string ClientId { get; set; } = string.Empty;

        [Category("Settings")]
        [DisplayName(@"PureCloud Client-Secret")]
        public string ClientSecret { get; set; } = string.Empty;

        [Category("Settings")]
        [DisplayName(@"PureCloud Environment")]
        [Description("e.g. mypurecloud.ie")]
        public string Environment { get; set; } = string.Empty;

        [Category("Settings")]
        [DisplayName(@"Target SQL")]
        [Description("SQL Server connection string")]
        public string TargetSql { get; set; } = string.Empty;

        [Category("Settings")]
        [DisplayName(@"Target CSV")]
        [Description("CSV settings")]
        public string TargetCsv { get; set; } = string.Empty;

        [Category("Settings")]
        [DisplayName(@"Statistics")]
        [Description("e.g. conversations,queues")]
        public string Stats { get; set; } = string.Empty;

        [Category("Settings")]
        [DisplayName(@"Start Date")]
        public DateTime? StartDate { get; set; } = null;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    }
}
