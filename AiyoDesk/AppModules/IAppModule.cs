using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AiyoDesk.AppModules;
internal interface IAppModule
{
    string ModuleName { get; set; }
    string ModuleDescription { get; set; }
    bool ModuleRunning { get; }
    bool ModuleInstalled { get; }

    Task ModuleActivate();
    Task ModuleStop();
}
