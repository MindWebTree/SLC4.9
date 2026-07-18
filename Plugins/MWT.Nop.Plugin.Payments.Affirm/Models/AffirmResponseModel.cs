using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Plugin.Payments.Affirm.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class _329772760
    {
        public string display_name { get; set; }
        public string item_type { get; set; }
        public int qty { get; set; }
        public string sku { get; set; }
        public int unit_price { get; set; }
    }

    public class _329777085
    {
        public string display_name { get; set; }
        public string item_type { get; set; }
        public int qty { get; set; }
        public string sku { get; set; }
        public int unit_price { get; set; }
    }

    public class Address
    {
        public string city { get; set; }
        public string line1 { get; set; }
        public string state { get; set; }
        public string zipcode { get; set; }
        public string line2 { get; set; }
        public string country { get; set; }
    }

    public class AffirmGoV3
    {
    }

    public class Billing
    {
        public string phone_number { get; set; }
        public string email { get; set; }
        public Name name { get; set; }
        public Address address { get; set; }
    }

    public class Classic
    {
        public string financing_program_v2_uuid { get; set; }
    }

    public class Config
    {
        public string user_confirmation_url_action { get; set; }
    }

    public class FinancingProgramMap
    {
        public AffirmGoV3 affirm_go_v3 { get; set; }
        public Classic classic { get; set; }
    }


    public class Merchant
    {
        public string name { get; set; }
        public string public_api_key { get; set; }
        public string user_cancel_url { get; set; }
        public string user_confirmation_url { get; set; }
        public string user_confirmation_url_action { get; set; }
    }

    public class Meta
    {
        public string __affirm_tracking_uuid { get; set; }
        public string customerid { get; set; }
        public string locale { get; set; }
        public string release { get; set; }
        public string tempOrderId { get; set; }
        public string user_timezone { get; set; }
        public string orderGuid { get;set; }
    }

    public class Metadata
    {
        public string checkout_channel_type { get; set; }
    }

    public class Name
    {
        public string full { get; set; }
        public string first { get; set; }
        public string last { get; set; }
    }

    public class AffirmResponseModel
    {
        public string api_version { get; set; }
        public Billing billing { get; set; }
        public string checkout_flow_type { get; set; }
        public string checkout_type { get; set; }
        public Config config { get; set; }
        public string currency { get; set; }
        public FinancingProgramMap financing_program_map { get; set; }
        public string financing_program_v2_uuid { get; set; }
        public string loan_type { get; set; }
        public Merchant merchant { get; set; }
        public Meta meta { get; set; }
        public Metadata metadata { get; set; }
        public string product_type { get; set; }
        public Shipping shipping { get; set; }
        public int shipping_amount { get; set; }
        public bool suppress_expiration_declination_messaging { get; set; }
        public int tax_amount { get; set; }
        public int total { get; set; }

        public string order_id { get; set; }
        public string product { get; set; }
        public string checkout_status { get; set; }
    }

    public class Shipping
    {
        public string phone_number { get; set; }
        public string email { get; set; }
        public Name name { get; set; }
        public Address address { get; set; }
    }

    public enum AffirmOrderStatus
    {
        confirmed=0
    }
}
