using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.Runtime;
using Newtonsoft.Json.Linq;
using System.IO;
using CesiumIonRevitAddin.Utils;
using Newtonsoft.Json;
using System.Threading;

namespace CesiumIonRevitAddin.CesiumIonClient
{
    public enum ConnectionStatus
    {
        Success,
        Failure,
        Cancelled
    }
    public class ConnectionResult
    {
        public ConnectionStatus Status { get; set; }
        public string Message { get; set; }

        public ConnectionResult(ConnectionStatus status, string message)
        {
            Status = status;
            Message = message;
        }
    }

    public static class RandomString
    {
        private static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        private static readonly RandomNumberGenerator rng = RandomNumberGenerator.Create();

        public static string GetUniqueString(int size)
        {
            var result = new char[size];
            byte[] data = new byte[size];

            rng.GetBytes(data);

            for (int i = 0; i < size; i++)
                result[i] = chars[data[i] % chars.Length];

            return new string(result);
        }
    }

    public static class Connection
    {
        private static readonly HttpClient client = new HttpClient();
        private static string ionServer;
        private static string apiServer;
        private static string clientID;
        private static string redirectUri;
        private static string localUrl = Path.Combine(Util.GetAddinUserDataFolder(), "ion_token.json");
        private static string codeVerifier;

        public static void Disconnect()
        {
            // Remove the token from the file system
            if (File.Exists(localUrl))
            {
                File.Delete(localUrl);
            }
        }

        public static async Task<ConnectionResult> ConnectToIon(string remoteUrl, string apiUrl, string responseType, string clientID, string redirectUri, string scope, CancellationToken cancellationToken)
        {
            Connection.ionServer = remoteUrl;
            Connection.apiServer = apiUrl;
            Connection.redirectUri = redirectUri;
            Connection.clientID = clientID;

            // Code challenge generation
            SHA256 encrypter = SHA256.Create();
            codeVerifier = RandomString.GetUniqueString(32);
            var hashed = encrypter.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            var encoded = Convert.ToBase64String(hashed);
            string codeChallenge = encoded.Replace("=", "").Replace('+', '-').Replace('/', '_');

            UriBuilder uriBuilder = new UriBuilder(remoteUrl)
            {
                Path = "oauth"
            };

            var queryParams = HttpUtility.ParseQueryString(string.Empty);
            queryParams["response_type"] = responseType;
            queryParams["client_id"] = clientID;
            queryParams["redirect_uri"] = Connection.redirectUri;
            queryParams["scope"] = scope;
            queryParams["code_challenge"] = codeChallenge;
            queryParams["code_challenge_method"] = "S256";
            uriBuilder.Query = queryParams.ToString();

            Uri finalUri = uriBuilder.Uri;

            // Start the listen server with cancellation support
            Task<ConnectionResult> listenTask = Task.Run(() => StartListenServer(cancellationToken), cancellationToken);

            // Load the browser to the login page
            OpenBrowser(finalUri.ToString());

            try
            {
                // Wait for either the listening task to complete or for cancellation
                return await listenTask;
            }
            catch (OperationCanceledException)
            {
                return new ConnectionResult(ConnectionStatus.Cancelled, "Connection cancelled by the user");
            }
        }

