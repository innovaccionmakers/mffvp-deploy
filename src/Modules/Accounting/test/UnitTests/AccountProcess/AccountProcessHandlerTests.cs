using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Domain;
using MediatR;
using Moq;
using System.Reflection;

namespace Accounting.test.UnitTests.AccountProcess
{
    public class AccountProcessHandlerReflectionTests
    {
        private readonly Mock<ISender> _senderMock;
        private readonly Mock<IClosingExecutionStore> _closingValidatorMock;
        private readonly Mock<IAccountingNotificationService> _accountingNotificationServiceMock;
        private readonly Mock<IUserLocator> _userLocatorMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IEventBus> _eventBusMock;
        private readonly object _handler;
        private readonly MethodInfo _handleMethod;

        public AccountProcessHandlerReflectionTests()
        {
            _senderMock = new Mock<ISender>();
            _closingValidatorMock = new Mock<IClosingExecutionStore>();
            _accountingNotificationServiceMock = new Mock<IAccountingNotificationService>();
            _userLocatorMock = new Mock<IUserLocator>();
            _userServiceMock = new Mock<IUserService>();
            _eventBusMock = new Mock<IEventBus>();

            // Crear instancia usando reflection
            var handlerType = Assembly.Load("Accounting.Application")
                .GetType("Accounting.Application.AccountProcess.AccountProcessHandler");

            _handler = Activator.CreateInstance(handlerType,
                _senderMock.Object,
                _closingValidatorMock.Object,
                _accountingNotificationServiceMock.Object,
                _userLocatorMock.Object,
                _userServiceMock.Object,
                _eventBusMock.Object);

            _handleMethod = handlerType.GetMethod("Handle");
        }

        [Fact]
        public async Task Handle_WhenClosingActive_ReturnsFailure()
        {
            // Arrange
            var command = new AccountProcessCommand(
                new List<int> { 1, 2, 3 },
                new DateOnly(2024, 1, 1));
            var cancellationToken = CancellationToken.None;

            _closingValidatorMock
                .Setup(x => x.IsClosingActiveAsync(cancellationToken))
                .ReturnsAsync(true);

            // Act
            var result = await (Task<Result<AccountProcessResult>>)_handleMethod
                .Invoke(_handler, new object[] { command, cancellationToken });

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("0001", result.Error.Code);
        }
    }
}