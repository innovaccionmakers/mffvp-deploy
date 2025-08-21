using NetArchTest.Rules;
using Shouldly;
using System.Net.Mime;
using System.Reflection;


namespace MFFVP.ArchitectureTests;

[Collection("TreasuryTest")]
public class TreasuryTest : BaseTest, IModuleTest
{
 
    public TreasuryTest() : base()
    {
        currentModule = nameof(Monolith.Treasury);

        foreach (var module in TreasuryModules)
        {
            var dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{module}.dll");
            if (File.Exists(dllPath))
                Assembly.LoadFrom(dllPath);
        }

  
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_AssociateModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Associate), AssociateModules);
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_ProductModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Products), ProductsModules);
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_OperationsModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Operations), OperationsModules);

    }

    [Fact]
    public void Modules_Should_Not_Depend_On_SecurityModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Security), SecurityModules);
    }

   public void Modules_Should_Not_Depend_On_TreasuryModules()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_TrustsModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Trusts), TrustsModules);
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_ClosingModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Closing), ClosingModules);

    }

    [Fact]
    public void Modules_Should_Not_Depend_On_CustomerModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Customers), CustomerModules);
    }


    [Fact]
    public void DbContextPersistenceLayerClasses_WhenInheritanceIsAttempted_ThenItShouldNotBePossible()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith(currentModule)
            .And()
            .Inherit(typeof(Microsoft.EntityFrameworkCore.DbContext))
            .And()
            .ImplementInterface(typeof(Treasury.Application.Abstractions.Data.IUnitOfWork))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue($"Todo DbContext debe ser sealed y implementar la interfaz IUnitWork en modulo {currentModule}");
    }


    #region Test Internals Module

    //[Fact]
    //public void Presentation_should_not_depend_on_Domain_or_Infrastructure()
    //{
    //    var result = Types.InCurrentDomain()
    //        .That()
    //        .ResideInNamespaceStartingWith($"{currentModule}.Presentation")
    //        .ShouldNot()
    //        .HaveDependencyOnAny($"{currentModule}.Domain", $"{currentModule}.Infrastructure")
    //        .GetResult();

    //    var failingTypeNames = GetTypesFalling(result);

    //    result.IsSuccessful.ShouldBeTrue($"{currentModule}.Presentation, no debe depender de sus capas de {currentModule}.Infrastructure" +
    //                                     (failingTypeNames.Any() ? $"Revisar : {string.Join(", ", failingTypeNames)}" : string.Empty));
    //}

    [Fact]
    public void Domain_should_not_depend_on_Application_nor_Infrastructure()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith($"{currentModule}.Domain")
            .ShouldNot()
            .HaveDependencyOnAny($"{currentModule}.Application", $"{currentModule}.Infrastructure")
            .GetResult();

        var failingTypeNames = GetTypesFalling(result);

        result.IsSuccessful.ShouldBeTrue($"{currentModule}.Domain, no debe depender de sus capas de {currentModule}.Infrastructure/Application" +
                                         (failingTypeNames.Any() ? $"Revisar : {string.Join(", ", failingTypeNames)}" : string.Empty));
    }


    [Fact]
    public void Application_should_not_depend_on_Infrastructure()
    {
        var result = Types.InCurrentDomain()
            .That()
            .ResideInNamespaceStartingWith($"{currentModule}.Application")
            .ShouldNot()
            .HaveDependencyOnAny($"{currentModule}.Infrastructure")
            .GetResult();

        var failingTypeNames = GetTypesFalling(result);

        result.IsSuccessful.ShouldBeTrue($"{currentModule}.Application, no debe depender de sus capas de {currentModule}.Infrastructure" +
                                         (failingTypeNames.Any() ? $"Revisar : {string.Join(", ", failingTypeNames)}" : string.Empty));

    }

    #endregion

}





