using System;
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
