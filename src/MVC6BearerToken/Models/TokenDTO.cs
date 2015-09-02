using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVC6BearerToken.Models
{
    public class TokenDTO
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public string ClientId { get; set; }
        public string UserName { get; set; }
        public string Expires { get; set; }
        public string Issued { get; set; }
        public string RefreshToken { get; set; }

    }
}
