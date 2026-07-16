using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.TabHeaderedContent.ViewModels
{
    public class TabContentViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Constructors
        public TabContentViewModel()
        {
            _eventAggregator = PrismIocExtensions.GetUnityContainer().Resolve<IEventAggregator>();

            //TabContentItems=new();

            ActiveTabContentItems = new();
            StoreTabContentItems = new();
        }
        #endregion

        #region Properties
        public ObservableCollection<TabContentItemViewModel> ActiveTabContentItems { get; }
        public List<TabContentItemViewModel> StoreTabContentItems { get; }

        public TabContentItemViewModel SelectedTabContentItem
        {
            get => field;
            set => SetProperty<TabContentItemViewModel>(ref field, value);
        }
        #endregion

        #region Methods
        public void Add(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            TabContentItemViewModel newTabContentItemViewModel = new(tabHeaderedContentInformation);

            if (tabHeaderedContentInformation.ViewElement is not null)
            {
                newTabContentItemViewModel.ViewElement = tabHeaderedContentInformation.ViewElement;
            }

            AddCore(newTabContentItemViewModel);
        }

        private void AddCore(TabContentItemViewModel newTabContentItemViewModel)
        {
            if (!IsExistsActivTabContentItems(newTabContentItemViewModel.Name, newTabContentItemViewModel.Title))
            {
                ActiveTabContentItems.Add(newTabContentItemViewModel);
            }

            // AddPropertyChanged();
            void AddPropertyChanged()
            {
                newTabContentItemViewModel.PropertyChanged += (sender, e) =>
                {
                    if (sender is TabContentItemViewModel tcivm)
                    {
                        if (e.PropertyName == nameof(TabContentItemViewModel.IsSelected))
                        {
                            if (SelectedTabContentItem is null && (tcivm is not null && tcivm.IsSelected))
                            {
                                SelectedTabContentItem = tcivm;
                            }

                            if (SelectedTabContentItem is not null && (tcivm is not null && tcivm.IsSelected && tcivm != SelectedTabContentItem))
                            {
                                SelectedTabContentItem.IsSelected = false;

                                SelectedTabContentItem = tcivm;
                            }
                        }
                    }
                };
            }

            StoreTabContentItem(newTabContentItemViewModel);

            SetActiveTabContentItem(newTabContentItemViewModel);

            RaisePropertyChanged(nameof(ActiveTabContentItems));
        }

        private void StoreTabContentItem(TabContentItemViewModel tabContentItemViewModel)
        {
            if (!IsExistsStoreTabContentItems(tabContentItemViewModel.Name, tabContentItemViewModel.Title))
            {
                StoreTabContentItems.Add(tabContentItemViewModel);
            }
        }

        public void SetActiveTabContentItem(TabContentItemViewModel tabContentItemViewModel)
        {
            if (tabContentItemViewModel is not null && !IsEqualsTabContentItemViewModel(tabContentItemViewModel, SelectedTabContentItem))
            {
                if (SelectedTabContentItem is null)
                {
                    SelectedTabContentItem = tabContentItemViewModel;
                    SelectedTabContentItem.IsSelected = true;
                }

                if (SelectedTabContentItem is not null && tabContentItemViewModel != SelectedTabContentItem)
                {
                    SelectedTabContentItem.IsSelected = false;
                    // SelectedTabContentItem.ViewElementVisibility = Visibility.Collapsed;

                    SelectedTabContentItem = tabContentItemViewModel;
                    SelectedTabContentItem.IsSelected = true;
                    //  SelectedTabContentItem.ViewElementVisibility = Visibility.Visible;
                }
            }
        }

        public void SetActiveTabContentItemByName(string name)
        {
            var activeTabContentItem = GetActiveTabContentItemByName(name);
            if (activeTabContentItem is not null)
            {
                SetActiveTabContentItem(activeTabContentItem);
            }
            else
            {
                var storeTabContentItem = GetStoreTabContentItemViewModelByName(name);
                if (storeTabContentItem is not null)
                {
                    AddCore(storeTabContentItem);
                }
            }
        }

        public void SetTabContentItem(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabHeaderedContentInformation);
            if (activeTabContentItem is not null)
            {
                SetActiveTabContentItem(activeTabContentItem);
            }
            else
            {
                var storeTabContentItem = GetStoreTabContentItemViewModelByInfo(tabHeaderedContentInformation);
                if (storeTabContentItem is not null)
                {
                    AddCore(storeTabContentItem);
                }
            }
        }

        public void RetsetTabItem(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabHeaderedContentInformation);
            if (activeTabContentItem is not null)
            {
                activeTabContentItem.ViewElement = null;
                if (tabHeaderedContentInformation.ViewElement is not null)
                {
                    activeTabContentItem.ViewElement = tabHeaderedContentInformation.ViewElement;
                }

                SetActiveTabContentItem(activeTabContentItem);
            }
            else
            {
                var storeContentTabItem = GetStoreTabContentItemViewModelByInfo(tabHeaderedContentInformation);
                if (storeContentTabItem is not null)
                {
                    storeContentTabItem.ViewElement = null;
                    if (tabHeaderedContentInformation.ViewElement is not null)
                    {
                        storeContentTabItem.ViewElement = tabHeaderedContentInformation.ViewElement;
                    }

                    ActiveTabContentItems.Add(storeContentTabItem);

                    SetActiveTabContentItem(storeContentTabItem);
                }
            }
        }

        public void SetTabItemOnClose(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabHeaderedContentInformation);
            if (activeTabContentItem is not null)
            {
                Remove(activeTabContentItem);
            }
        }

        public void Remove(TabContentItemViewModel tabContentItemViewModel)
        {
            if (tabContentItemViewModel is not null)
            {
                if (IsExistsActivTabContentItems(tabContentItemViewModel.Name, tabContentItemViewModel.Title))
                {
                    if (SelectedTabContentItem == tabContentItemViewModel || tabContentItemViewModel.IsSelected)
                    {
                        tabContentItemViewModel.IsSelected = false;
                        SelectedTabContentItem = null;
                    }

                    ActiveTabContentItems.Remove(tabContentItemViewModel);
                }

                if (!ActiveTabContentItems.Any())
                {
                    SelectedTabContentItem = null;
                }

                RaisePropertyChanged(nameof(ActiveTabContentItems));
            }
        }

        public void SetTabItemOnSelected(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var activeTabContentItem = GetActiveTabContentItemByInfo(tabHeaderedContentInformation);
            if (activeTabContentItem is not null)
            {
                SetActiveTabContentItem(activeTabContentItem);
            }
        }

        private TabContentItemViewModel GetActiveTabContentItemByInfo(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var activeTabContentItemViewModel = ActiveTabContentItems.FirstOrDefault(tc => IsEqualsNameOrTitle(tc.Name, tabHeaderedContentInformation.Name) || IsEqualsNameOrTitle(tc.Title, tabHeaderedContentInformation.Title));

            return activeTabContentItemViewModel;
        }

        public TabContentItemViewModel GetActiveTabContentItemByName(string name)
        {
            var activeTabContentItemViewModel = ActiveTabContentItems.FirstOrDefault(tc => IsEqualsNameOrTitle(tc.Name, name) || IsEqualsNameOrTitle(tc.Title, name));

            return activeTabContentItemViewModel;
        }

        public TabContentItemViewModel GetStoreTabContentItemViewModelByInfo(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var storeTabContentItem = StoreTabContentItems.FirstOrDefault(stc => IsEqualsNameOrTitle(stc.Name, tabHeaderedContentInformation.Name) || IsEqualsNameOrTitle(stc.Title, tabHeaderedContentInformation.Title));

            return storeTabContentItem;
        }

        public TabContentItemViewModel GetStoreTabContentItemViewModelByName(string name)
        {
            var storeTabContentItem = StoreTabContentItems.FirstOrDefault(stc => IsEqualsNameOrTitle(stc.Name, name) || IsEqualsNameOrTitle(stc.Title, name));

            return storeTabContentItem;
        }

        public System.Windows.DependencyObject GetStoreViewElementByType(Type viewType)
        {
            var tabContentItem = StoreTabContentItems.FirstOrDefault(stc => stc.ViewElement.GetType() == viewType);

            return tabContentItem?.ViewElement;
        }

        public System.Windows.DependencyObject? GetStoreViewElementByName(string name)
        {
            var storeTabItem = StoreTabContentItems.FirstOrDefault(sti => IsEqualsNameOrTitle(sti.Name, name) || IsEqualsNameOrTitle(sti.Title, name));

            return storeTabItem?.ViewElement;
        }
        #endregion

        #region Contain Methods
        private bool IsExistsActivTabContentItems(string name, string title)
        {
            var isAny = ActiveTabContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsExistsStoreTabContentItems(string name, string title)
        {
            var isAny = StoreTabContentItems.Any(ti => IsEqualsNameOrTitle(ti.Name, name) || IsEqualsNameOrTitle(ti.Title, title));

            return isAny;
        }

        private bool IsEqualsTabContentItemViewModel(TabContentItemViewModel tabContentItemViewModel, TabContentItemViewModel otherTabContentItemViewModel)
        {
            if (tabContentItemViewModel is null || otherTabContentItemViewModel is null)
            {
                return false;
            }

            var isEquals = (IsEqualsNameOrTitle(tabContentItemViewModel.Name, otherTabContentItemViewModel.Name) ||
                            IsEqualsNameOrTitle(tabContentItemViewModel.Title, otherTabContentItemViewModel.Title));

            return isEquals;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase) ||
                           otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase);

            return isEquals;
        }
        #endregion
    }
}
