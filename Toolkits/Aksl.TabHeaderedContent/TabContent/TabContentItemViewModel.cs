using System;
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
    public class TabContentItemViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly TabHeaderedContentInformation _tabHeaderedContentInformation;
        #endregion

        #region Constructors
        public TabContentItemViewModel(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            _eventAggregator = PrismIocExtensions.GetUnityContainer().Resolve<IEventAggregator>();

            _tabHeaderedContentInformation = tabHeaderedContentInformation;
        }
        #endregion

        #region Properties
        public TabHeaderedContentInformation TabHeaderedContentInformation => _tabHeaderedContentInformation;
        public string Name => _tabHeaderedContentInformation.Name;
        public string Title => _tabHeaderedContentInformation.Title;
        public string ViewName => _tabHeaderedContentInformation.ViewName;

        private Type _viewElementType = default;
        public Type ViewElementType
        {
            get
            {
                if (_viewElementType is null)
                {
                    string viewTypeAssemblyQualifiedName = _tabHeaderedContentInformation.ViewName;
                    _viewElementType = Type.GetType(viewTypeAssemblyQualifiedName);
                }

                return _viewElementType;
            }
        }

        private DependencyObject _viewElement = default;
        public DependencyObject ViewElement
        {
            get
            {
                if (_viewElement is null)
                {
                    if (ViewElementType is not null)
                    {
                         _viewElement = Activator.CreateInstance(ViewElementType) as DependencyObject;
                    }
                }

                return _viewElement;
            }
            set
            {
                SetProperty<DependencyObject>(ref _viewElement, value);
            }
        }

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    //if (ViewElement is not null)
                    //{
                    //    (ViewElement as UIElement).Visibility = _isSelected ? Visibility.Visible : Visibility.Collapsed;
                    //}
                    //if (_isSelected)
                    //{
                        if (ViewElement is not null)
                        {
                            (ViewElement as UIElement).Visibility = _isSelected ? Visibility.Visible : Visibility.Collapsed;
                        }
                    //}
                    //else
                    //{
                    //    if (ViewElement is not null)
                    //    {
                    //        ViewElement = null;
                    //    }
                    //}
                }
            }
        }

        private Visibility _viewElementVisibility = Visibility.Visible;
        public Visibility ViewElementVisibility
        {
            get => (ViewElement as UIElement).Visibility;
            set
            {
                if (SetProperty<Visibility>(ref _viewElementVisibility, value))
                {
                    (ViewElement as UIElement).Visibility=value;
                }
            }
        }
        #endregion
    }
}
