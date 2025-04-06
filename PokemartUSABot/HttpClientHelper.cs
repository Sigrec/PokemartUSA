namespace PokemartUSABot
{
    public class HttpClientHelper : IDisposable
    {
        private static readonly HttpClient _httpClient;
        private bool disposedValue;

        // Static constructor to initialize the HttpClient
        static HttpClientHelper()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "PokemartUSABot");
        }

        public static async Task<Stream?> GetSheetDataWithRetryAsync(string uri)
        {
            int maxRetries = 5;
            int retryDelay = 2000; // initial delay in milliseconds (2 seconds)

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Make the HTTP GET request to Google Sheets API
                    Stream response = await GetStreamAsync(uri);
                    return response;
                }
                catch (HttpRequestException ex)
                {
                    // Check if the exception is related to quota limits (e.g., 403)
                    if (ex.Message.Contains("Quota exceeded"))
                    {
                        if (attempt < maxRetries - 1)
                        {
                            // Wait before retrying (exponential backoff)
                            await Task.Delay(retryDelay);
                            retryDelay *= 2;  // Double the delay for the next attempt
                        }
                        else
                        {
                            throw new Exception("Max retry attempts reached. The Google Sheets API rate limit is exceeded.");
                        }
                    }
                    else
                    {
                        throw; // Rethrow if it's not related to rate limits
                    }
                }
            }

            return null; // If all attempts failed
        }

        public static async Task<string> GetAsync(string uri)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new HttpRequestException($"Failed to fetch data from {uri}. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log the error)
                throw new Exception($"An error occurred while making GET request to {uri}: {ex.Message}", ex);
            }
        }

        public static async Task<Stream> GetStreamAsync(string uri)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStreamAsync();
                }
                else
                {
                    throw new HttpRequestException($"Failed to fetch data from {uri}. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log the error)
                throw new Exception($"An error occurred while making GET request to {uri}: {ex.Message}", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _httpClient?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DistroProductSelector()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}