using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Recombee.ApiClient;
using Recombee.ApiClient.ApiRequests;
using Recombee.ApiClient.Bindings;

namespace CSCAssignment2.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        [HttpGet("GetRecommendations")]
        public IActionResult GetRecommendations()
        {
            var client = new RecombeeClient("cscassignment-dev", "");

            //client.Send(new AddDetailView("ifijj", "88dj", cascadeCreate: true));
            //client.Send(new AddDetailView("ljsfjd3231", "88dj", cascadeCreate: true));

            //Send a purchase to recombee whenever user completes a desired goal to get a better quality of recommendations
            //client.Send(new AddPurchase("ifijj", "88dj", cascadeCreate: true));
            //client.Send(new AddPurchase("ljsfjd3231", "88dj", cascadeCreate: true));

            RecommendationResponse recommended = client.Send(new RecommendItemsToUser("ifijj", 5, 
                returnProperties: true, cascadeCreate: true));
            List<object> itemList = new List<object>();
            
            // Iterating over recommendations:
            foreach (Recommendation r in recommended.Recomms)
            {
                itemList.Add(new
                {
                    id = r.Id,
                    name = r.Values.GetValueOrDefault("name"),
                    category = r.Values.GetValueOrDefault("category")
                });
            }
            return Ok(itemList);
        }

        [HttpPost("AddUserRecommendation")]
        public IActionResult AddUserRecommendation([FromForm] IFormCollection data)
        {
            var client = new RecombeeClient("cscassignment-dev", "");

            //client.Send(new AddUserProperty("name", "string"));
            //client.Send(new AddUserProperty("age", "int"));
            //client.Send(new AddUserProperty("language", "string"));
            //client.Send(new AddUserProperty("country", "string"));
            //client.Send(new AddUserProperty("sex", "string"));
            //client.Send(new AddUserProperty("reknown", "string"));
            //client.Send(new AddUserProperty("bio", "string"));
            //client.Send(new AddUserProperty("image", "image"));

            string userId = data["id"];
            client.Send(new AddUser(userId));

            client.Send(new SetUserValues(userId,
                new Dictionary<string, object>() {
                    {"name", data["name"]},
                    {"age", int.Parse(data["age"])},
                    {"language", new string[] {"english", "chinese" }},
                    {"country", data["country"]},
                    {"sex", data["sex"]},
                    {"reknown", data["reknown"]},
                    {"bio", data["bio"]},
                    {"image", "http://examplesite.com/products/xyz.jpg"},
                },
                cascadeCreate: true
            ));

            return Ok(new { message = "Recommendation successfully added" });

        }

        [HttpPost("AddItemRecommendation")]
        public IActionResult AddItemRecommendation([FromForm] IFormCollection data)
        {
            var client = new RecombeeClient("cscassignment-dev", "");
            try
            {
                string itemId = data["id"];
                client.Send(new AddItem(itemId));

                client.Send(new SetItemValues(itemId,
                    new Dictionary<string, object>() {
                    {"name", data["name"]},
                    {"category", data["category"]},
                    {"image", "http://examplesite.com/products/xyz.jpg"},
                    },
                    cascadeCreate: true
                ));
            } catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }


            return Ok(new { message = "Recommendation successfully added" });

        }
    }
}
