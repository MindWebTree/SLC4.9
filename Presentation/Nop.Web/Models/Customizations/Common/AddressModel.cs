using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Models.Common;

public partial record AddressModel 
{
    public string Abbreviation { get; set; }
    public string CountryTwoLetterSeoCode { get; set; }
    public string StateAbbreviation { get; set; }

    [NopResourceDisplayName("Address.Fields.FullName")]

    public string FullName { get; set; }
}