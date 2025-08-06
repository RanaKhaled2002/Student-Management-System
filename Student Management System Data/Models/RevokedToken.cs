using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Student_Management_System_Data.Models
{
    public class RevokedToken : BaseClass
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
