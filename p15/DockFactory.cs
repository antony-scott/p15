using Avalonia.Data;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Controls;
using p15.ViewModels;
using System;
using System.Collections.Generic;

namespace p15
{
    public class DockFactory : Factory
    {
        private StartViewModel _startViewModel;
        private TraceOutputViewModel _traceOutputViewModel;
        private MainViewModel _mainViewModel;

        public DockFactory(
            StartViewModel startViewModel,
            TraceOutputViewModel traceOutputViewModel,
            MainViewModel mainViewModel)
        {
            _startViewModel = startViewModel;
            _traceOutputViewModel = traceOutputViewModel;
            _mainViewModel = mainViewModel;

            this.HostWindowLocator = new Dictionary<string, Func<IHostWindow>>
            {
                [nameof(IDockWindow)] = () =>
                {
                    var hostWindow = new HostWindow()
                    {
                        [!HostWindow.TitleProperty] = new Binding("ActiveDockable.Title")
                    };
                    return hostWindow;
                }
            };
        }

        public override void CloseDockable(IDockable dockable)
        {
            if (dockable == null) return;
            base.CloseDockable(dockable);
        }

        public override IDock CreateLayout()
        {
            var documentsPane = new DocumentDock
            {
                Id = "DocumentsPane",
                Title = "DocumentsPane",
                Proportion = double.NaN,
                ActiveDockable = _startViewModel,
                VisibleDockables = CreateList<IDockable>
                (
                    _startViewModel
                )
            };

            var outputsPane = new DocumentDock
            {
                Id = "OutputsPane",
                Title = "OutputsPane",
                Proportion = 0.2,
                ActiveDockable = _traceOutputViewModel,
                VisibleDockables = CreateList<IDockable>
                (
                    _traceOutputViewModel
                )
            };

            var leftToolDock = new ToolDock
            {
                Id = "Left",
                Title = "Left",
                Proportion = 0.2,
                VisibleDockables = CreateList<IDockable>(),
                ActiveDockable = null,
                IsCollapsable = false
            };

            var mainLayout = new ProportionalDock
            {
                Id = "Main",
                Title = "Main",
                Proportion = double.NaN,
                Orientation = Orientation.Horizontal,
                ActiveDockable = documentsPane,
                VisibleDockables = CreateList<IDockable>
                (
                    leftToolDock,
                    new SplitterDock
                    {
                        Id = "VerticalSplitter",
                        Title = "VerticalSplitter"
                    },
                    new ProportionalDock
                    {
                        Id = "Right",
                        Title = "Right",
                        Proportion = double.NaN,
                        Orientation = Orientation.Vertical,
                        ActiveDockable = documentsPane,
                        VisibleDockables = CreateList<IDockable>
                        (
                            documentsPane,
                            new SplitterDock
                            {
                                Id = "HorizontalSplitter",
                                Title = "HorizontalSplitter"
                            },
                            outputsPane
                        )
                    }
                )
            };

            _mainViewModel.Id = "Main";
            _mainViewModel.Title = "Main";
            _mainViewModel.ActiveDockable = mainLayout;
            _mainViewModel.VisibleDockables = CreateList<IDockable>(mainLayout);
            _mainViewModel.DefaultPane = documentsPane;
            _mainViewModel.DefaultToolDock = leftToolDock;

            var root = CreateRootDock();

            root.Id = "Root";
            root.Title = "Root";
            root.ActiveDockable = _mainViewModel;
            root.DefaultDockable = _mainViewModel;
            root.VisibleDockables = CreateList<IDockable>
            (
                _mainViewModel
            );

            return root;
        }
    }
}
