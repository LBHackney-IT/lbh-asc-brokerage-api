using BrokerageApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace V1.Infrastructure.Migrations
{
    public partial class AddTypeToElementType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived")
                .Annotation("Npgsql:Enum:element_billing_type", "supplier,customer,none")
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .Annotation("Npgsql:Enum:element_type_type", "service,provisional_care_charge,confirmed_care_charge")
                .Annotation("Npgsql:Enum:provider_type", "framework,spot")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived")
                .OldAnnotation("Npgsql:Enum:element_billing_type", "supplier,customer,none")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .OldAnnotation("Npgsql:Enum:provider_type", "framework,spot")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");

            migrationBuilder.AddColumn<ElementTypeType>(
                name: "type",
                table: "element_types",
                type: "element_type_type",
                nullable: false,
                defaultValue: ElementTypeType.Service);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "type",
                table: "element_types");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived")
                .Annotation("Npgsql:Enum:element_billing_type", "supplier,customer,none")
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .Annotation("Npgsql:Enum:provider_type", "framework,spot")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived")
                .OldAnnotation("Npgsql:Enum:element_billing_type", "supplier,customer,none")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .OldAnnotation("Npgsql:Enum:element_type_type", "service,provisional_care_charge,confirmed_care_charge")
                .OldAnnotation("Npgsql:Enum:provider_type", "framework,spot")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");
        }
    }
}
