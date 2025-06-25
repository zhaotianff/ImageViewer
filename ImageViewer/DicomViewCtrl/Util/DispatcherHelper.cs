using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DicomViewCtrl.Util
{
    public class DispatcherHelper
    {
        public static void DispatherInvoke(Action action,bool isHightPriority = true)
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;

            if(!dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(isHightPriority ? DispatcherPriority.Send : DispatcherPriority.Background, action);
            }
            else
            {
                action();
            }
        }
    }
}
