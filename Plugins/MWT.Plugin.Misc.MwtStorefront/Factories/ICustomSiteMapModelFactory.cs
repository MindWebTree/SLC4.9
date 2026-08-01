using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories
{
    public interface ICustomSiteMapModelFactory : ISitemapModelFactory 
    {
        Task<string> CustomGenerateAsync(int? id);
        Task<string> CustomKwTermGenerateAsync(int? id);

        Task<string> CustomQuestionAnswerGenerateAsync(int? id);
        Task<string> CustomGeneratePictureSiteMapAsync(int? id);


        #region Version2

        Task<string> CustomGenerateSitemapIndexXml();
        Task<string> CustomGenerateProductSitemapXml();
        Task<string> CustomGeneratePageSitemapXml();
        Task<string> CustomGenerateCategorySitemapXml();
        Task<string> CustomGenerateQuestionAnswerSitemapXml();
        Task<string> CustomGenerateKwTermSitemapXml();

        #endregion
    }
}
