using DockerApiLib;
using DockerHandler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DockerHandler
{
    public class MainViewModel : PropertyChangedHelper
    {
        private string _inputText = string.Empty;
        public string InputText
        {
            get { return _inputText; }
            set { SetField(ref _inputText, value, "ItemsSource"); }
        }

        private ObservableRangeCollection<string> _logs = new ObservableRangeCollection<string>();
        public ObservableRangeCollection<string> Logs
        {
            get { return _logs; }
            set { SetField(ref _logs, value, "ItemsSource"); }
        }

        private ObservableRangeCollection<ContainerItem> _containerItems = new ObservableRangeCollection<ContainerItem>();
        public ObservableRangeCollection<ContainerItem> ContainerItems
        {
            get { return _containerItems; }
            set { SetField(ref _containerItems, value, "ItemsSource"); }
        }
        
        private ContainerItem _selectedItem;
        public ContainerItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetField(ref _selectedItem, value, "ItemsSource"); }
        }

        public ICommand RemoveCommand { get; set; }
        public ICommand LogCommand { get; set; }
        public ICommand InputCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ExitFilterCommand { get; set; }
        public ICommand MonitoringCommand { get; set; }
        public ICommand AttachStartCommand { get; set; }

        


        private DockerApi dockerApi;
        private string dockerImage = "Worker";
        public MainViewModel()
        {
            RemoveCommand = new DelegateCommand(RemoveCommandAction);
            LogCommand = new DelegateCommand(LogCommandAction);
            InputCommand = new DelegateCommand(InputCommandAction);
            RefreshCommand = new DelegateCommand(RefreshCommandAction);
            ExitFilterCommand = new DelegateCommand(ExitFilterCommandAction);
            MonitoringCommand = new DelegateCommand(MonitoringCommandAction);
            AttachStartCommand = new DelegateCommand(AttachStartCommandAction);


            var Address = new Address("npipe://./pipe/docker_engine");
            dockerApi = new DockerApi(Address);
        }

        private async void ExitFilterCommandAction()
        {
            ContainerItems.Clear();
            var result = await dockerApi.GetExitContainerAsync();
            ContainerItems.AddRange(result.Select(x => new ContainerItem(x.Id, x.Name, x.State)));
        }

        private async void InputCommandAction()
        {
            if (InputText != null)
            {
                var result = await dockerApi.CreateContainerAsync(new ContainerCreateData("worker", InputText));

                RefreshCommandAction();
            }
        }

        private async void AttachStartCommandAction()
        {
            if (InputText != null)
            {
                await dockerApi.ExecContainerAcync((x) => Logs.Add(x),new ContainerCreateData("worker", InputText));

                RefreshCommandAction();
            }
        }
        

        private async void RemoveCommandAction()
        {
            if (SelectedItem != null)
            {
                var result = await dockerApi.RemoveContainerAsync(new ContainerInfo(SelectedItem.Id, SelectedItem.Name));
                RefreshCommandAction();
            }

        }

        private async void RefreshCommandAction()
        {
            ContainerItems.Clear();
            var result = await dockerApi.GetAllContainersAsync();
            ContainerItems.AddRange(result.Select(x=> new ContainerItem(x.Id, x.Name, x.State)));
        }

        private void LogCommandAction()
        {
            if (SelectedItem != null)
            {
                var result = dockerApi.GetContainerLogsAsync(
                    (x) => Logs.Add(x)
                    , new ContainerInfo(SelectedItem.Id, SelectedItem.Name));
                //var result = dockerApi.AttachContainerAsync((x) => Logs.Add(x), new ContainerInfo(SelectedItem.Id, SelectedItem.Name));
            }
        }

        private void MonitoringCommandAction()
        {
            if (SelectedItem != null)
            {
                var result = dockerApi.ExitedContainerHandler(
                    (x) => MessageBox.Show($"{x} is Exited")
                    , new ContainerInfo(SelectedItem.Id, SelectedItem.Name));
            }
        }

    }
}
