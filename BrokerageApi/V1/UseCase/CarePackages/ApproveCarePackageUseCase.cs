using System.Threading.Tasks;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.Services.Interfaces;
using BrokerageApi.V1.UseCase.Interfaces.CarePackages;

namespace BrokerageApi.V1.UseCase.CarePackages
{
    public class ApproveCarePackageUseCase : IApproveCarePackageUseCase
    {
        private readonly ICarePackageGateway _carePackageGateway;
        private readonly IReferralGateway _referralGateway;
        private readonly IUserService _userService;
        private readonly IUserGateway _userGateway;
        private readonly IDbSaver _dbSaver;
        private readonly IAuditGateway _auditGateway;

        public ApproveCarePackageUseCase(ICarePackageGateway carePackageGateway,
            IReferralGateway referralGateway,
            IUserService userService,
            IUserGateway userGateway,
            IDbSaver dbSaver,
            IAuditGateway auditGateway)
        {
            _carePackageGateway = carePackageGateway;
            _referralGateway = referralGateway;
            _userService = userService;
            _userGateway = userGateway;
            _dbSaver = dbSaver;
            _auditGateway = auditGateway;
        }

        public Task ExecuteAsync(int referralId)
        {
            throw new System.NotImplementedException();
        }
    }
}
