using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace V1.Infrastructure.Migrations
{
    public partial class AddNursingCareElementTypeType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:amendment_status", "in_progress,resolved")
                .Annotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived,import_note,care_package_budget_approver_assigned,care_package_approved,amendment_requested")
                .Annotation("Npgsql:Enum:element_billing_type", "supplier,customer,none,ccg")
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .Annotation("Npgsql:Enum:element_type_type", "service,provisional_care_charge,confirmed_care_charge,nursing_care")
                .Annotation("Npgsql:Enum:provider_type", "framework,spot")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:amendment_status", "in_progress,resolved")
                .OldAnnotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived,import_note,care_package_budget_approver_assigned,care_package_approved,amendment_requested")
                .OldAnnotation("Npgsql:Enum:element_billing_type", "supplier,customer,none,ccg")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .OldAnnotation("Npgsql:Enum:element_type_type", "service,provisional_care_charge,confirmed_care_charge")
                .OldAnnotation("Npgsql:Enum:provider_type", "framework,spot")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:amendment_status", "in_progress,resolved")
                .Annotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived,import_note,care_package_budget_approver_assigned,care_package_approved,amendment_requested")
                .Annotation("Npgsql:Enum:element_billing_type", "supplier,customer,none,ccg")
                .Annotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .Annotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .Annotation("Npgsql:Enum:element_type_type", "service,provisional_care_charge,confirmed_care_charge")
                .Annotation("Npgsql:Enum:provider_type", "framework,spot")
                .Annotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .Annotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .Annotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic")
                .OldAnnotation("Npgsql:Enum:amendment_status", "in_progress,resolved")
                .OldAnnotation("Npgsql:Enum:audit_event_type", "referral_broker_assignment,referral_broker_reassignment,element_ended,element_cancelled,element_suspended,care_package_ended,care_package_cancelled,care_package_suspended,referral_archived,import_note,care_package_budget_approver_assigned,care_package_approved,amendment_requested")
                .OldAnnotation("Npgsql:Enum:element_billing_type", "supplier,customer,none,ccg")
                .OldAnnotation("Npgsql:Enum:element_cost_type", "hourly,daily,weekly,transport,one_off")
                .OldAnnotation("Npgsql:Enum:element_status", "in_progress,awaiting_approval,approved,inactive,active,ended,suspended,cancelled")
                .OldAnnotation("Npgsql:Enum:element_type_type", "service,provisional_care_charge,confirmed_care_charge,nursing_care")
                .OldAnnotation("Npgsql:Enum:provider_type", "framework,spot")
                .OldAnnotation("Npgsql:Enum:referral_status", "unassigned,in_review,assigned,on_hold,archived,in_progress,awaiting_approval,approved,active,ended,cancelled")
                .OldAnnotation("Npgsql:Enum:user_role", "brokerage_assistant,broker,approver,care_charges_officer,referrer")
                .OldAnnotation("Npgsql:Enum:workflow_type", "assessment,review,reassessment,historic");
        }
    }
}
