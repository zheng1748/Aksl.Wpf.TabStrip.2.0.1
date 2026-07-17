using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.TabHeaderedContent;
using Aksl.TabHeaderedContent.ViewModels;
using Aksl.TabStrip;
using Aksl.TabStrip.ViewModels;
using Aksl.TabStrip.Views;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBarTab.ViewModels
{
    public class HamburgerMenuSideBarItemViewModel : Mvvm.NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        private readonly Aksl.Infrastructure.MenuItem _menuItem;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarItemViewModel() : base()
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
            _menuService = PrismUnityExtensions.GetMenuService();

            _menuItem = null;
        }

        public HamburgerMenuSideBarItemViewModel(Aksl.Infrastructure.MenuItem menuItem, HamburgerMenuSideBarItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
            _menuService = PrismUnityExtensions.GetMenuService();

            _menuItem = menuItem;
        }
        #endregion

        #region Properties
        public Aksl.Infrastructure.MenuItem MenuItem => _menuItem;
        public string NavigationName => _menuItem.NavigationName;
        public bool IsSelectedOnInitialize => _menuItem.IsSelectedOnInitialize;
        public PackIconKind IconKind =>
                          _menuItem.IconKind.ToPackIconKind();
        public bool HasSubMenu =>
                           _menuItem.HasNextSubMenu();
        public bool HasViewName =>
                           _menuItem.HasViewName();
        //public bool IsSetLeftPaneActiveContentItem =>
        //                    _menuItem.HasNextSubMenu() && !_menuItem.HasViewName() && !_menuItem.IsNexApplication;
        public bool IsNavigationToRightTabContent =>
                             IsLeaf && _menuItem.HasNextSubMenu() && _menuItem.HasViewName() && _menuItem.IsNexApplication;
        public bool IsAddViewsToRightTabContent =>
                             IsLeaf && _menuItem.HasNextSubMenu() && !_menuItem.HasViewName() && !_menuItem.IsNexApplication;
        public bool IsAddViewToRightTabContent =>
                             IsLeaf && !_menuItem.HasNextSubMenu() && _menuItem.HasViewName() && !_menuItem.IsNexApplication;

        public bool IsSelected
        {
            get;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (field && IsAddViewToRightTabContent)
                    {
                        AddViewToRightTabContentAsync(_menuItem).Await();
                    }

                    if (field && IsAddViewsToRightTabContent)
                    {
                        AddViewsToRightTabContentAsync(_menuItem).Await();
                    }

                    if (field && IsNavigationToRightTabContent)
                    {
                        NavigationToRightTabContentAsync(_menuItem).Await();
                    }

                    //if (field && IsSetLeftPaneActiveContentItem)
                    //{
                    //    SetLeftPaneActiveContentItem();
                    //}

                    //if (field && IsLeaf)
                    //{
                    //    AddViewToRightTabContentAsync().Await();
                    //}
                }
            }
        } = false;

        public bool IsPaneOpen
        {
            get => field;
            set => SetProperty<bool>(ref field, value);
        } = true;

        public bool IsEnabled
        {
            get => field;

            set => SetProperty<bool>(ref field, value);
        } = true;

        private TabViewModel TopTabViewModel { get; set; } = new();
        #endregion

        #region Add Views To Right TabContent Method
        public async Task AddViewsToRightTabContentAsync(MenuItem menuItem)
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            try
            {
                var tabHeaderedContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabHeaderedContentViewModel>(name: ActiveContentNames.TabHeaderedContentHamburgerMenuSideBar);
                if (tabHeaderedContentViewModel.IsActiveTabItemByName(_menuItem.Name))
                {
                    return;
                }

                if (menuItem.HasNextSubMenu())
                {
                    #region Method
                    //TabViewModel topTabViewModel;
                    //var topTabView = tabHeaderedContentViewModel.GetStoreTabContentViewElementByName(_menuItem.Name) as TabView;
                    //if (topTabView is null)
                    //{
                    //    topTabViewModel = new();
                    //    topTabView = new()
                    //    {
                    //        DataContext = topTabViewModel
                    //    };

                    //    topTabViewModel.Add(new()
                    //    {
                    //        Name = _menuItem.Name,
                    //        Title = _menuItem.Title,
                    //        IconKind = _menuItem.IconKind,
                    //        ViewName = _menuItem.ViewName,
                    //        CloseTabButtonVisibility = Visibility.Collapsed
                    //    });

                    //    CreateTopTabHeaderedContent(_menuItem, tabHeaderedContentViewModel);
                    //    var tabContentItemViewModel = tabHeaderedContentViewModel.GetStoreTabContentItemByName(_menuItem.Name);
                    //    tabContentItemViewModel.ViewElement = topTabView;
                    //}
                    //else
                    //{
                    //    CreateTopTabHeaderedContent(_menuItem, tabHeaderedContentViewModel);
                    //    topTabViewModel = topTabView.DataContext as TabViewModel;
                    //}
                    //await AddSubTabViewAsync(_menuItem, topTabViewModel);
                    #endregion

                    TabViewModel mainTabViewModel = default;
                    var mainTabView = tabHeaderedContentViewModel.GetStoreTabContentViewElementByName(menuItem.Name) as TabView;
                    if (mainTabView is null)
                    {
                        mainTabViewModel = new();
                        mainTabView = new()
                        {
                            DataContext = mainTabViewModel
                        };

                        CreateTopTabHeaderedContent(menuItem, tabHeaderedContentViewModel);
                        var tabContentItemViewModel = tabHeaderedContentViewModel.GetStoreTabContentItemByName(menuItem.Name);
                        tabContentItemViewModel.ViewElement = mainTabView;

                        //  await InitializeMainTabViewAsync();

                        await InitializeMainTabViewCoreAsync();
                    }
                    else
                    {
                        mainTabViewModel = mainTabView.DataContext as TabViewModel;

                        CreateTopTabHeaderedContent(_menuItem, tabHeaderedContentViewModel);
                    }

                    async Task InitializeMainTabViewAsync()
                    {
                        TabViewModel topTabViewModel = new();
                        CreateTopTabView(menuItem, topTabViewModel);

                        await AddSubTabViewAsync(menuItem, topTabViewModel);

                        var subTabView = topTabViewModel.StoreTabItems.FirstOrDefault(sti => sti.ViewElement is TabView);
                        if (subTabView is not null)
                        {
                            bool isSetFirst = false;
                            var subTabViewModel = (subTabView.ViewElement as TabView).DataContext as TabViewModel;
                            foreach (var sti in subTabViewModel.StoreTabItems)
                            {
                                var storeTabItem = mainTabViewModel.GetStoreTabItemViewModelByInfo(sti.TabInformation);
                                if (storeTabItem is null)
                                {
                                    mainTabViewModel.Add(new TabInformation() { Name = sti.TabInformation.Name, Title = sti.TabInformation.Title, ViewName = "", ViewElement = sti.ViewElement, CloseTabButtonVisibility = Visibility.Collapsed }, false);
                                    isSetFirst = true;
                                }
                                else
                                {
                                    mainTabViewModel.RetsetTabItemNoActive(sti.TabInformation);
                                }
                            }

                            if (isSetFirst)
                            {
                                subTabViewModel.SetFirstActiveTabItem();
                            }
                        }
                    }

                    async Task InitializeMainTabViewCoreAsync()
                    {
                        IEnumerable<Aksl.Infrastructure.MenuItem> subMenus = await menuItem.GetNextSubMenuAsync();
                        foreach (var smi in subMenus)
                        {
                            var subTabView = mainTabViewModel.GetStoreViewElementByName(menuItem.Name) as TabView;
                            if (subTabView is null)
                            {
                                CreateTopTabView(smi, mainTabViewModel);

                                await AddSubTabViewAsync(smi, mainTabViewModel);
                            }
                            else
                            {
                                CreateTopTabView(smi, mainTabViewModel);
                            }
                        }
                    }
                }
                //else if (_menuItem.HasViewName())
                //{
                //    AddViewToTabContent(_menuItem, tabHeaderedContentViewModel);
                //}
            }
            catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                await dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error:Add Ta View");
            }
            catch (Exception ex)
            {
                await dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Add Ta View");
            }
        }

        private void CreateTopTabHeaderedContent(MenuItem menuItem, TabHeaderedContentViewModel tabHeaderedContentViewModel)
        {
            TabHeaderedContentInformation tabHeaderedContentInfo = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                ViewName = menuItem.ViewName
            };

            var currentView = tabHeaderedContentViewModel.GetStoreTabContentViewElementByName(menuItem.Name);
            if (currentView is not null)
            {
                if (menuItem.IsCacheable)
                {
                    tabHeaderedContentViewModel.SetTabItem(tabHeaderedContentInfo);
                }
                else
                {
                    tabHeaderedContentViewModel.RetsetTabItem(tabHeaderedContentInfo);
                }
            }
            else
            {
                tabHeaderedContentViewModel.Add(tabHeaderedContentInfo);
            }
        }

        private void CreateTopTabView(MenuItem menuItem, TabViewModel tabViewModel)
        {
            TabInformation topTabInfo = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                IconKind = menuItem.IconKind,
                ViewName = menuItem.ViewName,
                CloseTabButtonVisibility = Visibility.Visible
            };

            var currentView = tabViewModel.GetStoreViewElementByName(menuItem.Name);
            if (currentView is not null)
            {
                if (menuItem.IsCacheable)
                {
                    tabViewModel.SetActiveTabItemByName(menuItem.Name);
                }
                else
                {
                    tabViewModel.RetsetTabItem(topTabInfo);
                }
            }
            else
            {
                tabViewModel.Add(topTabInfo);
            }
        }

        private void CreateSubTopTabView(MenuItem menuItem, TabViewModel tabViewModel)
        {
            TabInformation tabInfo = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                IconKind = menuItem.IconKind,
                ViewName = menuItem.ViewName,
                CloseTabButtonVisibility = Visibility.Collapsed
            };

            var currentView = tabViewModel.GetStoreViewElementByName(menuItem.Name);
            if (currentView is not null)
            {
                if (menuItem.IsCacheable)
                {
                }
                else
                {
                    tabViewModel.RetsetTabItemNoActive(tabInfo);
                }
            }
            else
            {
                tabViewModel.Add(tabInfo, false);
            }
        }

        private async Task AddSubTabViewAsync(MenuItem menuItem, TabViewModel topTabViewModel)
        {
            await RecursiveSubMenuItemViewModelAsync(menuItem, topTabViewModel);

            async Task RecursiveSubMenuItemViewModelAsync(MenuItem currentMenuItem, TabViewModel currentTabViewModel)
            {
                var topTabItemViewModel = currentTabViewModel.GetStoreTabItemViewModelByName(currentMenuItem.Name);

                IEnumerable<MenuItem> nextSubMenus = await currentMenuItem.GetNextSubMenuAsync();
                if (nextSubMenus is not null && nextSubMenus.Any())
                {
                    TabViewModel subTabViewModel = new();
                    var subTabView = await FindTabViewByNameAsync(topTabViewModel, currentMenuItem.Name);
                    if (subTabView is null)
                    {
                        subTabView = new TabView
                        {
                            DataContext = subTabViewModel
                        };

                        topTabItemViewModel.ViewElement = subTabView;
                    }
                    else
                    {
                        // Debug.Assert(topTabItemViewModel.ViewElement == subTabView);
                        subTabViewModel = subTabView.DataContext as TabViewModel;
                    }

                    bool isSetFirst = false;

                    foreach (var smi in nextSubMenus)
                    {
                        var leafMenuItems = await smi.GetLeafMenuItems();
                        var isCurrent = leafMenuItems.IsCurrent(smi);

                        foreach (var lmi in leafMenuItems)
                        {
                            if (lmi.HasNextSubMenu())
                            {
                                CreateSubTopTabView(lmi, subTabViewModel);

                                await RecursiveSubMenuItemViewModelAsync(lmi, subTabViewModel);
                            }
                            else if (lmi.HasViewName())
                            {
                                Aksl.TabStrip.TabInformation subTabInformation = new()
                                {
                                    Name = lmi.Name,
                                    Title = lmi.Title,
                                    IconKind = lmi.IconKind,
                                    ViewName = lmi.ViewName,
                                    CloseTabButtonVisibility = Visibility.Collapsed
                                };

                                var currentView = subTabViewModel.GetStoreViewElementByName(lmi.Name);
                                if (currentView is not null)
                                {
                                    if (lmi.IsCacheable)
                                    {
                                    }
                                    else
                                    {
                                        subTabViewModel.RetsetTabItemNoActive(subTabInformation);
                                    }
                                }
                                else
                                {
                                    subTabViewModel.Add(subTabInformation, false);
                                    isSetFirst = true;
                                }
                            }
                        }
                    }

                    if (isSetFirst)
                    {
                        subTabViewModel.SetFirstActiveTabItem();
                    }
                }
            }
        }

        private async Task<TabView> FindTabViewByNameAsync(TabViewModel topTabViewModel, string name)
        {
            TabView findTabView = default;

            await RecursiveSubMenuItemViewModel(topTabViewModel);
            async Task RecursiveSubMenuItemViewModel(TabViewModel currentTabViewModel)
            {
                var subTabViewModels = currentTabViewModel.StoreTabItems.Where(sti => sti.ViewElement is TabView).ToList();
                foreach (var subtvm in subTabViewModels)
                {
                    if (subtvm.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        findTabView = subtvm.ViewElement as TabView;
                        return;
                    }
                    else
                    {
                        var nextTabViewModel = (subtvm.ViewElement as TabView).DataContext as TabViewModel;

                        await RecursiveSubMenuItemViewModel(nextTabViewModel);
                    }
                }
            }

            return findTabView;
        }
        #endregion

        #region Add View To Right TabContent Method
        private async Task AddViewToRightTabContentAsync(MenuItem menuItem)
        {
            try
            {
                var tabHeaderedContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabHeaderedContentViewModel>(name: ActiveContentNames.TabHeaderedContentHamburgerMenuSideBar);
                if (tabHeaderedContentViewModel.IsActiveTabItemByName(_menuItem.Name))
                {
                    return;
                }

                var viewTypeName = menuItem.GetViewTypeName();

                TabHeaderedContentInformation tabHeaderedContentInfo = new()
                {
                    Name = menuItem.Name,
                    Title = menuItem.Title,
                    IconKind = menuItem.IconKind,
                    ViewName = menuItem.ViewName
                };

                var currentView = tabHeaderedContentViewModel.GetStoreTabContentViewElementByName(menuItem.Name);
                if (currentView is not null)
                {
                    if (menuItem.IsCacheable)
                    {
                        tabHeaderedContentViewModel.SetTabItem(tabHeaderedContentInfo);
                    }
                    else
                    {
                        tabHeaderedContentViewModel.RetsetTabItem(tabHeaderedContentInfo);
                    }
                }
                else
                {
                    tabHeaderedContentViewModel.Add(tabHeaderedContentInfo);
                }
            }
            catch (Exception ex)
            {
                string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

               await  _dialogViewService.AlertAsync(message: $"Unable to find \"{msg}\".", title: $"Error:Missing Type");
            }
        }
        #endregion

        #region Navigation To RightTabContent Method
        public async Task NavigationToRightTabContentAsync(MenuItem menuItem)
        {
            try
            {
                var tabHeaderedContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabHeaderedContentViewModel>(name: ActiveContentNames.TabHeaderedContentHamburgerMenuSideBar);
                if (tabHeaderedContentViewModel.IsActiveTabItemByName(_menuItem.Name))
                {
                    return;
                }

                var viewTypeName = menuItem.GetViewTypeName();

                TabHeaderedContentInformation tabHeaderedContentInfo = CreateTabInformation(menuItem.Name, menuItem.Title, menuItem.ViewName, new() { { "CurrentMenuItem", _menuItem } });

                var currentView = tabHeaderedContentViewModel.GetStoreTabContentViewElementByName(menuItem.Name);
                if (currentView is not null)
                {
                    if (menuItem.IsCacheable)
                    {
                        tabHeaderedContentViewModel.SetTabItem(tabHeaderedContentInfo);
                    }
                    else
                    {
                        tabHeaderedContentViewModel.RetsetTabItem(tabHeaderedContentInfo);
                    }
                }
                else
                {
                    tabHeaderedContentViewModel.Add(tabHeaderedContentInfo);
                }
            }
            catch (Exception ex) when (!string.IsNullOrEmpty(ex.InnerException?.Message))
            {
                await _dialogViewService.AlertAsync(message: $"{ex.InnerException.Message}", title: $"Error:Add Tab View");
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"{ex.Message}", title: $"Error:Add Tab View");
            }
        }


        #region Create TabHeaderedContentInformation Method
        public TabHeaderedContentInformation CreateTabInformation(string name, string title, string viewTypeAssemblyQualifiedName, NavigationParameters navigationParameters)
        {
            Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
            if (viewType is null)
            {
                throw new ArgumentException($"Missing Type {viewTypeAssemblyQualifiedName}");
            }
            var viewName = viewType.Name;

            var unityContainer = PrismIocExtensions.GetUnityContainer();
            var regionNavigationService = unityContainer.Resolve<IRegionNavigationService>();

            TabHeaderedContentInformation tabHeaderedContentInfo = new()
            {
                Name = name,
                Title = title,
                ViewName = viewTypeAssemblyQualifiedName
            };

            var registeredView = unityContainer.Resolve<object>(viewName);
            if (registeredView is FrameworkElement frameworkElement)
            {
                MvvmHelpers.AutowireViewModel(registeredView);

                NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute))
                {
                    Parameters = navigationParameters
                };

                Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
                MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

                tabHeaderedContentInfo.ViewName = null;
                tabHeaderedContentInfo.ViewElement = frameworkElement;
            }

            return tabHeaderedContentInfo;
        }
        #endregion
        #endregion

        #region Set LeftPane Active ContentItem Method
        public void SetLeftPaneActiveContentItem()
        {
            var leftPaneActiveContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<SequenceActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);

            if (leftPaneActiveContentViewModel.ContainItemByName(this.Path))
            {
                leftPaneActiveContentViewModel.SetContentItemByName(this.Path);
            }
            else
            {
                HamburgerMenuSideBarHelper.AddViewsToLeftPaneAsync(this).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                    {
                        await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To LeftContent");
                    });
                });
            }
        }
        #endregion
    }
}