        private static async Task<ConnectionResult> StartListenServer(CancellationToken cancellationToken)
        {
            bool success = false;

            try
            {
                Uri uri = new Uri(redirectUri);
                string baseUri = $"{uri.Scheme}://{uri.Host}:{uri.Port}/";
                string callbackPath = uri.AbsolutePath;

                using (HttpListener listener = new HttpListener())
                {
                    listener.Prefixes.Add(baseUri);
                    listener.Start();
                    Debug.WriteLine("Listening for OAuth callback on " + baseUri);

                    // Wait for a request or cancellation
                    Task<HttpListenerContext> getContextTask = listener.GetContextAsync();
                    Task completedTask = await Task.WhenAny(getContextTask, Task.Delay(-1, cancellationToken));

                    // Check if the task completed due to cancellation
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Debug.WriteLine("StartListenServer operation was cancelled.");
                        listener.Stop();
                        return new ConnectionResult(ConnectionStatus.Cancelled, "StartListenServer operation was cancelled");
                    }

                    if (completedTask == getContextTask)
                    {
                        HttpListenerContext context = await getContextTask;

                        // Check if the request is for the specified callback path
                        if (context.Request.Url.AbsolutePath == callbackPath)
                        {
                            // Extract the code from the query string
                            string query = context.Request.Url.Query;

                            // Send a response to the browser so the user knows the request was received
                            string responseString = "<html><body>Authorization successful! You may close this window.</body></html>";
                            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                            context.Response.ContentLength64 = buffer.Length;
                            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            context.Response.OutputStream.Close();

                            // Process the query string to extract the authorization code and state
                            if (!string.IsNullOrEmpty(query) && query.Contains("code="))
                            {
                                Debug.WriteLine("Authorization code received.");

                                // Pass the query string to your RequestToken method
                                Task<bool> tokenRequest = RequestToken(query);
                                tokenRequest.Wait();

                                if (tokenRequest.Result)
                                {
                                    Debug.WriteLine("Token request successful. Access token saved.");
                                    success = true;
                                }
                                else
                                {
                                    Debug.WriteLine("Token request failed.");
                                }
                            }
                            else
                            {
                                Debug.WriteLine("No authorization code received.");
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Received request on a different path: {context.Request.Url.AbsolutePath}");
                        }
                    }
                    listener.Stop();
                }
            }
            catch (HttpListenerException e)
            {
                Debug.WriteLine("HttpListener exception: " + e.Message);
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("StartListenServer operation was cancelled.");
            }
            catch (Exception e)
            {
                Debug.WriteLine("General exception: " + e.Message);
            }

            if (success)
            {
                return new ConnectionResult(ConnectionStatus.Success, "Successfully connected to Cesium ion");
            }
            else
            {
                return new ConnectionResult(ConnectionStatus.Failure, "Failed to connect to Cesium ion");
            }

        }

        public static async Task<ConnectionResult> Upload(string filePath, string name, string description, string attribution, string type, string sourceType, string inputCrs, IProgress<double> progress = null)
        {
            description = description.Replace("__\\n__", "\n");
            attribution = attribution.Replace("__\\n__", "\n");

            // Prepare the content to send to the API
            var content = new JObject
            {
                { "name", name },
                { "description", description },
                { "attribution", attribution },
                { "type", type },
                {
                    "options", new JObject
                    {
                        { "sourceType", sourceType },
                        { "inputCrs", inputCrs }
                    }
                }
            };

            var POSTContent = new StringContent(content.ToString(), Encoding.UTF8, "application/json");

            string token = GetAccessToken();
            if (token == null)
            {
                Debug.WriteLine("No access token found.");
                return new ConnectionResult(ConnectionStatus.Failure, "No access token found.");
            }

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            client.DefaultRequestHeaders.Add("json", "true");

            UriBuilder uriBuilder = new UriBuilder(GetApiUrl())
            {
                Path = "v1/assets"
            };

            try
            {
                // Post the asset to the Cesium ion API
                using (HttpResponseMessage responseMessage = await client.PostAsync(uriBuilder.Uri, POSTContent))
                {
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        string error = await responseMessage.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error: " + error);
                        return new ConnectionResult(ConnectionStatus.Failure, $"API Error: {error}");
                    }

                    // Get the information to prepare for upload to S3
                    string responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    JObject responseJson = JObject.Parse(responseContent);
                    JObject uploadLocation = responseJson["uploadLocation"] as JObject;
                    JObject onComplete = responseJson["onComplete"] as JObject;

                    // Prepare the S3 Client and upload
                    SessionAWSCredentials credentials = new SessionAWSCredentials(
                        (string)uploadLocation["accessKey"],
                        (string)uploadLocation["secretAccessKey"],
                        (string)uploadLocation["sessionToken"]);

                    using (var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USEast1))
                    using (var fileTransferUtility = new TransferUtility(s3Client))
                    {
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            FilePath = filePath,
                            BucketName = uploadLocation["bucket"].ToString(),
                            Key = uploadLocation["prefix"].ToString() + Path.GetFileName(filePath)
                        };

                        // Track the total file size
                        long totalBytes = new FileInfo(filePath).Length;

                        // Event handler to show upload progress
                        void UploadProgressHandler(object sender, UploadProgressArgs e)
                        {
                            double percentComplete = (double)e.TransferredBytes / totalBytes * 100;
                            // Report progress to the caller (if provided)
                            progress?.Report(percentComplete);
                        }

                        // Attach the progress event handler
                        uploadRequest.UploadProgressEvent += UploadProgressHandler;

                        // Perform the upload
                        await fileTransferUtility.UploadAsync(uploadRequest);

                        // Detach the event handler after upload is complete
                        uploadRequest.UploadProgressEvent -= UploadProgressHandler;

                        // Report 100% completion at the end
                        progress?.Report(100);

                        // Notify the API that the upload is complete
                        var completeContent = new StringContent(onComplete["fields"].ToString(), Encoding.UTF8, "application/json");
                        await client.PostAsync((string)onComplete["url"], completeContent);

                        // Construct the asset ID and open the asset in a browser
                        string id = (string)uploadLocation["prefix"];
                        id = id.Substring(id.IndexOf("/"));
                        uriBuilder = new UriBuilder(GetIonUrl()) { Path = "assets" + id };

                        // Return success
                        return new ConnectionResult(ConnectionStatus.Success, uriBuilder.Uri.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error encountered during upload: {e.Message}");
                return new ConnectionResult(ConnectionStatus.Failure, $"Error: {e.Message}");
            }
        }

