
using MWT.Plugin.Misc.MwtStorefront.Models.Topics;
using Nop.Core.Domain.Topics;
using Nop.Web.Factories;

namespace MWT.Plugin.Misc.MwtStorefront.Factories.Topics
{
    public partial interface ICustomTopicModelFactory: ITopicModelFactory
    {
        Task<CustomTopicModel> CustomPrepareTopicModelByIdAsync(Topic topic);
    }
}
