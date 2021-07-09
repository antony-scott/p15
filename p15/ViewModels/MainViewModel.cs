using Dock.Model;
using Dock.Model.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace p15.ViewModels
{
    public class MainViewModel : RootDock
    {
        public IDock DefaultPane { get; set; }

        public IDock DefaultToolDock { get; set; }

        public override IDockable Clone()
        {
            var mainViewModel = App.Services.GetService<MainViewModel>();

            CloneHelper.CloneDockProperties(this, mainViewModel);
            CloneHelper.CloneRootDockProperties(this, mainViewModel);

            return mainViewModel;
        }
    }
}