        public static string GetAccessToken()
        {
            return GetSavedJsonValue("access_token");
        }

        public static string GetApiUrl()
        {
            return GetSavedJsonValue("api_url");
        }

        public static string GetIonUrl()
        {
            return GetSavedJsonValue("ion_url");
        }

        public static bool IsConnected()
        {
            return GetAccessToken() != null;
        }

        private static string GetSavedJsonValue(string key)
        {
            try
            {
                var jsonObject = ReadConnectionData();

                if (jsonObject == null)
                    return null;

                if (jsonObject.TryGetValue(key, out var token))
                {
                    return token.ToString(); // Return the access token as a string
                }
                else
                {
                    Debug.WriteLine($"{key} not found in JSON.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An unexpected error occurred: " + ex.Message);
                return null;
            }
        }

        public static void OpenBrowser(string url)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw new PlatformNotSupportedException("Unknown platform, unable to open URL.");
                }
            }
            catch (Exception ex)
            {
                // Handle or log exception (e.g., Process.Start can throw exceptions)
                Console.WriteLine($"Failed to open browser: {ex.Message}");
            }
        }

        private static async Task<bool> RequestToken(string query)
        {
            // Use HttpUtility.ParseQueryString to parse the query string
            var queryParams = HttpUtility.ParseQueryString(query);

            // Get the value of the 'code' parameter
            string code = queryParams["code"];
            string state = queryParams["state"];


            UriBuilder uriBuilder = new UriBuilder(apiServer);
            uriBuilder.Path = Path.Combine(uriBuilder.Path.TrimEnd('/'), "oauth/token/");

            Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "client_id", clientID },
            { "code", code },
            { "redirect_uri", redirectUri },
            { "code_verifier", codeVerifier}
        };
            var POSTContent = new FormUrlEncodedContent(parameters);

            using (HttpResponseMessage responseMessageToken = await client.PostAsync(uriBuilder.Uri, POSTContent).ConfigureAwait(false))
            {
                string contentToken = await responseMessageToken.Content.ReadAsStringAsync().ConfigureAwait(false);
                JObject jsonObject = JObject.Parse(contentToken);
                
                if (responseMessageToken.IsSuccessStatusCode)
                {
                    WriteConnectionData(jsonObject["access_token"].ToString(), Connection.apiServer, Connection.ionServer);
                    return true;
                }
            }
            return false;
        }

        private static void WriteConnectionData(string token, string apiUrl, string ionUrl)
        {
            JObject jsonObject = new JObject();
            jsonObject["access_token"] = token;
            jsonObject["api_url"] = apiUrl;
            jsonObject["ion_url"] = ionUrl;
            
            // Encrypt the JSON string using DPAPI
            byte[] encryptedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(jsonObject.ToString()), null, DataProtectionScope.CurrentUser);

            // Write the encrypted data to a file
            File.WriteAllBytes(localUrl, encryptedData);
        }

        private static JObject ReadConnectionData()
        {
            if (!File.Exists(localUrl))
            {
                return null;
            }

            try
            {
                byte[] encryptedData = File.ReadAllBytes(localUrl);

                // Decrypt the data using DPAPI
                byte[] decryptedData = ProtectedData.Unprotect(encryptedData, null, DataProtectionScope.CurrentUser);

                string jsonContent = Encoding.UTF8.GetString(decryptedData);

                return JObject.Parse(jsonContent);
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine("Failed to parse JSON: " + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An unexpected error occurred: " + ex.Message);
                return null;
            }
        }

    }

}


