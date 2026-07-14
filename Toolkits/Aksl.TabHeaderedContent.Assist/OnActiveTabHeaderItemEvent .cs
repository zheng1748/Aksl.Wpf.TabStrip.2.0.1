

using Prism.Events;

namespace Aksl.TabHeaderedContent
{
    public class OnActiveTabHeaderItemEvent : PubSubEvent<OnActiveTabHeaderItemEvent>
    {
        #region Constructors
        public OnActiveTabHeaderItemEvent()
        {
        }
        #endregion

        #region Properties
        public TabHeaderedContentInformation SelectedTabInfo { get; set; }
        #endregion
    }
}