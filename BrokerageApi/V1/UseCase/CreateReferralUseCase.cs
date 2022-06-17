using System;
using System.Linq;
using System.Threading.Tasks;
using BrokerageApi.V1.Boundary.Request;
using BrokerageApi.V1.Factories;
using BrokerageApi.V1.Gateways.Interfaces;
using BrokerageApi.V1.Infrastructure;
using BrokerageApi.V1.UseCase.Interfaces;
using Npgsql;

namespace BrokerageApi.V1.UseCase
{
    public class CreateReferralUseCase : ICreateReferralUseCase
    {
        private readonly IReferralGateway _referralGateway;

        public CreateReferralUseCase(IReferralGateway referralGateway)
        {
            _referralGateway = referralGateway;
        }

        public async Task<Referral> ExecuteAsync(CreateReferralRequest request)
        {
            var referral = request.ToDatabase();

            var existingReferrals = await _referralGateway.GetBySocialCareIdWithElementsAsync(request.SocialCareId);

            if (existingReferrals != null && existingReferrals.Any(r => r.Status == ReferralStatus.InProgress))
            {
                throw new InvalidOperationException("Existing in progress referral exists, please archive before raising new referral");
            }

            referral.Elements = existingReferrals?.SingleOrDefault(r => r.Status == ReferralStatus.Approved)?.Elements.Where(e => e.InternalStatus == ElementStatus.Approved).ToList();

            try
            {
                return await _referralGateway.CreateAsync(referral);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                PostgresException innerEx = ex.InnerException as PostgresException;

                if (innerEx?.SqlState == PostgresErrorCodes.UniqueViolation && innerEx.ConstraintName == "ix_referrals_workflow_id")
                {
                    throw new InvalidOperationException($"A referral for workflow {request.WorkflowId} already exists");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
