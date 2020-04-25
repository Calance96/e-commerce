using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Models.DTO
{
    public class AuthResult
    {
        public ApplicationUser ApplicationUser { get; set; }
        public int StatusCode { get; set; }
        public List<string> Message { get; set; }
    }
}
