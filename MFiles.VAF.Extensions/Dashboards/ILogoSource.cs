using MFiles.VAF.Common;
using MFiles.VAF.Configuration.Domain.Dashboards;
using MFiles.VAF.Extensions.Dashboards;
using MFiles.VAF.Extensions.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MFiles.VAF.Extensions.Dashboards
{
    public interface ILogoSource
		: IDashboardContent
    {
    }
    public class SvgLogoSource
        : LogoSourceBase
    {
        private SvgLogoSource()
        {
        }
        public static SvgLogoSource FromXmlString(string xmlString)
            => new SvgLogoSource()
            {
                ImageUriString = $"data:image/svg+xml;base64,{Convert.ToBase64String(xmlString.ToBytes(Encoding.UTF8))}"
            };
    }
    public abstract class LogoSourceBase
        : DashboardCustomContentEx, ILogoSource
    {
        public const int DefaultHeightInPixels = 100;
        public const int DefaultWidthInPixels = 200;

        protected LogoSourceBase()
			: base("<div></div>")
        {
            this.Styles.Add("height", DefaultHeightInPixels + "px");
            this.Styles.Add("width", DefaultWidthInPixels + "px");
            this.Styles.Add("background-size", "contain");
            this.Styles.Add("background-repeat", "no-repeat");
            this.Styles.Add("background-position", "center top");
        }
		private string imageUriString;
		public string ImageUriString
		{
			get => this.imageUriString;
			set
			{
				this.imageUriString = value;
				if (string.IsNullOrWhiteSpace(value))
					this.Styles.Remove("background-image");
				else
					this.Styles.AddOrUpdate("background-image", $"url('{value}')");
			}
		}
    }
}
