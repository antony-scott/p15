using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace p15.ViewModels
{
    public class MenuItemViewModel
    {
        public string Header { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }
        public object Icon { get; set; }
        public ObservableCollection<MenuItemViewModel> Items { get; } = new ObservableCollection<MenuItemViewModel>();
    }
}
