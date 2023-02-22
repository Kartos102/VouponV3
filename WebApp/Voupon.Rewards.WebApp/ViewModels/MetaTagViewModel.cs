using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels
{
    public class MetaTagViewModel
    {
        public string Title { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public string OGTitle { get; set; }
        public string OGDescription { get; set; }
        public string OGUrl { get; set; }
        public string OGImageUrl { get; set; }
        public string OGImageWidth { get; set; }
        public string OGImageHeight { get; set; }
        public string OGType { get; set; }
        public string FBAppId { get; set; }
    }
}
