using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace GemSharp {
    enum GeminiStatus {
        INPUT                       = 10,
        SENSITIVE_INPUT             = 11,
        SUCCESS                     = 20,
        REDIRECT_TEMPORARY          = 30,
        REDIRECT_PERMANENT          = 31,
        TEMPORARY_FAILURE           = 40,
        SERVER_UNAVAILABLE          = 41,
        CGI_ERROR                   = 42,
        PROXY_ERROR                 = 43,
        SLOW_DOWN                   = 44,
        PERMANENT_FAILURE           = 50,
        NOT_FOUND                   = 51,
        GONE                        = 52,
        PROXY_REQUEST_REFUSED       = 53,
        BAD_REQUEST                 = 59,
        CLIENT_CERTIFICATE_REQUIRED = 60,
        CERTIFICATE_NOT_AUTHORISED  = 61,
        CERTIFICATE_NOT_VALID       = 62
    }

    struct GeminiResponse {
        public GeminiStatus status;
        public string metadata;
        public string body;

        public GeminiResponse(string rawData) {
            // TODO: error handling
            this.status = (GeminiStatus)Byte.Parse(rawData.Split(' ', 2)[0]);
            string[] split = rawData.Split("\r\n", 2, StringSplitOptions.None);
            this.metadata = split[0];
            this.body = split[1];
        }
    }

    // https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslstream?view=net-8.0
    internal class Client {
        // The following method is invoked by the RemoteCertificateValidationDelegate
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            Console.WriteLine("Note: \"Trust On First Use\" (TOFU) SSL certificate authentication is currently unimplemented.\nAll certificates will be accepted by this client.\n");
            return true;
        }

        struct URL {
            public string url;
            public string protocol;
            public string domains;
            public string path;

            public URL (string url) {
                // TODO: other percent-encodings
                url = url.Replace(" ", "%20");
                string[] urlParts = url.Split(new string[] {"://", "/"}, 3, StringSplitOptions.None);
                this.url = url;
                this.protocol = urlParts[0];
                this.domains = urlParts[1];
                this.path= urlParts.Length == 3 ? urlParts[2] : "";
            }
        }

        public static GeminiResponse? Request(string absoluteUrl) {
            // Parse `absoluteUrl` into a convenient object
            URL url = new URL(absoluteUrl);

            // Create a TCP/IP client socket.
            TcpClient client = new TcpClient(url.domains, 1965);

            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback (ValidateServerCertificate),
                null
            );

            // The server name must match the name on the server certificate
            try {
                sslStream.AuthenticateAsClient(url.domains);
            }
            catch (AuthenticationException e) {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null) {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                return null;
            }

            // Make a Gemini request
            byte[] message = Encoding.UTF8.GetBytes($"{url.protocol}://{url.domains}/{url.path}\r\n");
            sslStream.Write(message);
            sslStream.Flush();

            // Read the server's response
            GeminiResponse response = GetResponse(sslStream);
            // Close the client connection
            client.Close();

            return response;
        }

        static GeminiResponse GetResponse(SslStream sslStream) {
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();

            while (sslStream.Read(buffer, 0, buffer.Length) != 0)
                messageData.Append(Encoding.UTF8.GetString(buffer));

            // Trim any remaining null bytes off the end of the message
            string messageString = messageData.ToString();
            messageString = messageString.TrimEnd('\0');

            return new GeminiResponse(messageString);
        }
    }
}