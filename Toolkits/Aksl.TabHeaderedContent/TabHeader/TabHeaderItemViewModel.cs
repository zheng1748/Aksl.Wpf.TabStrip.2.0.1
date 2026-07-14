using System;
using System.Windows;
using System.Windows.Controls;
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
    public class TabHeaderItemViewModel : BindableBase
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly TabHeaderedContentInformation _tabHeaderedContentInformation;
        #endregion

        #region Constructors
        public TabHeaderItemViewModel(TabHeaderedContentInformation tabHeaderedContentInformation)
        {
            _eventAggregator = PrismIocExtensions.GetUnityContainer().Resolve<IEventAggregator>();

            _tabHeaderedContentInformation = tabHeaderedContentInformation;

            CloseTabButtonVisibility = _tabHeaderedContentInformation.CloseTabButtonVisibility;

            CreateExecuteCloseTabCommand();
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

        //public PackIconKind IconKind
        //{
        //    get
        //    {
        //        PackIconKind kind = PackIconKind.None;

        //        _ = Enum.TryParse(_tabInformation.IconKind, out kind);

        //        return kind;
        //    }
        //}

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    if (_isSelected)
                    {
                        _eventAggregator.GetEvent<OnActiveTabHeaderItemEvent>().Publish(new() { SelectedTabInfo= _tabHeaderedContentInformation });
                    }
                }
            }
        }

        private Visibility _closeTabButtonVisibility = Visibility.Visible;
        public Visibility CloseTabButtonVisibility
        {
            get => _closeTabButtonVisibility;
            set => SetProperty<Visibility>(ref _closeTabButtonVisibility, value);
        }
        #endregion

        #region MouseLeftButton Down
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(sender is UserControl uc) 
            {
                IsSelected=true; 
            }
        }
        #endregion

        #region CloseTab Command
        public event EventHandler RequestClose;
        public ICommand ExecuteCloseTabCommand { get; private set; }

        private void CreateExecuteCloseTabCommand()
        {
            ExecuteCloseTabCommand = new DelegateCommand(() =>
            {
                RequestClose?.Invoke(this, EventArgs.Empty);
            },
            () =>
            {
                return true;
            });
        }
        #endregion
    }
}
