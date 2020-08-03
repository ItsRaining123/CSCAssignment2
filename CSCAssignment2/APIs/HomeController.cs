using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSCAssignment2.Models;
using CSCAssignment2.Helpers;
using CSCAssignment2.Services;
using Microsoft.Extensions.Options;
using ExamScriptTS.Models;
using Stripe;

namespace CSCAssignment2.APIs
{

    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        private readonly CSCAssignment2DbContext _context;
        private IUserService _userService;
        private CSCAssignment2DbContext _database;
        private AppSettings _appSettings;

        public HomeController(CSCAssignment2DbContext context, IUserService userService,
             CSCAssignment2DbContext database,
            IOptions<AppSettings> appSettings)
        {
            _context = context;
            _userService = userService;
            _database = database;
            _appSettings = appSettings.Value;
        }

        string key = "sk_test_51GxdLDHq05oyY0YBoHTN18NJHgarUMDCNAHpcBgYhBLseyoKXCOwtB9DtBxRlWJhnCaw1DBZ6QVvCme5g07hcVfP00VqfSJeKC"; // input Stripe API Key here

        // GET: api/Home
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // Post api/Home/CreateUser
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUserAsync([FromForm] IFormCollection data)
        {
            //Create an object AppUser type object, user
            Users user = new Users();

            //Start passing the collected data into the new AppUser object.
            user.FullName = data["fullName"];
            user.Username = data["userid"];
            user.RoleId = int.Parse(data["roleId"]);
            user.CustomerId = "";
            try
            {
                await _userService.CreateAsync(user, data["password"]);
            }
            catch (AppException ex)
            {
                // return error message if there was an exception
                return BadRequest(new { message = ex.Message });
            }
            //Send back an OK with 200 status code
            return Ok(new
            {
                message = "Saved user record"
            });
        }//End of CreateUserAsync web api

        //GET api/Home/GetRoles
        [HttpGet("GetRoles")]
        public IActionResult GetRoles()
        {
            List<Approles> users = new List<Approles>();
            try
            {
                users = _database.Approles.ToList();
            }
            catch (Exception ex)
            {
                //Return error message if there was an exception
                return BadRequest(new { message = "Unable to retrieve role records." });
            }
            //Send back an OK with 200 status code
            return Ok(new
            {
                records = users
            });
        }//End of GetRolesForInputControls web api


        // GET: api/Home/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // PUT: api/Home/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/AccountCreation
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            _context.Users.Add(users);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsers", new { id = users.Id }, users);
        }

        // DELETE: api/Home/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Users>> DeleteUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            _context.Users.Remove(users);
            await _context.SaveChangesAsync();

            return users;
        }

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
