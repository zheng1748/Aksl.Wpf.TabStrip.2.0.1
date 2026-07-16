using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;
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

        public Type ViewElementType
        {
            get
            {
                if (field is null)
                {
                    if (!string.IsNullOrEmpty(_tabHeaderedContentInformation.ViewName))
                    {
                        string viewTypeAssemblyQualifiedName = _tabHeaderedContentInformation.ViewName;
                        field = Type.GetType(viewTypeAssemblyQualifiedName);
                    }
                }

                return field;
            }
        }

        public DependencyObject ViewElement
        {
            get
            {
                if (field is null)
                {
                    if (ViewElementType is not null)
                    {
                         field = Activator.CreateInstance(ViewElementType) as DependencyObject;
                    }
                }

                return field;
            }
            set
            {
                SetProperty<DependencyObject>(ref field, value);
            }
        }

        public bool IsSelected
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (ViewElement is not null)
                    {
                        (ViewElement as UIElement)?.Visibility = field ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
            }
        }

        public Visibility ViewElementVisibility
        {
            get => field;
            set
            {
                if (SetProperty<Visibility>(ref field, value))
                {
                    (ViewElement as UIElement)?.Visibility=value;
                }
            }
        }
        #endregion
    }
}
