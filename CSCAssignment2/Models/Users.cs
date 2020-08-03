using System;
using System.Collections.Generic;

namespace CSCAssignment2.Models
{
    public partial class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public int RoleId{ get; set; }
        public Approles Role { get; set; }
        public byte[] PasswordSalt { get; set; }
        public byte[] PasswordHash { get; set; }
        public string CustomerId { get; set; }
    }
}