using NetArchTest.Rules;
using Shouldly;
using System.Net.Mime;
using System.Reflection;


namespace MFFVP.ArchitectureTests;

[Collection("OperationsTest")]
public class OperationsTest : BaseTest, IModuleTest
{
 
    public OperationsTest() : base()
    {
        currentModule = nameof(Monolith.Operations);

        foreach (var module in OperationsModules)
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


    public void Modules_Should_Not_Depend_On_OperationsModules()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_SecurityModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Security), SecurityModules);
    }

    [Fact]
    public void Modules_Should_Not_Depend_On_TreasuryModules()
    {
        CheckDependencyModule(currentModule, nameof(Monolith.Treasury), TreasuryModules);
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
            .ImplementInterface(typeof(Operations.Application.Abstractions.Data.IUnitOfWork))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue($"Todo DbContext debe ser sealed y implementar la interfaz IUnitWork en modulo {currentModule}");
    }


    #region Test Internals Module

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
                                         (failingTypeNames.Any() ? $" Revisar : {string.Join(", ", failingTypeNames)}" : string.Empty));

    }

    #endregion

}