using Activations.Domain.Clients;
using Activations.Integrations.Affiliates.CreateActivation;

namespace Activations.Application.Affiliates.CreateActivation
{
    public sealed class ActivationValidationContext
    {
        public CreateActivationCommand Request { get; }
        public Client? Client { get; }
        public bool IdType { get; }
        public bool Pensioner { get; }
        public bool Requirements { get; }
        public bool Dates { get; }
        public bool ExistingActivation { get; }

        public ActivationValidationContext(
            CreateActivationCommand request,
            Client? client,
            bool idType,
            bool pensioner,
            bool requirements,
            bool dates,
            bool existingActivation)
        {
            Request = request;
            Client = client;
            IdType = idType;
            Pensioner = pensioner;
            Requirements = requirements;
            Dates = dates;
            ExistingActivation = existingActivation;
        }
    }
}
