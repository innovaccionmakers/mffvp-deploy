using System.Data;
using System.Data.Common;
using Contributions.Application.Abstractions.Data;
using Contributions.Application.Abstractions.Lookups;
using Contributions.Application.Abstractions.Rules;
using Contributions.Domain.Clients;
using Contributions.Domain.ClientOperations;
using Contributions.Domain.Portfolios;
using Contributions.Domain.TrustOperations;
using Contributions.Domain.Trusts;
using Contributions.Integrations.FullContribution;
using FluentAssertions;
using Moq;
using RulesEngine.Models;
using Contributions.Application.FullContribution;

namespace Contributions.Tests.UnitTests
{
    public sealed class CreateFullContributionHandlerTests
    {
        private readonly Mock<IRuleEvaluator> _rules = new();
        private readonly Mock<ILookupService> _lookups = new();
        private readonly Mock<IClientRepository> _clientRepo = new();
        private readonly Mock<IPortfolioRepository> _portRepo = new();
        private readonly Mock<IClientOperationRepository> _coRepo = new();
        private readonly Mock<ITrustRepository> _trustRepo = new();
        private readonly Mock<ITrustOperationRepository> _toRepo = new();
        private readonly Mock<IUnitOfWork> _uow = new();

        private readonly CreateFullContributionCommand _validCmd =
            new(
                IdentificationType: "CC",
                Identification: "8027845",
                ObjectiveId: 1458,
                PortfolioCode: "P01",
                Amount: 10_000_000m,
                OriginCode: "A",
                CollectionMethodCode: "AO",
                PaymentMethodCode: "TB",
                CertifiedContribution: "SI",
                ContingentWithholding: 0,
                ExecutionDate: DateTime.UtcNow.Date
            );

        public CreateFullContributionHandlerTests()
        {
            // Default rules → OK
            _rules.Setup(r => r.EvaluateAsync(
                           It.IsAny<string>(),
                           It.IsAny<object>(),
                           It.IsAny<CancellationToken>()))
                  .ReturnsAsync((true, Array.Empty<RuleResultTree>(), Array.Empty<RuleValidationError>()));

            // Default lookups → all active
            _lookups.Setup(l => l.CodeExists(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);
            _lookups.Setup(l => l.CodeIsActive(It.IsAny<string>(), It.IsAny<string>()))
                    .Returns(true);

            // Happy-path client
            _clientRepo.Setup(c => c.Get("CC", "8027845"))
                       .Returns(new Client("CC", "8027845", true, false, true));

            // Happy-path portfolio
            _portRepo.Setup(p => p.GetByCode("P01"))
                     .Returns(new Portfolio("P01", "Portafolio Liquidez", true, 1458, OperationDate: DateTime.UtcNow.Date));
            _portRepo.Setup(p => p.BelongsToObjective("P01", 1458))
                     .Returns(true);

            // UnitOfWork mocks
            _uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DummyDbTransaction());
            _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);  // SaveChangesAsync returns Task<int>
        }

        [Fact]
        public async Task Handle_Should_ReturnSuccess_When_InputIsValid()
        {
            var sut = BuildSut();

            var result = await sut.Handle(_validCmd, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.PortfolioCode.Should().Be("P01");
            _coRepo.Verify(r => r.Insert(It.IsAny<ClientOperation>()), Times.Once);
            _trustRepo.Verify(r => r.Insert(It.IsAny<Trust>()), Times.Once);
            _toRepo.Verify(r => r.Insert(It.IsAny<TrustOperation>()), Times.Once);
            _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_ReturnError6000_When_ClientDoesNotExist()
        {
            _clientRepo.Setup(c => c.Get("CC", "8027845")).Returns((Client?)null);
            var sut = BuildSut();

            var result = await sut.Handle(_validCmd, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Code.Should().Be("6000");
        }

        [Fact]
        public async Task Handle_Should_ReturnError6267_When_AmountIsZero()
        {
            var cmd = _validCmd with { Amount = 0m };
            _rules.Setup(r => r.EvaluateAsync(
                           It.IsAny<string>(),
                           It.IsAny<object>(),
                           It.IsAny<CancellationToken>()))
                  .ReturnsAsync((false,
                                 Array.Empty<RuleResultTree>(),
                                 new[] { new RuleValidationError("6267", "Amount must be greater than 0") }
                  ));

            var sut = BuildSut();

            var result = await sut.Handle(cmd, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Description.Should().Contain("6267");
        }

        private CreateFullContributionCommandHandler BuildSut()
            => new(
                _rules.Object,
                _lookups.Object,
                _clientRepo.Object,
                _portRepo.Object,
                _coRepo.Object,
                _trustRepo.Object,
                _toRepo.Object,
                _uow.Object
            );

        private sealed class DummyDbTransaction : DbTransaction
        {
            protected override DbConnection? DbConnection => null;
            public override IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
            public override void Commit() { }
            public override void Rollback() { }
        }
    }
}
