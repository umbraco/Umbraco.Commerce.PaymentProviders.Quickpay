using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
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
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<PaymentLinkUrl> CreatePaymentLinkAsync(string paymentId, QuickpayPaymentLinkRequest data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/link", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .PutJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<PaymentLinkUrl>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> GetPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}", async (req, ct) => await req
                .GetJsonAsync<QuickpayPayment>(cancellationToken: ct).ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> CancelPaymentAsync(string paymentId, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/cancel", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .SetQueryParam("synchronized", string.Empty)
                .PostJsonAsync(null, cancellationToken: ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> CapturePaymentAsync(string paymentId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/capture", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .SetQueryParam("synchronized", string.Empty)
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<QuickpayPayment> RefundPaymentAsync(string paymentId, object data, CancellationToken cancellationToken = default)
        {
            return await RequestAsync($"/payments/{paymentId}/refund", async (req, ct) => await req
                .WithHeader("Content-Type", "application/json")
                .SetQueryParam("synchronized", string.Empty)
                .PostJsonAsync(data, cancellationToken: ct)
                .ReceiveJson<QuickpayPayment>().ConfigureAwait(false),
                cancellationToken).ConfigureAwait(false);
        }

        private async Task<TResult> RequestAsync<TResult>(string url, Func<IFlurlRequest, CancellationToken, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            var result = default(TResult);

            try
            {
                FlurlRequest req = new FlurlRequest(_config.BaseUrl + url)
                        .WithSettings(x =>
                        {
                            var jsonSettings = new System.Text.Json.JsonSerializerOptions
                            {
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
                            };
                            x.JsonSerializer = new CustomFlurlJsonSerializer(jsonSettings);
                        })
                        .WithHeader("Accept-Version", "v10")
                        .WithHeader("Authorization", _config.Authorization);

                result = await func.Invoke(req, cancellationToken).ConfigureAwait(false);
            }
            catch (FlurlHttpException ex)
            {
                throw;
            }

            return result;
        }
    }
}
