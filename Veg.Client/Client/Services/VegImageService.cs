using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veg.Client.Services
{
    public class VegImageService
    {
        private VegConfiguration _vegConfiguration;

        public VegImageService(VegConfiguration vegConfiguration)
        {
            _vegConfiguration = vegConfiguration;
        }


        public string GetImagePath(string imageSize, string imageName)
        {
            return _vegConfiguration.GetAPIBasePath() + @"imagestore/" + imageSize + "/" + (imageName != null ? imageName : "NoImage.png");
        }
    }
}
