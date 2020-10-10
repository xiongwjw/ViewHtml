using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UIServiceProtocol
{
    public interface IStepPanel
    {
        string ShowPage( object argElement, 
                         string argStepIndex);

        int TotalCount 
        { 
            get; 
        }
    }
}
