using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAC.DataService.DocFlow
{
    class TDocFlowPathFolder
    {
        
        public int dfPathFoldersId { get; private set; }
        public int dfClassesId { get; private set; }
        public int dfTypesId { get; private set; }
        public string Caption { get; private set; }
        public string StateName { get; private set; }
        public bool Active { get; private set; }
        public bool IsStartup { get; private set; }
        public int EditdfMethodsId { get; private set; }
        public bool AllowEdit { get; private set; }
        public bool AllowDelete { get; private set; }
        public bool AllowDesktop { get; private set; }
        public bool FullAccessView { get; private set; }
        public string NonStopRule { get; private set; }
        public int ImageIndex { get; private set; }
        public int AppDialogsLayoutIdApply { get; private set; }
        public int AppDialogsLayoutIdFinish { get; private set; }
        public int bgColor { get; private set; }
        public int FontColor { get; private set; }
        public int FontStyle { get; private set; }
        public bool EnableMethodSelection { get; private set; }
        public bool IsNonStop { get; private set; }
        public bool ShowForEach { get; private set; }
        public int UserInformation { get; private set; }
        public bool AutoRoute { get; private set; }
        public int AppDialogsLayoutIdEdit { get; private set; }

        public void Map()
        {
            /*dfPathFoldersId
                dfClassesId
            dfTypesId
            Caption
                CodeName
                Active
                IsStartup
                EditdfMethodsId
                AllowEdit
                AllowDelete
                AllowDesktop
                FullAccessView
                NonStopRule
                ImageIndex
                AppDialogsLayoutIdApply
                AppDialogsLayoutIdFinish
                bgColor
                FontColor
                FontStyle
                EnableMethodSelection
                IsNonStop
                ShowForEach
                UserInformation
                AutoRoute
                AppDialogsLayoutIdEdit
            */
        }
    }
}
