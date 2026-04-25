using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.Users
{
    public class CreateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}