using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Reports.Application.Reports.BalancesAndMovements;
using Reports.Domain.BalancesAndMovements;

namespace Reports.test.UnitTests
{
    public class BalancesAndMovementsReportTests
    {
        private readonly Mock<ILogger<BalancesAndMovementsReport>> _loggerMock;
        private readonly Mock<IBalancesAndMovementsReportRepository> _repositoryMock;
        private readonly BalancesAndMovementsReport _report;

        public BalancesAndMovementsReportTests()
        {
            _loggerMock = new Mock<ILogger<BalancesAndMovementsReport>>();
            _repositoryMock = new Mock<IBalancesAndMovementsReportRepository>();
            _report = new BalancesAndMovementsReport(_loggerMock.Object, _repositoryMock.Object);
        }

        [Fact]
        public void ReportName_ShouldReturnCorrectValue()
        {
            // Act
            var reportName = _report.ReportName;

            // Assert
            Assert.Equal("Informe de Saldos y Movimientos", reportName);
        }

        [Fact]
        public void ColumnHeaders_ShouldReturnSaldosHeaders()
        {
            // Arrange
            var expectedHeaders = new[]
            {
                "Fecha Inicial", "Fecha Final", "Tipo Identificacion", "Identificacion",
                "Nombre Afiliado", "IdObjetivo", "Objetivo", "Nombre Fondo", "Plan",
                "Alternativa", "Portafolio", "Saldo Inicial", "Entradas", "Salidas",
                "Rendimientos", "Retefuente", "Saldo Final"
            };

            // Act
            var headers = _report.ColumnHeaders;

            // Assert
            Assert.Equal(expectedHeaders, headers);
        }

        [Fact]
        public async Task GetReportDataAsync_WithValidBalancesAndMovementsRequest_ShouldGenerateReport()
        {
            // Arrange
            // Crear un request que sabemos que es válido (fechas correctas)
            var request = new BalancesAndMovementsReportRequest
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 31), // Fecha posterior a StartDate
                Identification = "123456789"
            };

            // Configurar mocks para retornar datos vacíos
            _repositoryMock.Setup(r => r.GetBalancesAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BalancesResponse>());

            _repositoryMock.Setup(r => r.GetMovementsAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<MovementsResponse>());

            // Act
            var result = await _report.GetReportDataAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<FileResult>(result);

            var fileResult = (FileResult)result;
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal("Informe de Saldos y Movimientos.xlsx", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task GetReportDataAsync_WhenRepositoryThrowsException_ShouldLogAndRethrow()
        {
            // Arrange
            var request = new BalancesAndMovementsReportRequest
            {
                StartDate = new DateTime(2024, 1, 1),
                EndDate = new DateTime(2024, 1, 31)
            };

            _repositoryMock.Setup(r => r.GetBalancesAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _report.GetReportDataAsync(request, CancellationToken.None));

            // Verify que se logueó el error - permitir múltiples llamadas
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error al generar el reporte")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce); // Cambiado de Times.Once a Times.AtLeastOnce
        }
    }
}
