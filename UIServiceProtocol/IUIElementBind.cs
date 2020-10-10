using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIServiceProtocol
{
    public interface IUIElementBind
    {
        bool BindProperty( string strElement,
                           string strProperty,
                           object objValue );

        void UpdateData( bool bSave );

        void Reset();
    }
}
