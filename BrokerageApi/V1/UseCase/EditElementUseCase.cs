using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces;

namespace BrokerageApi.V1.UseCase
{
    public class EditElementUseCase : IEditElementUseCase
    {
        private readonly IReferralGateway _referralGateway;
        private readonly IElementTypeGateway _elementTypeGateway;
        private readonly IProviderGateway _providerGateway;
        private readonly IUserService _userService;
        private readonly IClockService _mockClockObject;
        private readonly IDbSaver _dbSaver;

        public EditElementUseCase(
            IReferralGateway referralGateway,
            IElementTypeGateway elementTypeGateway,
            IProviderGateway providerGateway,
            IUserService userService,
            IClockService mockClockObject,
            IDbSaver dbSaver)
        {
            _referralGateway = referralGateway;
            _elementTypeGateway = elementTypeGateway;
            _providerGateway = providerGateway;
            _userService = userService;
            _mockClockObject = mockClockObject;
            _dbSaver = dbSaver;
        }

        public Task<Element> ExecuteAsync(int referralId, int elementId, EditElementRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
