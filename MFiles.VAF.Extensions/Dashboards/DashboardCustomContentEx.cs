using MFiles.VAF.Configuration.Domain.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MFiles.VAF.Extensions.Dashboards
{
	public class DashboardCustomContentEx
		: DashboardContentBase
	{
		public IDashboardContent InnerContent { get; set; }
		public DashboardCustomContentEx(IDashboardContent innerContent)
		{
			this.InnerContent = innerContent;
		}
		public DashboardCustomContentEx(string htmlContent)
		{
			this.InnerContent = new DashboardCustomContent(htmlContent);
		}
		protected override XmlDocumentFragment GenerateXmlDocumentFragment(XmlDocument xml)
		{
			if (null == this.InnerContent)
				return null;
			return this.InnerContent.Generate(xml);
		}
	}
}
