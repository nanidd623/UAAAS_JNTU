﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UAAAS.WebABAS {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="WebABAS.GetAffiliationDataSoap")]
    public interface GetAffiliationDataSoap {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Faculty", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string Faculty(string FacultyName, string RegistrationNo, string AadhaarNo, string CollegeID, string DepartmentID, string Designation, string Address, string City, string State, string Gender, string Dob, string MobileNo, string Email, string Pincode, string Type);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/Principal", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string Principal(string PrincipalName, string RegistrationNo, string AadhaarNo, string CollegeID, string PrincipalType);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/DeleteFaculty", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string DeleteFaculty(string RegistrationNo);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/DeletePrincipal", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        string DeletePrincipal(string RegistrationNo);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface GetAffiliationDataSoapChannel : UAAAS.WebABAS.GetAffiliationDataSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class GetAffiliationDataSoapClient : System.ServiceModel.ClientBase<UAAAS.WebABAS.GetAffiliationDataSoap>, UAAAS.WebABAS.GetAffiliationDataSoap {
        
        public GetAffiliationDataSoapClient() {
        }
        
        public GetAffiliationDataSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public GetAffiliationDataSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GetAffiliationDataSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GetAffiliationDataSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string Faculty(string FacultyName, string RegistrationNo, string AadhaarNo, string CollegeID, string DepartmentID, string Designation, string Address, string City, string State, string Gender, string Dob, string MobileNo, string Email, string Pincode, string Type) {
            return base.Channel.Faculty(FacultyName, RegistrationNo, AadhaarNo, CollegeID, DepartmentID, Designation, Address, City, State, Gender, Dob, MobileNo, Email, Pincode, Type);
        }
        
        public string Principal(string PrincipalName, string RegistrationNo, string AadhaarNo, string CollegeID, string PrincipalType) {
            return base.Channel.Principal(PrincipalName, RegistrationNo, AadhaarNo, CollegeID, PrincipalType);
        }
        
        public string DeleteFaculty(string RegistrationNo) {
            return base.Channel.DeleteFaculty(RegistrationNo);
        }
        
        public string DeletePrincipal(string RegistrationNo) {
            return base.Channel.DeletePrincipal(RegistrationNo);
        }
    }
}