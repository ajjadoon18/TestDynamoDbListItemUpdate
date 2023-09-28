using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;

namespace DynamoDbDataExporter
{
    public static class SsoHelper
    {
        /// <summary>
        /// Get SSO credentials from the information in the shared config file.
        /// </summary>
        public static AWSCredentials LoadSsoCredentials(string profile)
        {
            Console.WriteLine($"Loading credentials from profile {profile}");
            var chain = new CredentialProfileStoreChain();
            if (!chain.TryGetAWSCredentials(profile, out var credentials))
                throw new Exception($"Failed to find the {profile} profile");
            return credentials;
        }

        public static async Task PrintCredentials(AWSCredentials? credentials)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            var ssoProfileClient = new AmazonSecurityTokenServiceClient(credentials);
            var ssoProfile = (await ssoProfileClient.GetCallerIdentityAsync(new GetCallerIdentityRequest())).Arn;

            Console.WriteLine($"SSO Profile: {ssoProfile}");
        }
    }
}
