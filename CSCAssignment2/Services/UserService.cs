using System;
using System.Collections.Generic;
using System.Linq;
using CSCAssignment2.Models;

using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using CSCAssignment2.Helpers;
using ExamScriptTS.Models;

namespace CSCAssignment2.Services
{
    public interface IUserService
    {
        Task<Users> AuthenticateAsync(string inUserId, string inPassword);
        Task<List<Users>> GetAllUsersAsync();
        Task<List<Users>> GetAllUsersByRoleIdAsync(int roleId);
        Task<Users> GetUserByIdAsync(int id);
        Task<int> UpdateAsync(Users user, string password = null);
        Task<Users> CreateAsync(Users user, string password);
        Task<int> DeleteAsync(int id);
    }

    public class UserService : IUserService
    {
        private IAppDateTimeService _appDateTimeService;
        public CSCAssignment2DbContext Database { get; }

        public UserService(CSCAssignment2DbContext context,
        IAppDateTimeService appDateTimeService)
        {//Intialize the Database property and the member variable(s)
            Database = context;
            _appDateTimeService = appDateTimeService;
        }
        public async Task<int> UpdateAsync(Users user, string password = null)
        {
            int result = 0;
            Users tempUser = new Users();
            tempUser = await Database.Users.FindAsync(user.Id);
            if (tempUser == null)
            {
                throw new AppException("User not found");
            }
            if (user.Username != user.Username)
            {
                // username has changed so check if the new username is already taken
                if (await Database.Users.AnyAsync(x => x.Username == user.Username))
                {
                    throw new AppException("User name " + user.Username + " is already taken");
                }
            }
            //Update user properties
            tempUser.FullName = user.FullName;
            tempUser.Username = user.Username;
            tempUser.RoleId= user.RoleId;
            //tempUser.UpdatedAt = _appDateTimeService.GetCurrentDateTime();
            //Update password if it was entered
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                tempUser.PasswordHash = passwordHash;
                tempUser.PasswordSalt = passwordSalt;
            }
            try
            {
                Database.Users.Update(tempUser);
                result = await Database.SaveChangesAsync();
                return result;
            }
            catch
            {
                throw new AppException("Update operation encountered error.");
            }
        }

        public async Task<Users> AuthenticateAsync(string inUsername, string inPassword)
        {
            if (string.IsNullOrEmpty(inUsername) || string.IsNullOrEmpty(inPassword))
                return null;
            //Check whether there is a matching user name information first.
            //Then, the subsequent code will verify the password by calling
            //the VefiryPasswordHash method
            var user = await Database.Users.Include(u => u.Role).SingleOrDefaultAsync(x => x.Username == inUsername);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(inPassword, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await Database.Users.Include(user => user.Role).ToListAsync();
        }
        public async Task<List<Users>> GetAllUsersByRoleIdAsync(int roleId)
        {
            return await Database.Users.Where(user => user.RoleId == roleId)
            .Include(user => user.Role)
            .AsNoTracking()
            .ToListAsync();
        }
        public async Task<Users> GetUserByIdAsync(int id)
        {
            return await Database.Users.FindAsync(id);
        }

        public async Task<Users> CreateAsync(Users user, string password)
        {
            // validation to check if the password is empty or spaces only.
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");
            // If the user name (email) already exists, raise an exception
            // so that the Web API controller class code can capture the error and
            // send back a JSON response to the client side.
            if (await Database.Users.AnyAsync(appUser => appUser.Username == user.Username))
                throw new AppException("Username " + user.Username + " is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            //user.CreatedAt = _appDateTimeService.GetCurrentDateTime();
            //user.UpdatedAt = _appDateTimeService.GetCurrentDateTime();
            Database.Users.Add(user);
            await Database.SaveChangesAsync();
            return user;
        }



        public async Task<int> DeleteAsync(int id)
        {
            int result = 0;
            var user = await Database.Users.FindAsync(id);
            if (user != null)
            {
                try
                {
                    Database.Users.Remove(user);
                    result = await Database.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    if (ex.InnerException.Message.ToUpper().Contains("REFERENCE CONSTRAINT"))
                    {
                        throw new AppException("Unable to delete user record. The user information might have been linked to other information. ");
                    }
                    else
                    {
                        throw new AppException("Unable to delete user record.");
                    }
                }
            }
            return result;
        }

        // private helper methods

        private static void CreatePasswordHash(string inPassword, out byte[] inPasswordHash, out byte[] inPasswordSalt)
        {
            if (inPassword == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(inPassword)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            //The password is hashed with a new random salt.
            //https://crackstation.net/hashing-security.htm
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                inPasswordSalt = hmac.Key;
                inPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inPassword));
            }
        }

        private static bool VerifyPasswordHash(string inPassword, byte[] inStoredHash, byte[] inStoredSalt)
        {
            if (inPassword == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(inPassword)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (inStoredHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (inStoredSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(inStoredSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inPassword));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != inStoredHash[i]) return false;
                }
            }

            return true;
        }


    }
}