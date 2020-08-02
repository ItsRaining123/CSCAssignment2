using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSCAssignment2.Models;
using Microsoft.AspNetCore.Mvc;
using CSCAssignment2.Helpers;
using CSCAssignment2.Services;
using Microsoft.Extensions.Options;
using ExamScriptTS.Models;
using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using System.Configuration;
using Amazon.S3.Model;
using Amazon.Runtime;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Clarifai.API;	
using Clarifai.DTOs.Inputs;	
using Clarifai.DTOs.Predictions;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSCAssignment2.APIs
{
    [Route("api/[controller]")]
    [ApiController]
    public class TalentsController : ControllerBase
    {
        private readonly CSCAssignment2DbContext _context;
        private IUserService _userService;
        private CSCAssignment2DbContext _database;
        private AppSettings _appSettings;


        public TalentsController(CSCAssignment2DbContext context, IUserService userService, CSCAssignment2DbContext database, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _userService = userService;
            _database = database;
            _appSettings = appSettings.Value;
        }

        // GET api/<TalentsController>/5
        [HttpGet]
        public IActionResult Get()
        {
            List<Talents> talents = new List<Talents>();
            List<object> talentsList = new List<object>();


            talents = _database.Talents.ToList();

            try
            {
                if (talents == null)
                {
                    return NotFound(new { message = "Talents record(s) are missing." });
                }
                else
                {
                    using (var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
                    {
                        foreach (var talentData in talents)
                        {
                            talentsList.Add(new
                            {
                                talentBio = talentData.Bio,
                                talentId = talentData.Id,
                                talentName = talentData.TalentName,
                                talentReknown = talentData.Reknown,
                                talentshortName = talentData.Shortname,
                                talentUrl = talentData.TalentImageURL,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            return Ok(talentsList);
        }

        // GET API/Talents/GetTalent/5
        [HttpGet("GetTalent/{id}")]
        public IActionResult GetTalent(int id)
        {
            var oneTalent = _database.Talents.Where(t => t.Id == id)
                .SingleOrDefault();

            if (oneTalent == null)
            {
                return BadRequest(new { message = "Unable to retrieve record" });
            }
            else
            {
                var Result = new
                {
                    name = oneTalent.TalentName,
                    shortName = oneTalent.Shortname,
                    bio = oneTalent.Bio,
                    reknown = oneTalent.Reknown
                };

                return Ok(Result);
            }
        }

        // POST API/Talents/TalentCreate
        [HttpPost("TalentCreate")]
        public IActionResult TalentCreate([FromForm] IFormCollection data)
        {
            //Create an object User type object, user
            Talents talent = new Talents();
            object response = null;

            try
            {
                //Start passing the collected data into the new User object.
                talent.TalentName = data["name"];
                talent.Shortname = data["shortName"];
                talent.Bio = data["bio"];
                talent.Reknown = data["reknown"];
                talent.TalentImageURL = "";

                if (talent.TalentName == "")
                {
                    response = new { status = "fail", message = "Talent Name is required" };
                    return BadRequest(response);
                }
                else if (talent.Shortname == "")
                {
                    response = new { status = "fail", message = "Short Name is required" };
                    return BadRequest(response);
                }
                else if (talent.Bio == "")
                {
                    response = new { status = "fail", message = "Bio is required" };
                    return BadRequest(response);
                }
                else if (talent.Reknown == "")
                {
                    response = new { status = "fail", message = "Reknown is required" };
                    return BadRequest(response);
                }
                else
                {
                    _database.Talents.Add(talent);
                    _database.SaveChanges();
                }
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
            //Send back an OK with 200 status code
            return Ok(new
            {
                message = "Successfully Created Talent"
            });
        }//End of post web api

        // PUT API/Talents/UpdateTalent/5
        [HttpPut("UpdateTalent/{id}")]
        public IActionResult UpdateTalent(int id, [FromForm] IFormCollection data)
        {
            //Create an object Users type object, user
            Talents talent = _database.Talents
                .SingleOrDefault(u => u.Id == id);

            if (talent != null)
            {
                //Start passing the collected data
                talent.TalentName = data["name"];
                talent.Shortname = data["shortName"];
                talent.Bio = data["bio"];
                talent.Reknown = data["reknown"];
            }
            else
            {
                return NotFound(new { message = "Unable to update the talent's information." });
            }
            try
            {
                _database.Talents.Update(talent);
                _database.SaveChanges();
            }
            catch (Exception ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
            //Send back an OK with 200 status code
            return Ok(new
            {
                message = "Updated talent record"
            });

        }//End of PUT Web API method

        // DELETE API/Talents/DeleteTalent/5
        [HttpDelete("DeleteTalent/{id}")]
        public IActionResult DeleteTalent(int id)
        {
            string customMessage = "";

            try
            {
                var oneTalent = _database.Talents
               .SingleOrDefault(t => t.Id == id);
                //Call the remove method, pass the oneUser object into it
                //so that the Database object knows what to remove from the database.
                _database.Talents.Remove(oneTalent);
                //Tell the db model to commit/persist the changes to the database, 
                _database.SaveChanges();
            }
            catch (Exception ex)
            {
                customMessage = "Unable to delete user record.";
                return BadRequest(new { message = customMessage });
            }//End of try .. catch block on manage data

            return Ok(new { message = "Deleted user record" });
        }//end of DELETE Web API method

        [HttpPost("UploadFile/{id}")]
        public async Task<IActionResult> UploadFileToS3(int id, IFormFile photo)
        {
            var information = _database.Talents.Where(a => a.Id == id).FirstOrDefault();
            var url = "";
            using (var client = new AmazonS3Client(Amazon.RegionEndpoint.USEast1))
            {
                
                try
                {
                    using (var newMemoryStream = new MemoryStream())
                    {
                        photo.CopyTo(newMemoryStream);

                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = newMemoryStream,
                            Key = photo.FileName,
                            BucketName = "testbucketcscnew1",
                            CannedACL = S3CannedACL.PublicRead
                        };

                        GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
                        {
                            BucketName = "testbucketcscnew1",
                            Key = photo.FileName,
                            Expires = DateTime.Now.AddMinutes(5)
                        };


                        var fileTransferUtility = new TransferUtility(client);
                        await fileTransferUtility.UploadAsync(uploadRequest);
                        url = client.GetPreSignedURL(request1);
                    }

                    information.TalentImageURL = photo.FileName;
                    
                    //Check for NSFW image using Clarifai
                    var clientClarifai = new ClarifaiClient("1e765fbf24954a2b909d0259e0087945");
                    var response = await clientClarifai.PublicModels.NsfwModel
                        .Predict(new ClarifaiURLImage("https://testbucketcscnew1.s3.amazonaws.com/" + photo.FileName))
                        .ExecuteAsync();

                    double concepts = 0;
                    concepts = double.Parse(response.Get().Data.Select(c => $"{c.Value}").LastOrDefault());
                    //var concepts1 = response.Get().Data.Select(c => $"{c.Name}: {c.Value}");

                    if (concepts >= 0.85) //return BadRequest if image has >85% rate of being NSFW
                    {
                        return BadRequest(new { message = "The image you uploaded is Not Safe For Work (NSFW)" });
                    } else
                    {
                        _database.Talents.Update(information);
                        _database.SaveChanges();
                    } 

                }
                catch (Exception ex)
                {
                   return BadRequest(new { message = ex.Message });
                }
            }
            return Ok(new {message =  url});
        }
    }
}
