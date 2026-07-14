using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Vml.Office;
using LinqToDB;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure.Mapper;
using Nop.Data;
using Nop.Plugin.Misc.Redirect.Domain;
using Nop.Plugin.Misc.Redirect.Enums;
using Nop.Plugin.Misc.Redirect.Extensions;
using Nop.Plugin.Misc.Redirect.Models;
using Nop.Plugin.Misc.Redirect.Models.Redirections;
using Nop.Plugin.Misc.Redirect.Rules;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.Redirect.Services
{
	public class DataIOService : IDataIOService
	{
		private readonly IRepository<RedirectionRule> _redirectionRuleEntityRepository;
		private readonly IRedirectionsService _service;
		private readonly IStoreContext _storeContext; 

		private readonly CsvConfiguration _csvConf = new CsvConfiguration(CultureInfo.InvariantCulture);
		

		public DataIOService(IRedirectionsService service, IStoreContext storeContext, IRepository<RedirectionRule> redirectionRuleEntityRepository)
		{
			_service = service;
			_storeContext = storeContext;
			_redirectionRuleEntityRepository = redirectionRuleEntityRepository;
			_csvConf.Delimiter = ";"; 
        }

		public async Task<string> Export()
		{
			var store = await _storeContext.GetCurrentStoreAsync();
			var data = await _redirectionRuleEntityRepository.Table.Where(r => r.StoreId == store.Id).ToListAsync();
            var csvModels = data
           .Select(r => AutoMapperConfiguration.Mapper.Map<RedirectionCSVModel>(r))
           .ToList();
            return await csvModels.ToCsvAsync(_csvConf);
		}

		public async Task<List<(int line, InsertRedirectionResult result)>> Import(string csvText)
		{
			List<(int line, InsertRedirectionResult result)> errors = new List<(int line, InsertRedirectionResult result)>();
			IEnumerable<RedirectionCSVModel> records;
			using (var reader = new StringReader(csvText))
			using (var csvReader = new CsvReader(reader, _csvConf,false))
				records = await csvReader.GetRecordsAsync<RedirectionCSVModel>().ToListAsync();

            var redirections = records
        .Select(r => AutoMapperConfiguration.Mapper.Map<RedirectionRule>(r))
        .ToArray();
            for (int x =0; x < redirections.Length; x++)
			{
				var result = await _service.InsertRedirectionsAsync(redirections[x]);
				if (result != InsertRedirectionResult.OK)
					errors.Add((x, result));
			}
			
			return errors;
		}
	}
}
