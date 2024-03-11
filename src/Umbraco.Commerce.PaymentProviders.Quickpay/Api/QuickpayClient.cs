using Flurl.Http;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Commerce.PaymentProviders.Quickpay.Api.Models;

namespace Umbraco.Commerce.PaymentProviders.Quickpay.Api
{
    public class QuickpayClient
    {
        private QuickpayClientConfig _config;

        public QuickpayClient(QuickpayClientConfig config)
        {
            _config = config;
        }

        public async Task<QuickpayPayment> CreatePaymentAsync(QuickpayPaymentRequest data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync("/payments", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PostJsonAsync(data, ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<PaymentLinkUrl> CreatePaymentLinkAsync(string paymentId, QuickpayPaymentLinkRequest data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/link", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PutJsonAsync(data, ct)
                .ReceiveJson<PaymentLinkUrl>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}", async (req, ct) => await req
                .GetJsonAsync<QuickpayPayment>(ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> CancelPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/cancel", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .SetQueryParam("synchronized", string.Empty)
                .PostJsonAsync(null, ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> CapturePaymentAsync(string paymentId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/capture", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .SetQueryParam("synchronized", string.Empty)
                .PostJsonAsync(data, ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> RefundPaymentAsync(string paymentId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/refund", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .SetQueryParam("synchronized", string.Empty)
                .PostJsonAsync(data, ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<TResult> RequestAsync<TResult>(string url, Func<IFlurlRequest, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var result = default(TResult);

            try
            {
                var req = new FlurlRequest(_config.BaseUrl + url)
                        .ConfigureRequest(x =>
                        {
                            var jsonSettings = new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                MissingMemberHandling = MissingMemberHandling.Ignore
                            };
                            x.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Accept-Version", "v10")
                        .WithHeader("Authorization", _config.Authorization);

                result = await func.Invoke(req, cancellationToken).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }
    }
}
