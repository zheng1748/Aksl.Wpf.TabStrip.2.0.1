using Aksl.ActiveContents.ViewModels;
using Aksl.ActiveContents.Views;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.HamburgerMenuSideBarTab.Views;
using Aksl.TabHeaderedContent.ViewModels;
using Aksl.TabStrip.ViewModels;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarHubViewModel : Aksl.Mvvm.HamburgerMenuViewModel, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
       // private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarHubViewModel()
        {
            _container = PrismIocExtensions.GetUnityContainer();
            _eventAggregator = _container.Resolve<IEventAggregator>();
            _dialogViewService = _container.Resolve<IDialogViewService>();
            _menuService = _container.Resolve<IMenuService>();

            IsPaneOpen = true;
            SelectedDisplayMode = Aksl.Mvvm.SplitViewDisplayMode.CompactInline;
            SelectedPlacement = Aksl.Mvvm.SplitViewPanePlacement.Left;

            RegisterHamburgerMenuBarPaneOpenEvent();
            RegisterActiveContentAsync().Await();
        }
        #endregion

        #region Properties
        public SequenceActiveContentViewModel LeftPaneActiveContentViewModel { get; set; }
        public TabHeaderedContentViewModel TabHeaderedContentViewModel
        {
            get => field;
            set => SetProperty(ref field, value);
        }
        //public TabViewModel TabStripViewModel
        //{
        //    get => field;
        //    set => SetProperty(ref field, value);
        //}
       
        public HamburgerMenuSideBarViewModel TopHamburgerMenuSideBar { get; set; }

        public ActiveContentItemViewModel SelectedLeftPaneActiveContentItem
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public HamburgerMenuSideBarViewModel SelectedHamburgerMenuSideBar
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool IsLoading
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = false;

        //public bool CanMove =>
        //              LeftPaneActiveContentViewModel.CanMove;

        //public Visibility MoveButtonVisibility
        //{
        //    get
        //    {
        //        field = Visibility.Collapsed;
        //        return field;
        //    }
        //} = Visibility.Collapsed;
        #endregion

        #region RegisterPropertyChanged Method
        //private void RegisterPropertyChanged()
        //{
        //    this.PropertyChanged += (sender, e) =>
        //    {
        //        if (sender is HamburgerMenuSideBarHubViewModel hmsbhvm)
        //        {
        //            if (e.PropertyName == nameof(IsLoading))
        //            {
        //               // (MovePreviousCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        //            }
        //        }
        //    };
        //}

        //private void AddLeftPaneActiveContentViewModelPropertyChanged()
        //{
        //    LeftPaneActiveContentViewModel.PropertyChanged += (sender, e) =>
        //    {
        //        if (sender is SequenceActiveContentViewModel savm)
        //        {
        //            if (e.PropertyName == nameof(SequenceActiveContentViewModel.SelectedContentItem))
        //            {
        //                if (SelectedLeftPaneActiveContentItem is null)
        //                {
        //                    SelectedLeftPaneActiveContentItem = savm.SelectedContentItem;

        //                    SetMenuSideBar();
        //                }

        //                if (SelectedLeftPaneActiveContentItem is not null && SelectedLeftPaneActiveContentItem != savm.SelectedContentItem)
        //                {
        //                    SelectedLeftPaneActiveContentItem = null;
        //                    SelectedLeftPaneActiveContentItem = savm.SelectedContentItem;

        //                    SetMenuSideBar();
        //                }
        //            }

        //            if (e.PropertyName == nameof(ActiveContentViewModel.SelectedIndex))
        //            {
        //                RaisePropertyChanged(nameof(CanMove));
        //               // (MovePreviousCommand as DelegateCommand<ActiveContentItemViewModel>)?.RaiseCanExecuteChanged();
        //            }

        //            void SetMenuSideBar()
        //            {
        //                var hamburgerMenuSideBarView = savm.SelectedContentItem.ViewElement as HamburgerMenuSideBarView;
        //                var hamburgerMenuSideBarViewModel = hamburgerMenuSideBarView.DataContext as HamburgerMenuSideBarViewModel;

        //                if (SelectedHamburgerMenuSideBar != hamburgerMenuSideBarViewModel)
        //                {
        //                    SelectedHamburgerMenuSideBar = null;
        //                    SelectedHamburgerMenuSideBar = hamburgerMenuSideBarViewModel;

        //                    if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is null)
        //                    {
        //                       // RightContentActiveContentViewModel.ClearSelectedItem();
        //                    }

        //                    if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is not null &&
        //                        SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem.IsSetLeftPaneActiveContentItem)
        //                    {
        //                        if (hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu is not null &&
        //                            hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu != SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem)
        //                        {
        //                            // SelectedHamburgerMenuSideBarItem = hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu;

        //                            hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu.IsSelected = true;
        //                        }
        //                        else
        //                        {
        //                            //RightContentActiveContentViewModel.ClearSelectedItem();
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is not null &&
        //                            SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem.IsAddViewToRightTabContent)
        //                        {
        //                            ActiveContentManagerExtensions.AddViewToRandomContentAsync(SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        //                            {
        //                                System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        //                                {
        //                                    await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View");
        //                                });
        //                            });
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    };
        //}
        #endregion

        #region HamburgerMenu Properties
        public override bool IsPaneOpen
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (TopHamburgerMenuSideBar is not null)
                    {
                        TopHamburgerMenuSideBar.IsPaneOpen = value;
                    }

                    VisualState = GetVisualState();
                }
            }
        }
        #endregion

        #region Register HamburgerMenuBarPaneOpen Event
        private void RegisterHamburgerMenuBarPaneOpenEvent()
        {
            _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Subscribe(async (hmbpoe) =>
            {
                try
                {
                    IsPaneOpen = hmbpoe.IsPaneOpen;
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Subscribe PaneOpen Event Error.: \"{ex.Message}\"", title: "Error");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register ActiveContents Method
        private async Task RegisterActiveContentAsync()
        {
            RegisterRightTabStrip();
            void RegisterRightTabStrip()
            {
                _container.RegisterSingleton(from: typeof(TabHeaderedContentViewModel), to: typeof(TabHeaderedContentViewModel), name: ActiveContentNames.TabHeaderedContentHamburgerMenuSideBar);
                var tabHeaderedContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabHeaderedContentViewModel>(name: ActiveContentNames.TabHeaderedContentHamburgerMenuSideBar);

                TabHeaderedContentViewModel = tabHeaderedContentViewModel;
            }

            await RegisterLeftPaneActiveContentAsync();
            async Task RegisterLeftPaneActiveContentAsync()
            {
                _container.RegisterSingleton(from: typeof(SequenceActiveContentViewModel), to: typeof(SequenceActiveContentViewModel), name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
                LeftPaneActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<SequenceActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);

                //AddLeftPaneActiveContentViewModelPropertyChanged();

                await CreateTopHamburgerMenuSideBarViewModelAsync();
                //LeftPaneActiveContentViewModel.Add(new()
                //{
                //    Name = "Root",
                //    Title = "Root",
                //    ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarView,Aksl.Modules.HamburgerMenuSideBar",
                //    ViewElement = new HamburgerMenuSideBarView() { DataContext = TopHamburgerMenuSideBar }
                //}, true);
                LeftPaneActiveContentViewModel.Add(new()
                {
                    Name = "HamburgerMenuSideBarView",
                    Title = "HamburgerMenuSideBarView",
                    ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarView,Aksl.Modules.HamburgerMenuSideBar",
                    ViewElement = new Views.HamburgerMenuSideBarView() { DataContext = TopHamburgerMenuSideBar }
                }, true);
            }
        }
        #endregion

        #region Create TopHamburgerMenuSideBar ViewModel Method
        private async Task CreateTopHamburgerMenuSideBarViewModelAsync()
        {
            IsLoading = true;

            try
            {
                #region Method
                var rootMenuItem = await _menuService.GetMenuAsync("All");
                var subMenuItems = rootMenuItem.SubMenus;

                NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();
                TopHamburgerMenuSideBar = new();

                if (subMenuItems is not null && subMenuItems.Any())
                {
                    List<HamburgerMenuSideBarItemViewModel> allSideBarItemLeafs = new();

                    foreach (var mi in subMenuItems)
                    {
                        HamburgerMenuSideBarItemViewModel virtualParent = new();
                        Func<Infrastructure.MenuItem, HamburgerMenuSideBarItemViewModel, HamburgerMenuSideBarItemViewModel> childResolver = ((m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });

                        var topItem = await nodeResolver.GetTopItemByMenuItemAsync(mi, virtualParent, childResolver, false);
                        var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                        allSideBarItemLeafs.AddRange(allTopItemLeafs);
                    }

                    TopHamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allSideBarItemLeafs);
                }
                #endregion

                //var rootMenuItem = await _menuService.GetMenuAsync("All");
                //var subMenuItems = rootMenuItem.SubMenus;
                //TopHamburgerMenuSideBar = await HamburgerMenuSideBarHelper.CreateTopHamburgerMenuSideBarViewModelAsync(subMenuItems);

                TopHamburgerMenuSideBar.IsPaneOpen = IsPaneOpen;
                RaisePropertyChanged(nameof(TopHamburgerMenuSideBar));

            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create top hamburger menu : \"{ex.Message}\"", title: "Error: Create Top HamburgerMenuSideBar");
            }
            finally
            {
                if (IsLoading)
                {
                    IsLoading = false;
                }
            }
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters is not null)
            {
                if (parameters.Count == 0)
                {
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
