using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSCAssignment2.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace CSCAssignment2.APIs
{
    public class CartController : Controller
    {
        string key = "sk_test_51GxdLDHq05oyY0YBoHTN18NJHgarUMDCNAHpcBgYhBLseyoKXCOwtB9DtBxRlWJhnCaw1DBZ6QVvCme5g07hcVfP00VqfSJeKC"; // input Stripe API Key here
        string customerId = "cus_HlBMiYVkYrzsNG"; // input customer id here
        string productPrice1 = "price_1HBe0SHq05oyY0YBADFXyH6v"; // input product1 price here
        string subscriptionId = ""; // input subscription id here
        string paymentIntent = ""; // input payment intent here

        // POST: Cart/Subscribe
        [HttpPost]
        public ActionResult Subscribe()
        {
            try
            {
                // Use Stripe's library to make request
                StripeConfiguration.ApiKey = key;
                StripeConfiguration.MaxNetworkRetries = 2;

                var options = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                        Price = productPrice1,
                        },
                    },
                };

                var service = new SubscriptionService();
                Subscription subscription = service.Create(options);

                var model = new SubscriptionViewModel();
                model.SubscriptionId = subscription.Id;

                //return View("OrderStatus", model);
                return View("OrderStatus");
            }
            catch (StripeException e)
            {
                var x = new
                {
                    status = "Failed",
                    message = e.Message
                };
                return this.Json(x);
            }
        }

        // POST: Cart/Pause
        [HttpPost]
        public ActionResult Pause()
        {
            try
            {
                StripeConfiguration.ApiKey = key;
                StripeConfiguration.MaxNetworkRetries = 2;

                var options = new SubscriptionUpdateOptions
                {
                    PauseCollection = new SubscriptionPauseCollectionOptions
                    {
                        Behavior = "void",
                        ResumesAt = DateTime.Today.AddDays(1)
                    },
                };
                var service = new SubscriptionService();
                service.Update(subscriptionId, options);

                return View("OrderStatus");
            }
            catch (StripeException e)
            {
                var x = new
                {
                    status = "Failed",
                    message = e.Message
                };
                return this.Json(x);
            }
        }

        // POST: Cart/Resume
        [HttpPost]
        public ActionResult Resume()
        {
            try
            {
                StripeConfiguration.ApiKey = key;
                StripeConfiguration.MaxNetworkRetries = 2;

                var options = new SubscriptionUpdateOptions();
                options.AddExtraParam("pause_collection", "");
                var service = new SubscriptionService();
                service.Update(subscriptionId, options);

                return View("OrderStatus");
            }
            catch (StripeException e)
            {
                var x = new
                {
                    status = "Failed",
                    message = e.Message
                };
                return this.Json(x);
            }
        }

        // POST: Cart/Refund
        [HttpPost]
        public ActionResult Refund()
        {
            try
            {
                StripeConfiguration.ApiKey = key;
                StripeConfiguration.MaxNetworkRetries = 2;

                var refunds = new RefundService();
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntent
                };
                var refund = refunds.Create(refundOptions);

                return View("OrderStatus");
            }
            catch (StripeException e)
            {
                var x = new
                {
                    status = "Failed",
                    message = e.Message
                };
                return this.Json(x);
            }
        }

        // POST: Cart/Cancel
        [HttpPost]
        public ActionResult Cancel()
        {
            try
            {
                StripeConfiguration.ApiKey = key;
                StripeConfiguration.MaxNetworkRetries = 2;

                var service = new SubscriptionService();
                var cancelOptions = new SubscriptionCancelOptions
                {
                    InvoiceNow = false,
                    Prorate = false,
                };
                Subscription subscription = service.Cancel(subscriptionId, cancelOptions);

                return View("OrderStatus");
            }
            catch (StripeException e)
            {
                var x = new
                {
                    status = "Failed",
                    message = e.Message
                };
                return this.Json(x);
            }
        }
    }
}
