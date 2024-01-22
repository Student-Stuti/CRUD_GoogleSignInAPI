using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DTO
{
    public class ResponseDto
    {
        public string Token { get; set; }
        public DateTime Expiry { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public bool IsAuthSuccessful { get; set; }
        public int Code { get; set; }

        public List<string> Roles { get; set; }
    }
}
