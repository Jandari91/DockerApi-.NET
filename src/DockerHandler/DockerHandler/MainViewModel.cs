using DockerApiLib;
using DockerHandler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DockerHandler
{
    public class MainViewModel : PropertyChangedHelper
    {

        public string InputText
        {
            get { return _inputText; }
            set { SetField(ref _inputText, value, "ItemsSource"); }
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
        public ICommand InputCommand { get; set; }
        public ICommand RefreshCommand { get; set; }

        private string _inputText = string.Empty;
        private DockerApi dockerApi;
        private string dockerImage = "Worker";
        public MainViewModel()
        {
            RemoveCommand = new DelegateCommand(RemoveCommandAction);
            InputCommand = new DelegateCommand(InputCommandAction);
            RefreshCommand = new DelegateCommand(RefreshCommandAction);

            var Address = new Address("npipe://./pipe/docker_engine");
            dockerApi = new DockerApi(Address);
        }



        private async void InputCommandAction()
        {
            if (InputText != null)
            {
                var result = await dockerApi.CreateContainerAsync(new ContainerCreateData("worker", InputText));

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
    }
}
