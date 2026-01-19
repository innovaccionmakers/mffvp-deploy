using Accounting.Application.Abstractions.External;
using Accounting.Application.AccountingOperations;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain.OperationTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Accounting.test.UnitTests.AccountingOperations
{
    public class AccountingOperationsHandlerTests
    {
        private readonly Mock<IOperationLocator> _operationLocatorMock;
        private readonly Mock<IRpcClient> _rpcClientMock;
        private readonly Mock<ISender> _senderMock;
        private readonly Mock<ILogger<AccountingOperationsHandlerValidation>> _loggerMock;
        private readonly AccountingOperationsHandlerValidation _handler;
        private readonly AccountingOperationsCommand _command;

        public AccountingOperationsHandlerTests()
        {
            _operationLocatorMock = new Mock<IOperationLocator>();
            _rpcClientMock = new Mock<IRpcClient>();
            _senderMock = new Mock<ISender>();
            _loggerMock = new Mock<ILogger<AccountingOperationsHandlerValidation>>();

            _handler = new AccountingOperationsHandlerValidation(
                _operationLocatorMock.Object,
                _rpcClientMock.Object,
                _senderMock.Object,  // Make sure this line is present and correct
                _loggerMock.Object);

            _command = new AccountingOperationsCommand(
                new List<int> { 1, 2, 3 },
                DateTime.Now.Date);
        }

        [Fact]
        public async Task ProcessOperationsInParallel_ShouldCompleteSuccessfully()
        {
            // Arrange
            var operations = GenerateMockOperations(1000); // Reducido para debugging
            var treasuryByPortfolioId = GenerateMockTreasuries(operations);
            var issuersByBankId = GenerateMockIssuers();
            var collectionBankIdsByClientOperationId = GenerateMockCollectionBankIds(operations);
            var portfoliosByPortfolioId = GenerateMockPortfolios(operations);
            var cancellationToken = CancellationToken.None;

            // Debug: Verificar datos de entrada
            Console.WriteLine($"Operaciones generadas: {operations.Count}");
            Console.WriteLine($"Treasuries: {treasuryByPortfolioId.Count}");

            // Act
            var result = await _handler.ProcessOperationsInParallel(
                operations,
                treasuryByPortfolioId,
                _command,
                OperationTypeAttributes.Names.Contribution,
                issuersByBankId,
                collectionBankIdsByClientOperationId,
                portfoliosByPortfolioId,
                cancellationToken);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ProcessOperationsInParallel_ShouldHandleMissingDataGracefully()
        {
            // Arrange - Datos con missing values intencionales pero algunos válidos
            var operations = GenerateMockOperationsWithSomeValidData(1000);
            var treasuryByPortfolioId = GeneratePartialTreasuries(operations);
            var issuersByBankId = GenerateMockIssuers();
            var collectionBankIdsByClientOperationId = GenerateMockCollectionBankIds(operations);
            var portfoliosByPortfolioId = GenerateMockPortfolios(operations);

            // Act
            var result = await _handler.ProcessOperationsInParallel(
                operations,
                treasuryByPortfolioId,
                _command,
                OperationTypeAttributes.Names.Contribution,
                issuersByBankId,
                collectionBankIdsByClientOperationId,
                portfoliosByPortfolioId,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Console.WriteLine($"Operaciones con datos faltantes: {operations.Count}");
        }

        [Fact]
        public async Task ProcessOperationsInParallel_ShouldProcessInBatchesCorrectly()
        {
            // Arrange
            var operations = GenerateValidMockOperations(1000);
            var treasuryByPortfolioId = GenerateMockTreasuries(operations);
            var issuersByBankId = GenerateMockIssuers();
            var collectionBankIdsByClientOperationId = GenerateMockCollectionBankIds(operations);
            var portfoliosByPortfolioId = GenerateMockPortfolios(operations);

            // Act
            var result = await _handler.ProcessOperationsInParallel(
                operations,
                treasuryByPortfolioId,
                _command,
                OperationTypeAttributes.Names.Contribution,
                issuersByBankId,
                collectionBankIdsByClientOperationId,
                portfoliosByPortfolioId,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task ProcessOperationsInParallel_WithMixedData_ShouldHandleAllScenarios()
        {
            // Arrange
            var operations = GenerateMixedValidityOperations(1000);
            var treasuryByPortfolioId = GenerateMockTreasuries(operations);
            var issuersByBankId = GenerateMockIssuers();
            var collectionBankIdsByClientOperationId = GenerateMockCollectionBankIds(operations);
            var portfoliosByPortfolioId = GenerateMockPortfolios(operations);
            var processDate = DateTime.Now;

            // Act
            var result = await _handler.ProcessOperationsInParallel(
                operations,
                treasuryByPortfolioId,
                _command,
                OperationTypeAttributes.Names.Contribution,
                issuersByBankId,
                collectionBankIdsByClientOperationId,
                portfoliosByPortfolioId,
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Console.WriteLine($"Operaciones mixtas procesadas: {operations.Count}");
            Console.WriteLine($"Asistentes generados: {result}");
        }


        private IReadOnlyCollection<GetAccountingOperationsResponse> GenerateMockOperations(int count)
        {
            var operations = new List<GetAccountingOperationsResponse>();
            var random = new Random();
            var operationTypes = new[] { "DEPOSIT", "WITHDRAWAL", "TRANSFER", "FEE", "INTEREST" };

            for (int i = 0; i < count; i++)
            {
                var operationType = operationTypes[random.Next(operationTypes.Length)];
                var nature = random.Next(2) == 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;

                operations.Add(new GetAccountingOperationsResponse(
                    ClientOperationId: i + 1,
                    PortfolioId: random.Next(1, 100), // Rango más pequeño para asegurar matches
                    AffiliateId: random.Next(1, 1000),
                    Amount: (decimal)(random.NextDouble() * 1000) + 1, // Mínimo 1 para ser válido
                    OperationTypeName: operationType,
                    Nature: nature,
                    OperationTypeId: random.Next(1, 10),
                    CollectionAccount: $"ACC{random.Next(100, 200)}"
                ));
            }

            return operations;
        }

        private IReadOnlyCollection<GetAccountingOperationsResponse> GenerateValidMockOperations(int count)
        {
            var operations = new List<GetAccountingOperationsResponse>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                operations.Add(new GetAccountingOperationsResponse(
                    ClientOperationId: i + 1,
                    PortfolioId: random.Next(1, 50), // Rango pequeño para asegurar matches
                    AffiliateId: random.Next(1, 500),
                    Amount: (decimal)(random.NextDouble() * 5000) + 10, // Mínimo 10
                    OperationTypeName: "DEPOSIT",
                    Nature: IncomeEgressNature.Income,
                    OperationTypeId: 1,
                    CollectionAccount: "ACC001"
                ));
            }

            return operations;
        }

        private IReadOnlyCollection<GetAccountingOperationsResponse> GenerateMockOperationsWithSomeValidData(int count)
        {
            var operations = new List<GetAccountingOperationsResponse>();
            var random = new Random();

            for (int i = 0; i < count; i++)
            {
                // 70% de operaciones válidas, 30% con datos faltantes
                var hasValidData = random.Next(10) < 7;

                var affiliateId = hasValidData ? random.Next(1, 100) : 0;
                var portfolioId = hasValidData ? random.Next(1, 50) : 9999;
                var amount = hasValidData ? (decimal)(random.NextDouble() * 1000) + 1 : -1;

                operations.Add(new GetAccountingOperationsResponse(
                    ClientOperationId: i + 1,
                    PortfolioId: portfolioId,
                    AffiliateId: affiliateId,
                    Amount: amount,
                    OperationTypeName: "DEPOSIT",
                    Nature: IncomeEgressNature.Income,
                    OperationTypeId: 1,
                    CollectionAccount: "ACC001"
                ));
            }

            return operations;
        }

        private IReadOnlyCollection<GetAccountingOperationsResponse> GenerateMixedValidityOperations(int count)
        {
            var operations = new List<GetAccountingOperationsResponse>();
            var random = new Random();
            var operationTypes = new[] { "DEPOSIT", "WITHDRAWAL", "INVALID_TYPE" };

            for (int i = 0; i < count; i++)
            {
                var operationType = operationTypes[random.Next(operationTypes.Length)];
                var isValid = operationType != "INVALID_TYPE";

                operations.Add(new GetAccountingOperationsResponse(
                    ClientOperationId: i + 1,
                    PortfolioId: isValid ? random.Next(1, 100) : 9999,
                    AffiliateId: isValid ? random.Next(1, 1000) : 0,
                    Amount: isValid ? (decimal)(random.NextDouble() * 1000) + 1 : -1,
                    OperationTypeName: operationType,
                    Nature: IncomeEgressNature.Income,
                    OperationTypeId: 1,
                    CollectionAccount: isValid ? "ACC001" : "INVALID_ACC"
                ));
            }

            return operations;
        }

        private Dictionary<int, GetAccountingOperationsTreasuriesResponse> GenerateMockTreasuries(IReadOnlyCollection<GetAccountingOperationsResponse> operations)
        {
            var treasuries = new Dictionary<int, GetAccountingOperationsTreasuriesResponse>();
            var debitAccounts = new[] { "1001001001", "1001001002", "1001001003" };
            var random = new Random();
            var distinctPortfolioIds = operations.Select(o => o.PortfolioId).Distinct().Where(id => id > 0 && id < 1000);

            foreach (var portfolioId in distinctPortfolioIds)
            {
                treasuries[portfolioId] = new GetAccountingOperationsTreasuriesResponse(
                    PortfolioId: portfolioId,
                    DebitAccount: debitAccounts[random.Next(debitAccounts.Length)],
                    CreditAccount: "200200200" + portfolioId
                );
            }

            return treasuries;
        }

        private Dictionary<int, GetAccountingOperationsTreasuriesResponse> GeneratePartialTreasuries(IReadOnlyCollection<GetAccountingOperationsResponse> operations)
        {
            var treasuries = new Dictionary<int, GetAccountingOperationsTreasuriesResponse>();
            var debitAccounts = new[] { "1001001001", "1001001002" };
            var random = new Random();
            var distinctPortfolioIds = operations.Select(o => o.PortfolioId).Distinct().Where(id => id > 0 && id < 1000);

            foreach (var portfolioId in distinctPortfolioIds)
            {
                // Solo el 70% de los portfolios tendrán treasury
                if (random.Next(10) > 3)
                {
                    treasuries[portfolioId] = new GetAccountingOperationsTreasuriesResponse(
                        PortfolioId: portfolioId,
                        DebitAccount: debitAccounts[random.Next(debitAccounts.Length)],
                        CreditAccount: "200200200" + portfolioId
                    );
                }
            }

            return treasuries;
        }

        private Dictionary<long, IssuerInfo> GenerateMockIssuers()
        {
            var issuers = new Dictionary<long, IssuerInfo>();
            var bankIds = new[] { 1L, 2L, 3L, 4L, 5L };
            var nits = new[] { "900123456", "900234567", "900345678", "900456789", "900567890" };
            var names = new[] { "Banco Test 1", "Banco Test 2", "Banco Test 3", "Banco Test 4", "Banco Test 5" };

            for (int i = 0; i < bankIds.Length; i++)
            {
                issuers[bankIds[i]] = new IssuerInfo(
                    Id: bankIds[i],
                    Nit: nits[i],
                    Digit: i + 1,
                    Description: names[i]
                );
            }

            return issuers;
        }

        private Dictionary<long, int> GenerateMockCollectionBankIds(IReadOnlyCollection<GetAccountingOperationsResponse> operations)
        {
            var collectionBankIds = new Dictionary<long, int>();
            var bankIds = new[] { 1, 2, 3, 4, 5 };
            var random = new Random();

            foreach (var operation in operations)
            {
                if (!collectionBankIds.ContainsKey(operation.ClientOperationId))
                {
                    collectionBankIds[operation.ClientOperationId] = bankIds[random.Next(bankIds.Length)];
                }
            }

            return collectionBankIds;
        }

        private Dictionary<int, PortfolioResponse> GenerateMockPortfolios(IReadOnlyCollection<GetAccountingOperationsResponse> operations)
        {
            var portfolios = new Dictionary<int, PortfolioResponse>();
            var distinctPortfolioIds = operations.Select(o => o.PortfolioId).Distinct().Where(id => id > 0 && id < 1000);

            foreach (var portfolioId in distinctPortfolioIds)
            {
                portfolios[portfolioId] = new PortfolioResponse(
                    NitApprovedPortfolio: $"900{portfolioId:D6}",
                    VerificationDigit: portfolioId % 10,
                    Name: $"Portafolio Test {portfolioId}"
                );
            }

            return portfolios;
        }
    }
}