using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using Unity;

namespace Aksl.TabHeaderedContent.ViewModels 
{
    public class TabHeaderedContentViewModel : BindableBase
    {
        #region Members
        #endregion

        #region Constructors
        public TabHeaderedContentViewModel()
        {
            TabHeaderViewModel = new();
            TabContentViewModel = new();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        public TabHeaderViewModel TabHeaderViewModel { get; set; }

        public TabContentViewModel TabContentViewModel { get; set; }

        public bool HasContent
        {
            get
            { 
                return TabContentViewModel.ActiveTabContentItems is not null &&TabContentViewModel.ActiveTabContentItems.Any();
            }
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            TabHeaderViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is TabHeaderViewModel thvm)
                {
                    if (e.PropertyName == nameof(TabHeaderViewModel.SelectedTabHeaderItem))
                    {
                        if (thvm.SelectedTabHeaderItem is not null &&  thvm.SelectedTabHeaderItem.IsSelected)
                        {
                            TabContentViewModel.SetTabItemOnSelected(thvm.SelectedTabHeaderItem.TabHeaderedContentInformation);
                        }
                    }
                }
            };

            TabContentViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is TabContentViewModel tvvm)
                {
                    if (e.PropertyName == nameof(TabContentViewModel.ActiveTabContentItems))
                    {
                        RaisePropertyChanged(nameof(HasContent));
                    }
                }
            };
        }
        #endregion

        #region Methods
        public void Add(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            TabHeaderViewModel.Add(tabHeaderedContentInformation);

            TabHeaderViewModel.RequestClose += (sender, e) =>
            {
                if (sender is TabHeaderItemViewModel tabHeaderItemViewModel)
                {
                    TabContentViewModel.SetTabItemOnClose(tabHeaderItemViewModel.TabHeaderedContentInformation);
                }
            };

            TabContentViewModel.Add(tabHeaderedContentInformation);
        }
       
        public void SetTabItem(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            TabHeaderViewModel.SetTabHeaderItem(tabHeaderedContentInformation);

            TabContentViewModel.SetTabContentItem(tabHeaderedContentInformation);
        }

        public void RetsetTabItem(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            TabHeaderViewModel.RetsetTabItem(tabHeaderedContentInformation);

            TabContentViewModel.RetsetTabItem(tabHeaderedContentInformation);
        }

        public void SetActiveContentItemByName(string name)
        {
             TabContentViewModel.SetActiveTabContentItemByName(name);
        }

        public System.Windows.DependencyObject GetStoreViewElementByType(Type viewType)
        {
            var viewElement = TabContentViewModel.GetStoreViewElementByType(viewType);

            return viewElement;
        }

        public System.Windows.DependencyObject? GetStoreTabContentViewElementByName(string name)
        {
            var viewElement = TabContentViewModel.GetStoreViewElementByName(name);

            return viewElement;
        }

        public TabContentItemViewModel GetStoreTabContentItemByName(string name)
        {
            var storeTabContentItem = TabContentViewModel.GetStoreTabContentItemViewModelByName(name);

            return storeTabContentItem;
        }

        public bool IsActiveTabItem(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            var isActive = TabHeaderViewModel.IsActiveTabItem(tabHeaderedContentInformation);

            return isActive;
        }

        public bool IsActiveTabItemByName(string name)
        {
            var isAny = TabHeaderViewModel.IsActiveTabItemByName(name);

            return isAny;
        }
        #endregion

        #region Contain Methods
        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isEquals = (!string.IsNullOrEmpty(nameOrTitle) && nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                           (!string.IsNullOrEmpty(otherNameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

            return isEquals;
        }
        #endregion
    }
}
