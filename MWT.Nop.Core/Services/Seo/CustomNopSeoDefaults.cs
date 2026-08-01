using Nop.Core.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Nop.Core.Services.Seo
{ 
    public static class CustomNopSeoDefaults
    {
        public static CacheKey UrlRecordByTermSlugCacheKey => new CacheKey("Nop.urlrecord.Term.byslug.{0}");

        public static CacheKey UrlRecordByQuestionAnswerSlugCacheKey => new CacheKey("Nop.urlrecord.QuestionAnswer.byslug.{0}");

        public static CacheKey UrlRecordByLandingPageSlugCacheKey => new CacheKey("Nop.urlrecord.LandingPage.byslug.{0}");
    }
}
