using System;
using System.Composition;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Microsoft.Alm.Authentication;

namespace Guit
{
    [Shared]
    public class DefaultExports
    {
        [Export(typeof(CredentialsHandler))]
        public CredentialsHandler CredentialsHandler { get; } = new CredentialsHandler(OnHandleCredentials);

        static Credentials OnHandleCredentials(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var secrets = new SecretStore("git");
            var auth = new BasicAuthentication(secrets);
            // See https://github.com/microsoft/Git-Credential-Manager-for-Windows/issues/859
            var uri = new Uri(url);
            var creds = auth.GetCredentials(new TargetUri(uri.GetComponents(UriComponents.Scheme | UriComponents.Host, UriFormat.Unescaped)));
            return new UsernamePasswordCredentials
            {
                Username = creds.Username,
                Password = creds.Password,
            };
        }
    }
}
