using System.Collections.Concurrent;
using Common.SharedKernel.Application.EventBus;
using DotNetCore.CAP;

namespace Application.EventBus
{
    public class PersonRequestReplyService(
        ICapPublisher capPublisher) : ICapSubscribe
    {
        private static readonly ConcurrentDictionary<string, TaskCompletionSource<PersonDataResponseEvent>> _pendingRequests = new();
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        public async Task<PersonDataResponseEvent> RequestPersonDataAsync(
            string DocumentType,
            string Identification,
            CancellationToken cancellationToken = default)
        {
            var request = new PersonDataRequestEvent(DocumentType, Identification);
            var tcs = new TaskCompletionSource<PersonDataResponseEvent>();

            _pendingRequests[Identification] = tcs;

            await capPublisher.PublishAsync(nameof(PersonDataRequestEvent), request, cancellationToken: cancellationToken);

            using var reg = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            TimeSpan timestamp = TimeSpan.FromSeconds(30);

            timeoutCts.CancelAfter(timestamp);

            return await tcs.Task.WaitAsync(timeoutCts.Token);
        }

        [CapSubscribe(nameof(PersonDataResponseEvent))]
        public Task HandleResponse(PersonDataResponseEvent response)
        {
            if (_pendingRequests.TryGetValue(response.Identification ?? string.Empty, out var tcs))
                tcs.TrySetResult(response);
                
            return Task.CompletedTask;
        }
    }
}