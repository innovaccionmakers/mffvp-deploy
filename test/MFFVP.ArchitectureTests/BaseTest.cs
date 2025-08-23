using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NetArchTest.Rules;
using Shouldly;
using static System.Net.Mime.MediaTypeNames;

namespace MFFVP.ArchitectureTests
{
    public abstract class BaseTest
    {
        public string currentModule = "";
        #region Librerias Modulos

        public enum Monolith
        {
            Associate,
            Closing,
            Customers,
            Operations,
            Products,
            Security,
            Treasury,
            Trusts
        }
        
        public readonly string[] AssociateModules =
        [
            "Associate.Application",
            "Associate.Domain",
            "Associate.Infrastructure",
            "Associate.Presentation"
        ];

        public readonly string[] ClosingModules =
        [
            "Closing.Application",
            "Closing.Domain",
            "Closing.Infrastructure",
            "Closing.Presentation"
        ];

        public readonly string[] CustomerModules =
        [
            "Customers.Application",
            "Customers.Domain",
            "Customers.Infrastructure",
            "Customers.Presentation"
        ];

        public readonly string[] OperationsModules =
        [
            "Operations.Application",
            "Operations.Domain",
            //"Operations.Infrastructure",
            "Operations.Infrastructure",
            "Operations.Presentation"
        ];

        public readonly string[] ProductsModules =
        [
            "Products.Application",
            "Products.Domain",
            "Products.Infrastructure",
            "Products.Presentation"
        ];

        public readonly string[] SecurityModules =
        [
            "Security.Application",
            "Security.Domain",
            "Security.Infrastructure",
            "Security.Presentation"
        ];

        public readonly string[] TreasuryModules =
        [
            "Treasury.Application",
            "Treasury.Domain",
            "Treasury.Infrastructure",
            "Treasury.Presentation"
        ];

        public readonly string[] TrustsModules =
        [
            "Trusts.Application",
            "Trusts.Domain",
            "Trusts.Infrastructure",
            "Trusts.Presentation"
        ];


        

        #endregion
        protected BaseTest() { }

        protected static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        protected void CheckDependencyModule(string currentModule,string evaluateModule, string[] dependencies)
        {

         var result = Types.InCurrentDomain()
                .That()
                .ResideInNamespaceStartingWith(currentModule)
                .ShouldNot()
                .HaveDependencyOnAny(dependencies)
                .GetResult();
           
         var failingTypeNames = GetTypesFalling(result);
            
            result.IsSuccessful.ShouldBeTrue(
                $"El módulo '{currentModule}' no debe depender de módulos de {evaluateModule} " +
                (failingTypeNames.Any()
                    ? $"Revisar : {string.Join(", ", failingTypeNames)}"
                    : string.Empty)
            );


        }


        protected List<string> GetTypesFalling(TestResult? result)
        {
            return  result.IsSuccessful
                ? new List<string>()
                : result.FailingTypes
                    .Where(t => t.FullName != null)
                    .Select(t => t.FullName!)
                    .ToList();
        }


    }
}
