using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FYP_Customer_Behavior_.Models.Dtos
{
    public class CustomerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string gender { get; set; }
        public string contact { get; set; }
        public string secondName { get; set; }

    }
}