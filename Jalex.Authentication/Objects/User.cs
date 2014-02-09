using System;
using System.Text;
using Jalex.Authentication.Attributes;
using Jalex.Authentication.DynamoDB;
using Jalex.Infrastructure.Attributes;

namespace Jalex.Authentication.Objects
{
    public class User
    {
        public enum RegistrationResult
        {
            Success,
            Duplicate,
            Failure
        }

        public enum AccountState
        {
            Active, Inactive,
            Visiting,
            NotVerified,
            Locked,
            AdminLocked,
            Verified
        }

        public enum Gender
        {
            Male,
            Female
        }

        public class Role
        {
            public string id { get; set; }
        }

        public class Person
        {
            public AccountState accountState { get; set; }
            public bool billingSameAsMailing { get; set; }
            //public Entitlement[] bmidpTVEEntitlements { get; set; }
            public string[] bmidpTVEEntitlements { get; set; }

            [Id]
            public string bmidpuid { get; set; }
            public string bmidpValidationToken { get; set; }
            public string bmidpTokenCreationTime { get; set; }
            public int bmidpTokenValidationPeriod { get; set; }
            public bool bmidpIsAccountOwner { get; set; }
            public string bmidpHouseholdAccountId { get; set; }

            [NIProfile]
            public object[] devices { get; set; }

            [Ignore]
            public DateTime dateOfBirth
            {
                get { return DateTime.ParseExact(_dateOfBirth, "MMddyyyy", null); }
                set { dateOfBirth = value; _dateOfBirth = value.ToString("MMddyyyy"); }
            }
            //        [JsonProperty("dateOfBirth")]
            public string _dateOfBirth { get; set; }
            public string email { get; set; }
            public string familyName { get; set; }
            public string fullName { get; set; }
            public Gender gender { get; set; }
            public string[] geoLocation { get; set; }
            public string givenName { get; set; }
            public string[] managedBy { get; set; }
            public Metadata meta { get; set; }

            public string mailingCity { get; set; }
            public string mailingCountryCode { get; set; }
            public string mailingPostalCode { get; set; }
            public string mailingProvince { get; set; }
            public string mailingStreet { get; set; }

            public string billingCity { get; set; }
            public string billingCountryCode { get; set; }
            public string billingPostalCode { get; set; }
            public string billingProvince { get; set; }
            public string billingStreet { get; set; }

            public string nickName { get; set; }
            //public string @operator { get; set; }
            public string userPassword { get; set; }
            public string telephoneNumber { get; set; }
            public string prefLanguage { get; set; }
            public string redirectFrom { get; set; }
            public string salutation { get; set; }
            public bool userLocked { get; set; }
            public bool bmidpTCAndPrivacyPolicy { get; set; }
            public bool bmidpUserPersonalInfoAgreement { get; set; }
            public Role[] roles { get; set; }
        }

        public class RegistrationRequest
        {
            public bool createAccount { get; set; }
            public string data { get; set; }
            public string userIP { get; set; }
            public bool noEmailValidation { get; set; }
            public bool noEmailConfirmation { get; set; }
            public RegistrationRequest()
            {
                noEmailValidation = false;
                noEmailConfirmation = false;
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("createAccount={0}", createAccount);
                sb.Append("&data=" + data);
                sb.AppendFormat("&userIP={0}", userIP);
                sb.AppendFormat("&noEmailValidation={0}", noEmailValidation);
                sb.AppendFormat("&noEmailConfirmation={0}", noEmailConfirmation);

                return sb.ToString();
            }
        }

        public class RegistrationResponse
        {
            public RegistrationResult result { get; set; }
            public Guid id { get; set; }
            public AccountState state { get; set; }
            public string error { get; set; }
        }

        public class NIProfile
        {
            public string UserID { get; set; }
            public int AssetID { get; set; }
            public string Name { get; set; }
            public string Data { get; set; }
            public DateTime Created { get; set; }
        }
    }
}
