using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace pcsd.ui
{
    class MainWindowViewModel: ViewModelBase
    {

        #region "var & prop"

        private Config _selectedConfig;
        public Config SelectedConfig
        {
            get { return _selectedConfig; }
            set
            {
                _selectedConfig = value;
                RaisePropertyChanged();
            }
        }
        
        public ObservableCollection<Config> ConfigList { get; } = new ObservableCollection<Config>();

        #endregion

        #region "ctor"
        public MainWindowViewModel()
        {
            Load();
        }
        #endregion

        #region "storage"

        public void Load()
        {
            try
            {
                var loadedConfigList = Storage.Load();
                ConfigList.Clear();
                foreach (var c in loadedConfigList) { ConfigList.Add(c); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Save()
        {
            try
            {
                Storage.Save(ConfigList.ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #endregion

        #region "commands"

        public ICommand AddCommand => new RelayCommand(AddAction);

        private void AddAction()
        {
            ConfigList.Add(new Config() {DisplayName = "New config"});
            Save();
        }

        public ICommand RemoveCommand => new RelayCommand(RemoveAction);

        private void RemoveAction()
        {
            if (SelectedConfig == null) return;            
            ConfigList.Remove(SelectedConfig);
            Save();
        }
        public ICommand ExecCommand => new RelayCommand(ExecAction);

        private void ExecAction()
        {
            if (SelectedConfig == null) return;
            try
            {
                var command = Executor.GetArguments(SelectedConfig);
                if (MessageBox.Show(command, "Do you want to run PCSD with following arguments?", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Executor.Execute(command);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
                
        #endregion
   
    }
}
