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
        public bool IsAddViewToTabContent =>
                                          IsLeaf;

        public bool IsSelected
        {
            get;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (field && IsLeaf)
                    {
                        AddViewToRightTabContentAsync().Await();
                    }
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

        #region Add View To RightTab Method
        public async Task AddViewToRightTabContentAsync()
        {
            var dialogViewService = PrismUnityExtensions.GetDialogViewService();

            try
            {
                var tabHeaderedContentViewModel = PrismIocExtensions.GetUnityContainer().Resolve<TabHeaderedContentViewModel>(name: ActiveContentNames.TabHeaderedContentHamburgerMenuSideBar);
                if (tabHeaderedContentViewModel.IsActiveTabItemByName(_menuItem.Name))
                {
                    return;
                }

                if (_menuItem.HasNextSubMenu())
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
                    var mainTabView = tabHeaderedContentViewModel.GetStoreTabContentViewElementByName(_menuItem.Name) as TabView;
                    if (mainTabView is null)
                    {
                        mainTabViewModel = new();
                        mainTabView = new()
                        {
                            DataContext = mainTabViewModel
                        };

                        CreateTopTabHeaderedContent(_menuItem, tabHeaderedContentViewModel);
                        var tabContentItemViewModel = tabHeaderedContentViewModel.GetStoreTabContentItemByName(_menuItem.Name);
                        tabContentItemViewModel.ViewElement = mainTabView;

                        await InitializeMainTabViewAsync();
                    }
                    else
                    {
                        mainTabViewModel = mainTabView.DataContext as TabViewModel;

                        CreateTopTabHeaderedContent(_menuItem, tabHeaderedContentViewModel);
                    }

                    async Task InitializeMainTabViewAsync()
                    {
                        TabViewModel topTabViewModel = new();
                        CreateTopTabView(_menuItem, topTabViewModel);

                        await AddSubTabViewAsync(_menuItem, topTabViewModel);

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
                }
                else if (_menuItem.HasViewName())
                {
                    AddViewToTabContent(_menuItem, tabHeaderedContentViewModel);
                }
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

        private void AddViewToTabContent(MenuItem menuItem, TabHeaderedContentViewModel tabHeaderedContentViewModel)
        {
            try
            {
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

                _dialogViewService.AlertAsync(message: $"Unable to find \"{msg}\".", title: $"Error:Missing Type").Await();
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

                IEnumerable<MenuItem> nextSubMenu = await currentMenuItem.GetNextSubMenuAsync();
                if (nextSubMenu is not null && nextSubMenu.Any())
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

                    foreach (var smi in nextSubMenu)
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
    }
}

