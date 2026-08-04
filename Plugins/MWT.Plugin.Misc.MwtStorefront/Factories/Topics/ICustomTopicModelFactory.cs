 
using MWT.Plugin.Misc.MwtStorefront.Models.Topics;
using Nop.Core.Domain.Topics;
using Nop.Web.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Topics
{
    public partial interface ICustomTopicModelFactory: ITopicModelFactory
    {
        Task<CustomTopicModel> CustomPrepareTopicModelByIdAsync(Topic topic);
    }
}
