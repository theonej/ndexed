using CuttingEdge.Conditions;
using NDexed.Domain.Models.Payment;
using NDexed.Domain.Models.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NDexed.Domain;
using NDexed.Domain.Models;

namespace NDexed.Payments.Stripe
{
    public class StripePaymentProvider : IPaymentProvider
    {
        private readonly string m_ApiKey;

        private const string API_BASE_URL = "https://api.stripe.com/v1/";

        private const decimal TRANSACTION_CHARGE_MULTIPLIER = .029m;
        private const decimal TRANSACTION_CHARGE_DOLLARS = .30m;
        private const string CURRENCY_CODE = "usd";

        public StripePaymentProvider()
        {
            m_ApiKey = ConfigurationManager.AppSettings["PaymentProviderApiKey"];
            Condition.Requires(m_ApiKey).IsNotNull();
        }

        public CustomerInfo CreateCustomer(UserInfo userData)
        {
            HttpClient client = InitializeClient();

            StringContent content = new StringContent("{}");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = client.PostAsync("customers", content).Result;
            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(responseContent);
            }

            dynamic customerObject = JsonConvert.DeserializeObject(responseContent);

            CustomerInfo customer = new CustomerInfo();
            customer.Id = Guid.NewGuid();
            customer.Key = customerObject.id;

            return customer;
        }

        public PaymentCardInfo AddPaymentCard(CustomerInfo customer, PaymentCardInfo paymentCard)
        {
            HttpClient client = InitializeClient();

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("card", paymentCard.CardToken));

            FormUrlEncodedContent postContent = new FormUrlEncodedContent(postData);
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            string requestUrl = string.Format("customers/{0}/cards", customer.Key);
            HttpResponseMessage response = client.PostAsync(requestUrl, postContent).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(responseContent);
            }

            dynamic cardObject = JsonConvert.DeserializeObject(responseContent);

            PaymentCardInfo createdCard = new PaymentCardInfo();
            createdCard.CardToken = cardObject.id;
            createdCard.Id = Guid.NewGuid();
            createdCard.LastFourDigits = cardObject.last4;

            return createdCard;
        }

        public PaymentConfirmationInfo SubmitPayment(PaymentCardInfo paymentCard, PaymentInfo payment)
        {
            HttpClient client = InitializeClient();

            //payment amounts are submitted as decimals representing cents
            int totalPayment = (int)(payment.TotalPaymentAmount * 100);

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("card", paymentCard.CardToken));
            postData.Add(new KeyValuePair<string, string>("amount", totalPayment.ToString()));
            postData.Add(new KeyValuePair<string, string>("currency", CURRENCY_CODE));

            FormUrlEncodedContent postContent = new FormUrlEncodedContent(postData);
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = client.PostAsync("charges", postContent).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(responseContent);
            }

            dynamic paymentConfirmation = JsonConvert.DeserializeObject(responseContent);

            PaymentConfirmationInfo confirmation = new PaymentConfirmationInfo();
            confirmation.Id = Guid.NewGuid();
            confirmation.ConfirmationToken = paymentConfirmation.id;
            confirmation.Status = paymentConfirmation.created;

            return confirmation;
        }

        public string GenerateCardToken(PaymentCardInfo paymentCard)
        {
            HttpClient client = InitializeClient();

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("card[number]", paymentCard.Number));
            postData.Add(new KeyValuePair<string, string>("card[exp_month]", paymentCard.Expiration.ToString("MM")));
            postData.Add(new KeyValuePair<string, string>("card[exp_year]", paymentCard.Expiration.ToString("yyyy")));
            postData.Add(new KeyValuePair<string, string>("card[cvc]", paymentCard.CVC));

            FormUrlEncodedContent postContent = new FormUrlEncodedContent(postData);
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = client.PostAsync("tokens", postContent).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(responseContent);
            }

            dynamic tokenObject = JsonConvert.DeserializeObject(responseContent);

            return tokenObject.id;
        }

        #region Private Methods

        private HttpClient InitializeClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(API_BASE_URL);

            string value = string.Format("{0}:{1}", m_ApiKey, string.Empty);
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            string encodedValue = Convert.ToBase64String(buffer);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", encodedValue);

            return client;
        }

        #endregion


    }
}
