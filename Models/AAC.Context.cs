﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UAAAS.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class uaaasDBContext : DbContext
    {
        public uaaasDBContext()
            : base("name=uaaasDBContext")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<college_circulars> college_circulars { get; set; }
        public DbSet<college_clusters> college_clusters { get; set; }
        public DbSet<college_grade> college_grade { get; set; }
        public DbSet<customer> customers { get; set; }
        public DbSet<jntuh_academic_calendermaster> jntuh_academic_calendermaster { get; set; }
        public DbSet<jntuh_academic_performance> jntuh_academic_performance { get; set; }
        public DbSet<jntuh_academic_year> jntuh_academic_year { get; set; }
        public DbSet<jntuh_address> jntuh_address { get; set; }
        public DbSet<jntuh_address_log> jntuh_address_log { get; set; }
        public DbSet<jntuh_affiliation_requests> jntuh_affiliation_requests { get; set; }
        public DbSet<jntuh_affiliation_type> jntuh_affiliation_type { get; set; }
        public DbSet<jntuh_afrc_fee> jntuh_afrc_fee { get; set; }
        public DbSet<jntuh_appeal_auditor_assigned> jntuh_appeal_auditor_assigned { get; set; }
        public DbSet<jntuh_appeal_auditors_dataentry> jntuh_appeal_auditors_dataentry { get; set; }
        public DbSet<jntuh_appeal_college_edit_status> jntuh_appeal_college_edit_status { get; set; }
        public DbSet<jntuh_appeal_college_intake_existing> jntuh_appeal_college_intake_existing { get; set; }
        public DbSet<jntuh_appeal_college_intake_existing_supportingdocuments> jntuh_appeal_college_intake_existing_supportingdocuments { get; set; }
        public DbSet<jntuh_appeal_college_laboratories> jntuh_appeal_college_laboratories { get; set; }
        public DbSet<jntuh_appeal_faculty_registered> jntuh_appeal_faculty_registered { get; set; }
        public DbSet<jntuh_appeal_mbadeficiency> jntuh_appeal_mbadeficiency { get; set; }
        public DbSet<jntuh_appeal_pharmacydata> jntuh_appeal_pharmacydata { get; set; }
        public DbSet<jntuh_appeal_principal_registered> jntuh_appeal_principal_registered { get; set; }
        public DbSet<jntuh_appeal_scmproceedingrequest_addfaculty> jntuh_appeal_scmproceedingrequest_addfaculty { get; set; }
        public DbSet<jntuh_appeal_scmproceedingsrequests> jntuh_appeal_scmproceedingsrequests { get; set; }
        public DbSet<jntuh_approach_road> jntuh_approach_road { get; set; }
        public DbSet<jntuh_approvedadmitted_intake> jntuh_approvedadmitted_intake { get; set; }
        public DbSet<jntuh_area_requirement> jntuh_area_requirement { get; set; }
        public DbSet<jntuh_attendence_registrationnumberstracking> jntuh_attendence_registrationnumberstracking { get; set; }
        public DbSet<jntuh_auditor_assigned> jntuh_auditor_assigned { get; set; }
        public DbSet<jntuh_auditors_dataentry> jntuh_auditors_dataentry { get; set; }
        public DbSet<jntuh_bpharmacy_faculty_deficiency> jntuh_bpharmacy_faculty_deficiency { get; set; }
        public DbSet<jntuh_chairperson_designation> jntuh_chairperson_designation { get; set; }
        public DbSet<jntuh_changeuserid> jntuh_changeuserid { get; set; }
        public DbSet<jntuh_college_416randomcodes> jntuh_college_416randomcodes { get; set; }
        public DbSet<jntuh_college_academic_calender> jntuh_college_academic_calender { get; set; }
        public DbSet<jntuh_college_academic_data> jntuh_college_academic_data { get; set; }
        public DbSet<jntuh_college_academic_performance> jntuh_college_academic_performance { get; set; }
        public DbSet<jntuh_college_academic_performance_points> jntuh_college_academic_performance_points { get; set; }
        public DbSet<jntuh_college_accounts_payment_type> jntuh_college_accounts_payment_type { get; set; }
        public DbSet<jntuh_college_affiliation> jntuh_college_affiliation { get; set; }
        public DbSet<jntuh_college_affiliationfee_dues> jntuh_college_affiliationfee_dues { get; set; }
        public DbSet<jntuh_college_affliationstatus> jntuh_college_affliationstatus { get; set; }
        public DbSet<jntuh_college_aictefaculty> jntuh_college_aictefaculty { get; set; }
        public DbSet<jntuh_college_antiragging_committee> jntuh_college_antiragging_committee { get; set; }
        public DbSet<jntuh_college_antiragging_complaints> jntuh_college_antiragging_complaints { get; set; }
        public DbSet<jntuh_college_bas_complaints> jntuh_college_bas_complaints { get; set; }
        public DbSet<jntuh_college_basreport> jntuh_college_basreport { get; set; }
        public DbSet<jntuh_college_batch_performance> jntuh_college_batch_performance { get; set; }
        public DbSet<jntuh_college_closure> jntuh_college_closure { get; set; }
        public DbSet<jntuh_college_committee_observations> jntuh_college_committee_observations { get; set; }
        public DbSet<jntuh_college_complaints> jntuh_college_complaints { get; set; }
        public DbSet<jntuh_college_computer_lab> jntuh_college_computer_lab { get; set; }
        public DbSet<jntuh_college_computer_lab_printers> jntuh_college_computer_lab_printers { get; set; }
        public DbSet<jntuh_college_computer_student_ratio> jntuh_college_computer_student_ratio { get; set; }
        public DbSet<jntuh_college_dd_payments> jntuh_college_dd_payments { get; set; }
        public DbSet<jntuh_college_degree> jntuh_college_degree { get; set; }
        public DbSet<jntuh_college_desirable_others> jntuh_college_desirable_others { get; set; }
        public DbSet<jntuh_college_desirable_requirement> jntuh_college_desirable_requirement { get; set; }
        public DbSet<jntuh_college_document> jntuh_college_document { get; set; }
        public DbSet<jntuh_college_edit_remarks> jntuh_college_edit_remarks { get; set; }
        public DbSet<jntuh_college_edit_status> jntuh_college_edit_status { get; set; }
        public DbSet<jntuh_college_edit_status_log> jntuh_college_edit_status_log { get; set; }
        public DbSet<jntuh_college_enclosures> jntuh_college_enclosures { get; set; }
        public DbSet<jntuh_college_enclosures_hardcopy> jntuh_college_enclosures_hardcopy { get; set; }
        public DbSet<jntuh_college_essential_requirements> jntuh_college_essential_requirements { get; set; }
        public DbSet<jntuh_college_events> jntuh_college_events { get; set; }
        public DbSet<jntuh_college_examination_branch> jntuh_college_examination_branch { get; set; }
        public DbSet<jntuh_college_examination_branch_edep> jntuh_college_examination_branch_edep { get; set; }
        public DbSet<jntuh_college_examination_branch_security> jntuh_college_examination_branch_security { get; set; }
        public DbSet<jntuh_college_examination_branch_staff> jntuh_college_examination_branch_staff { get; set; }
        public DbSet<jntuh_college_expenditure> jntuh_college_expenditure { get; set; }
        public DbSet<jntuh_college_expenditure_type> jntuh_college_expenditure_type { get; set; }
        public DbSet<jntuh_college_experiments> jntuh_college_experiments { get; set; }
        public DbSet<jntuh_college_faculty_deficiency> jntuh_college_faculty_deficiency { get; set; }
        public DbSet<jntuh_college_faculty_registered> jntuh_college_faculty_registered { get; set; }
        public DbSet<jntuh_college_faculty_registered_test> jntuh_college_faculty_registered_test { get; set; }
        public DbSet<jntuh_college_faculty_replaceregistered> jntuh_college_faculty_replaceregistered { get; set; }
        public DbSet<jntuh_college_faculty_student_ratio> jntuh_college_faculty_student_ratio { get; set; }
        public DbSet<jntuh_college_facultyverification_tracking> jntuh_college_facultyverification_tracking { get; set; }
        public DbSet<jntuh_college_governingmeeting> jntuh_college_governingmeeting { get; set; }
        public DbSet<jntuh_college_grievance_complaints> jntuh_college_grievance_complaints { get; set; }
        public DbSet<jntuh_college_grievancesassigned> jntuh_college_grievancesassigned { get; set; }
        public DbSet<jntuh_college_hostel_maintenance> jntuh_college_hostel_maintenance { get; set; }
        public DbSet<jntuh_college_income> jntuh_college_income { get; set; }
        public DbSet<jntuh_college_income_type> jntuh_college_income_type { get; set; }
        public DbSet<jntuh_college_infrastructure_parameters> jntuh_college_infrastructure_parameters { get; set; }
        public DbSet<jntuh_college_intake_existing> jntuh_college_intake_existing { get; set; }
        public DbSet<jntuh_college_intake_existing_log> jntuh_college_intake_existing_log { get; set; }
        public DbSet<jntuh_college_intake_proposed> jntuh_college_intake_proposed { get; set; }
        public DbSet<jntuh_college_internet_bandwidth> jntuh_college_internet_bandwidth { get; set; }
        public DbSet<jntuh_college_lab> jntuh_college_lab { get; set; }
        public DbSet<jntuh_college_laboratories> jntuh_college_laboratories { get; set; }
        public DbSet<jntuh_college_laboratories_dataentry2> jntuh_college_laboratories_dataentry2 { get; set; }
        public DbSet<jntuh_college_laboratories_deficiency> jntuh_college_laboratories_deficiency { get; set; }
        public DbSet<jntuh_college_land_registration> jntuh_college_land_registration { get; set; }
        public DbSet<jntuh_college_legal_software> jntuh_college_legal_software { get; set; }
        public DbSet<jntuh_college_library> jntuh_college_library { get; set; }
        public DbSet<jntuh_college_library_details> jntuh_college_library_details { get; set; }
        public DbSet<jntuh_college_links_assigned> jntuh_college_links_assigned { get; set; }
        public DbSet<jntuh_college_macaddress> jntuh_college_macaddress { get; set; }
        public DbSet<jntuh_college_minoritystatus> jntuh_college_minoritystatus { get; set; }
        public DbSet<jntuh_college_monthlybasreports> jntuh_college_monthlybasreports { get; set; }
        public DbSet<jntuh_college_news> jntuh_college_news { get; set; }
        public DbSet<jntuh_college_noc_data> jntuh_college_noc_data { get; set; }
        public DbSet<jntuh_college_noc_enclosures> jntuh_college_noc_enclosures { get; set; }
        public DbSet<jntuh_college_other_university_courses> jntuh_college_other_university_courses { get; set; }
        public DbSet<jntuh_college_payment> jntuh_college_payment { get; set; }
        public DbSet<jntuh_college_paymentoffee_type> jntuh_college_paymentoffee_type { get; set; }
        public DbSet<jntuh_college_payments_banks> jntuh_college_payments_banks { get; set; }
        public DbSet<jntuh_college_payments_subpurpose> jntuh_college_payments_subpurpose { get; set; }
        public DbSet<jntuh_college_pgcourse_faculty> jntuh_college_pgcourse_faculty { get; set; }
        public DbSet<jntuh_college_pgcourses> jntuh_college_pgcourses { get; set; }
        public DbSet<jntuh_college_previous_academic_faculty> jntuh_college_previous_academic_faculty { get; set; }
        public DbSet<jntuh_college_principal_director> jntuh_college_principal_director { get; set; }
        public DbSet<jntuh_college_principal_registered> jntuh_college_principal_registered { get; set; }
        public DbSet<jntuh_college_randamcodes> jntuh_college_randamcodes { get; set; }
        public DbSet<jntuh_college_rti_complaints> jntuh_college_rti_complaints { get; set; }
        public DbSet<jntuh_college_rti_details> jntuh_college_rti_details { get; set; }
        public DbSet<jntuh_college_screens> jntuh_college_screens { get; set; }
        public DbSet<jntuh_college_screens_assigned> jntuh_college_screens_assigned { get; set; }
        public DbSet<jntuh_college_screens_assigned_log> jntuh_college_screens_assigned_log { get; set; }
        public DbSet<jntuh_college_sports> jntuh_college_sports { get; set; }
        public DbSet<jntuh_college_staticdata_modifications> jntuh_college_staticdata_modifications { get; set; }
        public DbSet<jntuh_college_status> jntuh_college_status { get; set; }
        public DbSet<jntuh_college_studentregistration> jntuh_college_studentregistration { get; set; }
        public DbSet<jntuh_college_students_complaints> jntuh_college_students_complaints { get; set; }
        public DbSet<jntuh_college_type> jntuh_college_type { get; set; }
        public DbSet<jntuh_college_users> jntuh_college_users { get; set; }
        public DbSet<jntuh_college_women_protection_cell> jntuh_college_women_protection_cell { get; set; }
        public DbSet<jntuh_college_women_protection_cell_complaints> jntuh_college_women_protection_cell_complaints { get; set; }
        public DbSet<jntuh_collegefaculty_leaves> jntuh_collegefaculty_leaves { get; set; }
        public DbSet<jntuh_collegestatus> jntuh_collegestatus { get; set; }
        public DbSet<jntuh_committee_observations> jntuh_committee_observations { get; set; }
        public DbSet<jntuh_complaints> jntuh_complaints { get; set; }
        public DbSet<jntuh_complaints_givenby> jntuh_complaints_givenby { get; set; }
        public DbSet<jntuh_complaints_status> jntuh_complaints_status { get; set; }
        public DbSet<jntuh_computer_student_ratio_norms> jntuh_computer_student_ratio_norms { get; set; }
        public DbSet<jntuh_course_affiliation_status> jntuh_course_affiliation_status { get; set; }
        public DbSet<jntuh_dataentry_allotment> jntuh_dataentry_allotment { get; set; }
        public DbSet<jntuh_dates> jntuh_dates { get; set; }
        public DbSet<jntuh_degree> jntuh_degree { get; set; }
        public DbSet<jntuh_degree_type> jntuh_degree_type { get; set; }
        public DbSet<jntuh_department> jntuh_department { get; set; }
        public DbSet<jntuh_designation> jntuh_designation { get; set; }
        public DbSet<jntuh_desirable_requirement_type> jntuh_desirable_requirement_type { get; set; }
        public DbSet<jntuh_district> jntuh_district { get; set; }
        public DbSet<jntuh_documents_required> jntuh_documents_required { get; set; }
        public DbSet<jntuh_download> jntuh_download { get; set; }
        public DbSet<jntuh_edep_equipment> jntuh_edep_equipment { get; set; }
        public DbSet<jntuh_education_category> jntuh_education_category { get; set; }
        public DbSet<jntuh_enclosures> jntuh_enclosures { get; set; }
        public DbSet<jntuh_error_log> jntuh_error_log { get; set; }
        public DbSet<jntuh_essential_requirements> jntuh_essential_requirements { get; set; }
        public DbSet<jntuh_eventmaster> jntuh_eventmaster { get; set; }
        public DbSet<jntuh_facility_status> jntuh_facility_status { get; set; }
        public DbSet<jntuh_faculty_category> jntuh_faculty_category { get; set; }
        public DbSet<jntuh_faculty_deactivation_reason> jntuh_faculty_deactivation_reason { get; set; }
        public DbSet<jntuh_faculty_degree> jntuh_faculty_degree { get; set; }
        public DbSet<jntuh_faculty_edit_requests> jntuh_faculty_edit_requests { get; set; }
        public DbSet<jntuh_faculty_education> jntuh_faculty_education { get; set; }
        public DbSet<jntuh_faculty_education_edit_request> jntuh_faculty_education_edit_request { get; set; }
        public DbSet<jntuh_faculty_mobility_requests> jntuh_faculty_mobility_requests { get; set; }
        public DbSet<jntuh_faculty_news> jntuh_faculty_news { get; set; }
        public DbSet<jntuh_faculty_student_ratio_norms> jntuh_faculty_student_ratio_norms { get; set; }
        public DbSet<jntuh_faculty_subjects> jntuh_faculty_subjects { get; set; }
        public DbSet<jntuh_faculty_supporting_documents> jntuh_faculty_supporting_documents { get; set; }
        public DbSet<jntuh_faculty_type> jntuh_faculty_type { get; set; }
        public DbSet<jntuh_ffc_auditor> jntuh_ffc_auditor { get; set; }
        public DbSet<jntuh_ffc_auditor_campus> jntuh_ffc_auditor_campus { get; set; }
        public DbSet<jntuh_ffc_cluster_emails> jntuh_ffc_cluster_emails { get; set; }
        public DbSet<jntuh_ffc_committee> jntuh_ffc_committee { get; set; }
        public DbSet<jntuh_ffc_external_auditor_groups> jntuh_ffc_external_auditor_groups { get; set; }
        public DbSet<jntuh_ffc_order> jntuh_ffc_order { get; set; }
        public DbSet<jntuh_ffc_schedule> jntuh_ffc_schedule { get; set; }
        public DbSet<jntuh_generated_letters> jntuh_generated_letters { get; set; }
        public DbSet<jntuh_governingbodydesignations> jntuh_governingbodydesignations { get; set; }
        public DbSet<jntuh_grc_designation> jntuh_grc_designation { get; set; }
        public DbSet<jntuh_inactive_faculty> jntuh_inactive_faculty { get; set; }
        public DbSet<jntuh_infrastructure_parameters> jntuh_infrastructure_parameters { get; set; }
        public DbSet<jntuh_inspection_phase> jntuh_inspection_phase { get; set; }
        public DbSet<jntuh_lab_master> jntuh_lab_master { get; set; }
        public DbSet<jntuh_lab_master_experments> jntuh_lab_master_experments { get; set; }
        public DbSet<jntuh_land_category> jntuh_land_category { get; set; }
        public DbSet<jntuh_land_registration_type> jntuh_land_registration_type { get; set; }
        public DbSet<jntuh_land_type> jntuh_land_type { get; set; }
        public DbSet<jntuh_leavetype> jntuh_leavetype { get; set; }
        public DbSet<jntuh_link_screens> jntuh_link_screens { get; set; }
        public DbSet<jntuh_menu> jntuh_menu { get; set; }
        public DbSet<jntuh_miscellaneous_parameters> jntuh_miscellaneous_parameters { get; set; }
        public DbSet<jntuh_newsevents> jntuh_newsevents { get; set; }
        public DbSet<jntuh_noscmfaculty> jntuh_noscmfaculty { get; set; }
        public DbSet<jntuh_notin415faculty> jntuh_notin415faculty { get; set; }
        public DbSet<jntuh_pan_status> jntuh_pan_status { get; set; }
        public DbSet<jntuh_paymentrequests> jntuh_paymentrequests { get; set; }
        public DbSet<jntuh_paymentrequests_copy> jntuh_paymentrequests_copy { get; set; }
        public DbSet<jntuh_paymentresponse> jntuh_paymentresponse { get; set; }
        public DbSet<jntuh_paymentresponse_copy> jntuh_paymentresponse_copy { get; set; }
        public DbSet<jntuh_paymentresponse_new> jntuh_paymentresponse_new { get; set; }
        public DbSet<jntuh_phd_subject> jntuh_phd_subject { get; set; }
        public DbSet<jntuh_phdundertaking> jntuh_phdundertaking { get; set; }
        public DbSet<jntuh_physical_labmaster> jntuh_physical_labmaster { get; set; }
        public DbSet<jntuh_physical_labmaster_copy> jntuh_physical_labmaster_copy { get; set; }
        public DbSet<jntuh_program_type> jntuh_program_type { get; set; }
        public DbSet<jntuh_qualification> jntuh_qualification { get; set; }
        public DbSet<jntuh_registered_faculty_edit_fields> jntuh_registered_faculty_edit_fields { get; set; }
        public DbSet<jntuh_registered_faculty_education> jntuh_registered_faculty_education { get; set; }
        public DbSet<jntuh_registered_faculty_education_log> jntuh_registered_faculty_education_log { get; set; }
        public DbSet<jntuh_registered_faculty_log> jntuh_registered_faculty_log { get; set; }
        public DbSet<jntuh_registered_faculty_subjectstaught> jntuh_registered_faculty_subjectstaught { get; set; }
        public DbSet<jntuh_registration> jntuh_registration { get; set; }
        public DbSet<jntuh_scm> jntuh_scm { get; set; }
        public DbSet<jntuh_scm_new> jntuh_scm_new { get; set; }
        public DbSet<jntuh_scmproceedingrequest_addfaculty> jntuh_scmproceedingrequest_addfaculty { get; set; }
        public DbSet<jntuh_scmproceedingsrequests> jntuh_scmproceedingsrequests { get; set; }
        public DbSet<jntuh_scmproceedingsrequests_copy> jntuh_scmproceedingsrequests_copy { get; set; }
        public DbSet<jntuh_scmupload> jntuh_scmupload { get; set; }
        public DbSet<jntuh_shift> jntuh_shift { get; set; }
        public DbSet<jntuh_smssendstatus> jntuh_smssendstatus { get; set; }
        public DbSet<jntuh_society_other_colleges> jntuh_society_other_colleges { get; set; }
        public DbSet<jntuh_society_other_locations_colleges> jntuh_society_other_locations_colleges { get; set; }
        public DbSet<jntuh_software_norms> jntuh_software_norms { get; set; }
        public DbSet<jntuh_specialization> jntuh_specialization { get; set; }
        public DbSet<jntuh_sports_type> jntuh_sports_type { get; set; }
        public DbSet<jntuh_state> jntuh_state { get; set; }
        public DbSet<jntuh_subcomplaints> jntuh_subcomplaints { get; set; }
        public DbSet<jntuh_ugstudents> jntuh_ugstudents { get; set; }
        public DbSet<jntuh_university> jntuh_university { get; set; }
        public DbSet<jntuh_water_type> jntuh_water_type { get; set; }
        public DbSet<jntuh_year_in_degree> jntuh_year_in_degree { get; set; }
        public DbSet<my_aspnet_applications> my_aspnet_applications { get; set; }
        public DbSet<my_aspnet_membership> my_aspnet_membership { get; set; }
        public DbSet<my_aspnet_paths> my_aspnet_paths { get; set; }
        public DbSet<my_aspnet_personalizationallusers> my_aspnet_personalizationallusers { get; set; }
        public DbSet<my_aspnet_personalizationperuser> my_aspnet_personalizationperuser { get; set; }
        public DbSet<my_aspnet_profiles> my_aspnet_profiles { get; set; }
        public DbSet<my_aspnet_roles> my_aspnet_roles { get; set; }
        public DbSet<my_aspnet_sessioncleanup> my_aspnet_sessioncleanup { get; set; }
        public DbSet<my_aspnet_sessions> my_aspnet_sessions { get; set; }
        public DbSet<my_aspnet_sitemap> my_aspnet_sitemap { get; set; }
        public DbSet<my_aspnet_users> my_aspnet_users { get; set; }
        public DbSet<my_aspnet_usersinroles> my_aspnet_usersinroles { get; set; }
        public DbSet<temp_aeronautical> temp_aeronautical { get; set; }
        public DbSet<universities_table> universities_table { get; set; }
        public DbSet<user_browsers> user_browsers { get; set; }
        public DbSet<user_login_logout> user_login_logout { get; set; }
        public DbSet<jntu_phd> jntu_phd { get; set; }
        public DbSet<jntuh_college_attendace> jntuh_college_attendace { get; set; }
        public DbSet<jntuh_college_faculty_registered_copy> jntuh_college_faculty_registered_copy { get; set; }
        public DbSet<jntuh_samepan> jntuh_samepan { get; set; }
        public DbSet<jntuh_scmproceedingrequest_addfaculty_copy> jntuh_scmproceedingrequest_addfaculty_copy { get; set; }
        public DbSet<jntuh_settings> jntuh_settings { get; set; }
        public DbSet<jntuh_college_faculty> jntuh_college_faculty { get; set; }
        public DbSet<jntuh_registered_faculty_experience_log> jntuh_registered_faculty_experience_log { get; set; }
        public DbSet<jntuh_college_laboratories_copy> jntuh_college_laboratories_copy { get; set; }
        public DbSet<jntuh_lab_master_copy> jntuh_lab_master_copy { get; set; }
        public DbSet<jntuh_college_covidactivities> jntuh_college_covidactivities { get; set; }
        public DbSet<jntuh_covid_activites> jntuh_covid_activites { get; set; }
        public DbSet<jntuh_faculty_phddetails> jntuh_faculty_phddetails { get; set; }
        public DbSet<jntuh_college_aicteapprovedintake> jntuh_college_aicteapprovedintake { get; set; }
        public DbSet<jntuh_college_intake_adjustmentsdata> jntuh_college_intake_adjustmentsdata { get; set; }
        public DbSet<jntuh_college_intake_adjustments> jntuh_college_intake_adjustments { get; set; }
        public DbSet<jntuh_compliance_questionnaire> jntuh_compliance_questionnaire { get; set; }
        public DbSet<jntuh_college_facultydeclaration> jntuh_college_facultydeclaration { get; set; }
        public DbSet<jntuh_college_compliance_questionnaire> jntuh_college_compliance_questionnaire { get; set; }
        public DbSet<jntuh_college_nbaaccreditationdata> jntuh_college_nbaaccreditationdata { get; set; }
        public DbSet<jntuh_college> jntuh_college { get; set; }
        public DbSet<jntuh_college_affiliation_type> jntuh_college_affiliation_type { get; set; }
        public DbSet<jntuh_college_affiliation_tracking> jntuh_college_affiliation_tracking { get; set; }
        public DbSet<jntuh_college_chairperson> jntuh_college_chairperson { get; set; }
        public DbSet<jntuh_college_land> jntuh_college_land { get; set; }
        public DbSet<jntuh_selfappraisal> jntuh_selfappraisal { get; set; }
        public DbSet<jntuh_college_funds> jntuh_college_funds { get; set; }
        public DbSet<jntuh_extracurricularactivities> jntuh_extracurricularactivities { get; set; }
        public DbSet<jntuh_college_extracurricularactivities> jntuh_college_extracurricularactivities { get; set; }
        public DbSet<jntuh_college_womenprotection_antiragging_complaints> jntuh_college_womenprotection_antiragging_complaints { get; set; }
        public DbSet<jntuh_college_jntu_pci_aicte_intakes> jntuh_college_jntu_pci_aicte_intakes { get; set; }
        public DbSet<jntuh_college_jntu_pci_aicte_supportingdocs> jntuh_college_jntu_pci_aicte_supportingdocs { get; set; }
        public DbSet<jntuh_college_governingbody> jntuh_college_governingbody { get; set; }
        public DbSet<jntuh_college_courtcases> jntuh_college_courtcases { get; set; }
        public DbSet<jntuh_college_establishment> jntuh_college_establishment { get; set; }
        public DbSet<jntuh_college_faculty_information> jntuh_college_faculty_information { get; set; }
        public DbSet<jntuh_college_financialstandards> jntuh_college_financialstandards { get; set; }
        public DbSet<jntuh_college_placement> jntuh_college_placement { get; set; }
        public DbSet<jntuh_college_selfappraisal> jntuh_college_selfappraisal { get; set; }
        public DbSet<jntuh_pa_college_edit_status> jntuh_pa_college_edit_status { get; set; }
        public DbSet<jntuh_pa_college_screens_assigned> jntuh_pa_college_screens_assigned { get; set; }
        public DbSet<jntuh_college_booksandjournals> jntuh_college_booksandjournals { get; set; }
        public DbSet<jntuh_oladcolleges> jntuh_oladcolleges { get; set; }
        public DbSet<jntuh_direct_recruitment_principal> jntuh_direct_recruitment_principal { get; set; }
        public DbSet<jntuh_college_direct_principalrecruit> jntuh_college_direct_principalrecruit { get; set; }
        public DbSet<jntuh_college_direct_principal_experience> jntuh_college_direct_principal_experience { get; set; }
        public DbSet<jntuh_college_jhubactivities> jntuh_college_jhubactivities { get; set; }
        public DbSet<jntuh_jhubactivities> jntuh_jhubactivities { get; set; }
        public DbSet<jntuh_registered_faculty_experience> jntuh_registered_faculty_experience { get; set; }
        public DbSet<jntuh_college_facultytracking> jntuh_college_facultytracking { get; set; }
        public DbSet<jntuh_registered_faculty> jntuh_registered_faculty { get; set; }
        public DbSet<jntuh_principal_details_affiliated_colleges> jntuh_principal_details_affiliated_colleges { get; set; }
        public DbSet<jntuh_college_grievance_committee> jntuh_college_grievance_committee { get; set; }
        public DbSet<jntuh_college_women_protection_cell_rti_antiragging_details> jntuh_college_women_protection_cell_rti_antiragging_details { get; set; }
        public DbSet<jntuh_api_designations> jntuh_api_designations { get; set; }
        public DbSet<jntuh_apistudentspush_data> jntuh_apistudentspush_data { get; set; }
        public DbSet<jntuh_apistaffpush_data> jntuh_apistaffpush_data { get; set; }
        public DbSet<jntuh_academic_audit> jntuh_academic_audit { get; set; }
        public DbSet<jntuh_college_area> jntuh_college_area { get; set; }
        public DbSet<committeemembers2023_24> committeemembers2023_24 { get; set; }
        public DbSet<jntuh_college_paymentoffee> jntuh_college_paymentoffee { get; set; }
        public DbSet<jntuh_college_noc_mergecourse> jntuh_college_noc_mergecourse { get; set; }
        public DbSet<jntuh_college_noc_restoreintake> jntuh_college_noc_restoreintake { get; set; }
    }
}