using Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain;


namespace Closing.test.UnitTests.Application.PreClosing.Services.Yield.Builders
{
    public sealed class AutomaticConceptYieldDetailBuilderTests
    {
        [Fact]
        public void CanHandle_ReturnsTrue_ForAutomaticConceptSummary()
        {
            // Arrange
            var builder = new AutomaticConceptYieldDetailBuilder();

            // Act
            var result = builder.CanHandle(typeof(AutomaticConceptSummary));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanHandle_ReturnsFalse_ForOtherTypes()
        {
            // Arrange
            var builder = new AutomaticConceptYieldDetailBuilder();

            // Act
            var resultString = builder.CanHandle(typeof(string));
            var resultInt = builder.CanHandle(typeof(int));

            // Assert
            Assert.False(resultString);
            Assert.False(resultInt);
        }

        [Fact]
        public void Build_PositiveTotalAmount_SetsIncomeWithTotalAmount_AndZeroExpenses()
        {
            // Arrange
            var summary = new AutomaticConceptSummary(
                ConceptId: 10,
                ConceptName: "Ajuste Rendimiento Ingreso",
                Nature: IncomeExpenseNature.Income,
                Source: YieldsSources.AutomaticConcept,
                TotalAmount: 123.45m);

            var parameters = new RunSimulationParameters(
                PortfolioId: 7,
                ClosingDate: new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc),
                IsClosing: true);

            var builder = new AutomaticConceptYieldDetailBuilder();

            // Act
            var detail = builder.Build(summary, parameters);

            // Assert
            Assert.NotNull(detail);
            // Income y Expenses según nueva regla de negocio:
            Assert.Equal(123.45m, detail.Income);
            Assert.Equal(0m, detail.Expenses);
        }

        [Fact]
        public void Build_NegativeTotalAmount_SetsIncomeWithNegativeTotalAmount_AndZeroExpenses()
        {
            // Arrange
            var summary = new AutomaticConceptSummary(
                ConceptId: 20,
                ConceptName: "Ajuste Rendimiento Gasto",
                Nature: IncomeExpenseNature.Expense,
                Source: YieldsSources.AutomaticConcept,
                TotalAmount: -50m);    // gasto → negativo

            var parameters = new RunSimulationParameters(
                PortfolioId: 7,
                ClosingDate: new DateTime(2025, 10, 7, 0, 0, 0, DateTimeKind.Utc),
                IsClosing: true);

            var builder = new AutomaticConceptYieldDetailBuilder();

            // Act
            var detail = builder.Build(summary, parameters);

            // Assert
            Assert.NotNull(detail);
            // El signo se conserva en Income, Expenses siempre 0:
            Assert.Equal(-50m, detail.Income);
            Assert.Equal(0m, detail.Expenses);
        }
    }
}
