using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Customizations.Mandrill
{
    public class Mandril_Email_Request_Format
    {
        public string key { get; set; }
        public Mandrill_Message_Format message { get; set; }
    }
    public class Mandrill_Message_Format
    {
        public string html { get; set; }
        public string subject { get; set; }
        public string from_email { get; set; }
        public string from_name { get; set; }
        public Mandrill_Message_To[] to { get; set; }
        public Mandrill_Message_headers headers { get; set; }
        public bool important { get { return true; } }
        public bool track_opens { get { return true; } }
        public bool track_clicks { get { return true; } }
        public bool auto_text { get { return true; } }
        public bool merge { get { return true; } }
        public string merge_language { get { return "mailchimp"; } }
        public global_merge_vars[] global_merge_vars { get; set; }

    }
    public class Mandrill_Message_To
    {
        public string email { get; set; }
        public string name { get; set; }
        public string type { get { return "to"; } }
    }
    public class Mandrill_Message_headers
    {
        [JsonProperty(propertyName: "Reply-To")]
        public string ReplyTo { get; set; }
    }
    public class global_merge_vars
    {
        public string name { get; set; }
        public string content { get; set; }
    }
}
