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
                            //Dennis: testbucketcscnew1
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

                    if (concepts >= 0.85)
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