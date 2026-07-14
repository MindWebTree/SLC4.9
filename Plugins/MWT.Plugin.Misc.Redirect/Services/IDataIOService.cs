using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Plugin.Misc.Redirect.Domain;
using Nop.Plugin.Misc.Redirect.Enums;
using Nop.Plugin.Misc.Redirect.Models.Redirections;

namespace Nop.Plugin.Misc.Redirect.Services
{
    public interface IDataIOService
    {
		Task<List<(int line, InsertRedirectionResult result)>> Import(string csvText);

		Task<string> Export();
	}
}