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

        //private Type _viewElementType = default;
        //public Type ViewElementType
        //{
        //    get
        //    {
        //        if (_viewElementType is null)
        //        {
        //            string viewTypeAssemblyQualifiedName = _tabHeaderedContentInformation.ViewName;
        //            _viewElementType = Type.GetType(viewTypeAssemblyQualifiedName);
        //        }

        //        return _viewElementType;
        //    }
        //}

        //public PackIconKind IconKind
        //{
        //    get
        //    {
        //        PackIconKind kind = PackIconKind.None;

        //        _ = Enum.TryParse(_tabInformation.IconKind, out kind);

        //        return kind;
        //    }
        //}

        public bool IsSelected
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    if (field)
                    {
                        _eventAggregator.GetEvent<OnActiveTabHeaderItemEvent>().Publish(new() { SelectedTabHeaderedContentInfo = _tabHeaderedContentInformation });
                    }
                }
            }
        }

        public Visibility CloseTabButtonVisibility
        {
            get => field;
            set => SetProperty<Visibility>(ref field, value);
        } = Visibility.Visible;
        #endregion

        #region MouseLeftButton Down
        public void ExecuteMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is UserControl uc)
            {
                IsSelected = true;
            }
        }
        #endregion

        #region CloseTab Command
        //public event EventHandler RequestClose;
        private event EventHandler _requestCloseEventHandler;
        public event EventHandler RequestClose
        {
            add { _requestCloseEventHandler += value; }
            remove { _requestCloseEventHandler -= value; }
        }
        public ICommand ExecuteCloseTabCommand { get; private set; }

        private void CreateExecuteCloseTabCommand()
        {
            ExecuteCloseTabCommand = new DelegateCommand(() =>
            {
                _requestCloseEventHandler?.Invoke(this, EventArgs.Empty);
            },
            () =>
            {
                return true;
            });
        }
        #endregion
    }
}
