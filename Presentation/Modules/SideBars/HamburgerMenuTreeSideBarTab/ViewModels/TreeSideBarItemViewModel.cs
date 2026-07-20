using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.TabHeaderedContent;
using Aksl.TabHeaderedContent.ViewModels;
using Aksl.TabStrip;
using Aksl.TabStrip.ViewModels;
using Aksl.TabStrip.Views;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Commands;
using Prism.Common;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;

namespace Aksl.Modules.HamburgerMenuTreeSideBarTab.ViewModels
{
    public class TreeSideBarItemViewModel : Mvvm.NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public TreeSideBarItemViewModel() : base()
        {
            _menuItem = null;
        }

        public TreeSideBarItemViewModel(MenuItem menuItem) : base(menuItem.Name, menuItem.Title, null)
        {
            _menuItem = menuItem;

            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
        }

        public TreeSideBarItemViewModel(MenuItem menuItem, TreeSideBarItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _menuItem = menuItem;

            _eventAggregator = PrismUnityExtensions.GetEventAggregator();
            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
        }

        //public TreeSideBarItemViewModel(IEventAggregator eventAggregator, MenuItem menuItem) : this(eventAggregator, menuItem, null)
        //{
        //}

        //public TreeSideBarItemViewModel(IEventAggregator eventAggregator, MenuItem menuItem, TreeSideBarItemViewModel parent)
        //{
        //    _eventAggregator = eventAggregator;
        //    _menuItem = menuItem;
        //    _parent = parent;

        //    _children = new((from child in _menuItem.SubMenus
        //                     select new TreeSideBarItemViewModel(eventAggregator, child, this)).ToList<TreeSideBarItemViewModel>());
        //}
        #endregion

        #region Properties 
        public MenuItem MenuItem => _menuItem;
        public bool HasSubMenu =>
                _menuItem.HasNextSubMenu();
        public bool IsNavigationToRightTabContent =>
                            IsLeaf && _menuItem.HasNextSubMenu() && _menuItem.HasViewName() && _menuItem.IsNexApplication;
        public bool IsAddViewsToRightTabContent =>
                             IsLeaf && _menuItem.HasNextSubMenu() && !_menuItem.HasViewName() && !_menuItem.IsNexApplication;
        public bool IsAddViewToRightTabContent =>
                             IsLeaf && !_menuItem.HasNextSubMenu() && _menuItem.HasViewName() && !_menuItem.IsNexApplication;

        public bool IsSelected
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    int level = Level;

                    //if (field && IsLeaf)
                    //{
                    //    AddViewToRightTabContent().Await();
                    //}

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
                }
            }
        }

        public bool IsExpanded
        {
            get => field;
            set
            {
                SetProperty<bool>(ref field, value);

                if (field && Parent is not null)
                {
                    if (!(Parent as TreeSideBarItemViewModel).IsExpanded)
                    {
                        (Parent as TreeSideBarItemViewModel).IsExpanded = true;
                    }
                }
            }
        } = false;

        public PackIconKind IconKind =>
                  _menuItem.IconKind.ToPackIconKind();

        public bool IsEnabled
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    foreach (var children in this.Children)
                    {
                        (children as TreeSideBarItemViewModel).IsEnabled = field;
                    }
                }
            }
        } = true;
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

                var currentView = tabHeaderedContentViewModel.GetStoreViewElementByName(menuItem.Name);
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

                await _dialogViewService.AlertAsync(message: $"Unable to find \"{msg}\".", title: $"Error:Missing Type");
            }
        }
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
                    bool isSetFirst = false;
                    TabViewModel mainTabViewModel = default;
                    var mainTabView = tabHeaderedContentViewModel.GetStoreViewElementByName(menuItem.Name) as TabView;
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

                        await InitializeMainTabViewCoreAsync();
                    }
                    else
                    {
                        mainTabViewModel = mainTabView.DataContext as TabViewModel;
                        CreateTopTabHeaderedContent(menuItem, tabHeaderedContentViewModel);
                        await InitializeMainTabViewCoreAsync();
                    }

                    async Task InitializeMainTabViewCoreAsync()
                    {
                        IEnumerable<Aksl.Infrastructure.MenuItem> subMenus = await menuItem.GetNextSubMenuAsync();
                        foreach (var smi in subMenus)
                        {
                            var subTabView = mainTabViewModel.GetStoreViewElementByName(smi.Name) as TabView;
                            if (subTabView is null)
                            {
                                isSetFirst = true;
                                CreateSubTopTabView(smi, mainTabViewModel);
                                await AddSubTabViewAsync(smi, mainTabViewModel);
                            }
                            else
                            {
                                CreateSubTopTabView(smi, mainTabViewModel);
                                await AddSubTabViewAsync(smi, mainTabViewModel);
                            }
                        }
                    }
                    if (isSetFirst)
                    {
                        mainTabViewModel.SetFirstActiveTabItem();
                    }
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

        private void CreateTopTabHeaderedContent(MenuItem menuItem, TabHeaderedContentViewModel tabHeaderedContentViewModel)
        {
            TabHeaderedContentInformation tabHeaderedContentInfo = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                ViewName = menuItem.ViewName
            };

            var currentView = tabHeaderedContentViewModel.GetStoreViewElementByName(menuItem.Name);
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

                var currentView = tabHeaderedContentViewModel.GetStoreViewElementByName(menuItem.Name);
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
    }
}