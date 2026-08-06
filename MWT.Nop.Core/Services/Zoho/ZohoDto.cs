using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Zoho
{
    public partial class ZohoDto
    {
        public string APIException { get; set; } // Readonly
        public string GALeadID { get; set; }
        public string Company { get; set; }
        public string LeadSource { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public string SKU { get; set; }
        public string ProductName { get; set; }
        public string URL { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string IPAddress { get; set; }
        public string Address { get; set; }
        public string Attachments { get; set; }
        public string LeadStatus { get; set; }
        public decimal Total { get; set; }
        public Customer customer { get; set; }
    }
}
