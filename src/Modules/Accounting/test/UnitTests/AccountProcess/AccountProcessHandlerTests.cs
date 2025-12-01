using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Moq;
using System.Reflection;

namespace Accounting.test.UnitTests.AccountProcess
{
    public class AccountProcessHandlerTests
    {
        private readonly Mock<ISender> _senderMock;
        private readonly Mock<IClosingExecutionStore> _closingValidatorMock;
        private readonly Mock<IAccountingNotificationService> _accountingNotificationServiceMock;
        private readonly Mock<IUserLocator> _userLocatorMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly Mock<IActiveProcess> _activeProcessStoreMock;
        private readonly Mock<IPortfolioLocator> _portfolioLocatorMock;
        private readonly object _handler;
        private readonly MethodInfo _handleMethod;

        public AccountProcessHandlerTests()
        {
            _senderMock = new Mock<ISender>();
            _closingValidatorMock = new Mock<IClosingExecutionStore>();
            _accountingNotificationServiceMock = new Mock<IAccountingNotificationService>();
            _userLocatorMock = new Mock<IUserLocator>();
            _userServiceMock = new Mock<IUserService>();
            _eventBusMock = new Mock<IEventBus>();
            _activeProcessStoreMock = new Mock<IActiveProcess>();
            _portfolioLocatorMock = new Mock<IPortfolioLocator>();

            // Crear instancia usando reflection
            var assembly = Assembly.Load("Accounting.Application");
            var handlerType = assembly.GetType("Accounting.Application.AccountProcess.AccountProcessHandler");

            if (handlerType == null)
            {
                throw new TypeLoadException("No se pudo encontrar el tipo AccountProcessHandler");
            }

            // Pasar todas las 8 dependencias al constructor EN EL ORDEN CORRECTO
            _handler = Activator.CreateInstance(
                handlerType,
                _senderMock.Object,
                _closingValidatorMock.Object,
                _accountingNotificationServiceMock.Object,
                _userLocatorMock.Object,
                _userServiceMock.Object,
                _eventBusMock.Object,
                _activeProcessStoreMock.Object,
                _portfolioLocatorMock.Object);

            // Obtener el método Handle
            _handleMethod = handlerType.GetMethod("Handle");
        }

        private async Task<Result<AccountProcessResult>> InvokeHandleAsync(AccountProcessCommand command, CancellationToken cancellationToken)
        {
            return await (Task<Result<AccountProcessResult>>)_handleMethod.Invoke(
                _handler,
                new object[] { command, cancellationToken });
        }

        [Fact]
        public async Task Handle_WhenClosingActive_ReturnsFailure()
        {
            // Arrange
            var command = new AccountProcessCommand(
                PortfolioIds: new List<int> { 1, 2, 3 },
                ProcessDate: new DateOnly(2024, 1, 1));

            var cancellationToken = CancellationToken.None;

            _closingValidatorMock
                .Setup(x => x.IsClosingActiveAsync(cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await InvokeHandleAsync(command, cancellationToken);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("0001", result.Error.Code);
            Assert.Equal("Existe un proceso de cierre activo.", result.Error.Description);
        }

        [Fact]
        public async Task Handle_WhenPortfoliosNotClosed_ReturnsFailure()
        {
            // Arrange
            var command = new AccountProcessCommand(
                PortfolioIds: new List<int> { 1, 2, 3 },
                ProcessDate: new DateOnly(2024, 1, 1));

            var cancellationToken = CancellationToken.None;
            var processDate = command.ProcessDate.ToDateTime(TimeOnly.MinValue);

            _closingValidatorMock
                .Setup(x => x.IsClosingActiveAsync(cancellationToken))
                .ReturnsAsync(false);

            _portfolioLocatorMock
                .Setup(x => x.AreAllPortfoliosClosedAsync(command.PortfolioIds, processDate, cancellationToken))
                .ReturnsAsync(Result.Failure<bool>(new Error("0002", "No todos los portafolios están cerrados", ErrorType.Validation)));

            // Act
            var result = await InvokeHandleAsync(command, cancellationToken);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("0002", result.Error.Code);
        }

        [Fact]
        public async Task Handle_WhenAccountingProcessActive_ReturnsFailure()
        {
            // Arrange
            var command = new AccountProcessCommand(
                PortfolioIds: new List<int> { 1, 2, 3 },
                ProcessDate: new DateOnly(2024, 1, 1));

            var cancellationToken = CancellationToken.None;
            var processDate = command.ProcessDate.ToDateTime(TimeOnly.MinValue);

            _closingValidatorMock
                .Setup(x => x.IsClosingActiveAsync(cancellationToken))
                .ReturnsAsync(false);

            _portfolioLocatorMock
                .Setup(x => x.AreAllPortfoliosClosedAsync(command.PortfolioIds, processDate, cancellationToken))
                .ReturnsAsync(Result.Success(true));

            _activeProcessStoreMock
                .Setup(x => x.GetProcessActiveAsync(cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await InvokeHandleAsync(command, cancellationToken);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("0003", result.Error.Code);
            Assert.Equal("Ya existe un generación contable en ejecución.", result.Error.Description);
        }
    }
}