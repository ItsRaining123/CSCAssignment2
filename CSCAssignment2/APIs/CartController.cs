using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using CSCAssignment2.Helpers;
using CSCAssignment2.Models;
using CSCAssignment2.Services;
using ExamScriptTS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Options;
using Stripe;

namespace CSCAssignment2.APIs
{
    public class CartController : Controller
    {
        private readonly CSCAssignment2DbContext _context;
        private IUserService _userService;
        private CSCAssignment2DbContext _database;
        private AppSettings _appSettings;
        private readonly IPutItem _putItem;
        private readonly IGetItem _getItem;
        private readonly IAmazonDynamoDB _dynamoClient;

        public CartController(CSCAssignment2DbContext context, IUserService userService, CSCAssignment2DbContext database, IOptions<AppSettings> appSettings, IPutItem putItem, IGetItem getItem, IAmazonDynamoDB dynamoClient)
        {
            _context = context;
            _userService = userService;
            _database = database;
            _appSettings = appSettings.Value;
            _putItem = putItem;
            _getItem = getItem;
            _dynamoClient = dynamoClient;
        }

        string key = "sk_test_51GxdLDHq05oyY0YBoHTN18NJHgarUMDCNAHpcBgYhBLseyoKXCOwtB9DtBxRlWJhnCaw1DBZ6QVvCme5g07hcVfP00VqfSJeKC"; // input Stripe API Key here
        string customerId = "cus_HlBMiYVkYrzsNG"; // input customer id here
        string productPrice1 = "price_1HBlswHq05oyY0YBYSWDL0KX"; // input product1 price here
        string subscriptionId = ""; // input subscription id here
        string paymentIntent = ""; // input payment intent here
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();

        // GET: Cart/Invoice/5
        [HttpGet("Cart/Invoice/{id}")]
        public async Task<IActionResult> GetItem(int? id)
        {
            var response = await _getItem.GetItems(id);
            //var service = new InvoiceService();
            //service.Get(response.Items);

            return Ok(response);
        }

        // POST: Cart/Subscribe/5
        [HttpPost("Cart/Subscribe/{id}")]
        public ActionResult Subscribe(int id)
        {
            var user = _database.Users.Where(u => u.Id == id).FirstOrDefault();
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

                user.CustomerId = subscription.CustomerId;

                _putItem.AddNewEntry(subscription.Id, subscription.LatestInvoiceId, user.Id);
                _database.Users.Update(user);
                _database.SaveChanges();

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
