using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFFVP.ArchitectureTests
{
    public interface IModuleTest
    {
        void Modules_Should_Not_Depend_On_AssociateModules();
        void Modules_Should_Not_Depend_On_ClosingModules();
        void Modules_Should_Not_Depend_On_OperationsModules();
        void Modules_Should_Not_Depend_On_ProductModules();
        void Modules_Should_Not_Depend_On_CustomerModules();
        void Modules_Should_Not_Depend_On_SecurityModules();
        void Modules_Should_Not_Depend_On_TreasuryModules();
        void Modules_Should_Not_Depend_On_TrustsModules();
    }
}
