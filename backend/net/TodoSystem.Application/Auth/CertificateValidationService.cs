using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace TodoSystem.Application.Auth
{
    public class CertificateValidationService
    {
        public bool ValidateCertificate(X509Certificate2 clientCertificate)
        {
            // Load the .p12 certificate from the certs folder
            var certPath = Path.Combine(AppContext.BaseDirectory, "certs", "todo-certificate.p12");
            var cert = new X509Certificate2(certPath, "test");

            return clientCertificate.Thumbprint == cert.Thumbprint;
        }
    }
}