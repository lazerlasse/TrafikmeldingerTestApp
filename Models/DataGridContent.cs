using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrafikmeldingerTestApp
{
	public class DataGridContent
	{
		public string ValidityState { get; set; }
		public string Version { get; set; }
		public DateTime CreatedTime { get; set; }
		public DateTime VersionTime { get; set; }
		public string SituationText { get; set; }
		public string LocationLongitude { get; set; }
		public string LocationLatitude { get; set; }
	}
}
