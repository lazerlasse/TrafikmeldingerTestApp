using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace TrafikmeldingerTestApp
{
	public class SituationSearch
	{
		private readonly string URLString = "https://data.vd-nap.dk/secure/api/v1/nap/data?profile=VD.TrafficEvents.Aggregated&auth=eyJhbGciOiJSUzUxMiJ9.eyJjdXMiOiJuYXAiLCJzdWIiOiI2NjEiLCJkb20iOiJ1c2VycyIsImNyZWF0ZWQiOjE2Mjc4NTc3Mzc3NjIsInByb2ZpbGUiOiJWRC5UcmFmZmljRXZlbnRzLkFnZ3JlZ2F0ZWQiLCJzdWJzY3JpcHRpb25JZCI6IjYwNDU4NjkwIiwiZXhwIjoxOTQzMjE3NzM3LCJyb2wiOlsiVkROQVBfQVBJQUNDRVNTIl0sInRpZCI6IjQzOCJ9.Cir7y4C4xdpV8yDSgT97CqyoC_UXYQRD9JcVTK2234w6hP1_Hn7XCJwHjeUt36ThXJfLGm6me2eXtoUXJRXuh7ANj4_VCabtMstGrRrY1bcDk0dQhyYF5ma7u7qCZyy_ayqynSQ3BYdzhh0NtvsdSfFdv1In4jRG9djNhVJYOLDTXPQEnQOr9ZAxU-Hkv1nLjQGJfaxCS9099slmzgLM2wxn6_z3-zVqjbrEu9tclGeWFsARB_ts3nztQVD6_tXktWfWoBO6MOwTFZ2xA_aOZEleUEosWJcYAd2S8caGnqdmewAPSWUgQFigr1jx1RlrUSISvIDm8wpalhqMNMu5bw";

		public SituationSearch()
		{
		}

		public List<DataGridContent> SearchForNewSituations(List<DataGridContent> currentContents)
		{
			DataGridContent newSituationContent;
			XmlDocument xDocument = new();
			List<DataGridContent> newSituationsList = new();

			try
			{
				xDocument.Load(URLString);
			}
			catch (Exception)
			{
				return currentContents;
			}

			XmlNamespaceManager nsmgr = new(xDocument.NameTable);
			nsmgr.AddNamespace("ns", "http://datex2.eu/schema/2/2_0");

			XmlNodeList situationList = xDocument.SelectNodes("//ns:situationRecord", nsmgr);

			foreach (XmlNode situation in situationList)
			{
				newSituationContent = new();

				if (situation.SelectSingleNode(".//ns:groupOfLocations/ns:locationForDisplay", nsmgr) != null)
				{
					newSituationContent.LocationLongitude = situation.SelectSingleNode(".//ns:groupOfLocations/ns:locationForDisplay/ns:longitude", nsmgr).InnerText;
					newSituationContent.LocationLatitude = situation.SelectSingleNode(".//ns:groupOfLocations/ns:locationForDisplay/ns:latitude", nsmgr).InnerText;
				}

				if (DateTime.TryParse(situation["situationRecordCreationTime"].InnerText, out DateTime parsedCreatedTime))
				{
					newSituationContent.CreatedTime = parsedCreatedTime.ToLocalTime();
				}

				if (DateTime.TryParse(situation["situationRecordVersionTime"].InnerText, out DateTime parsedVersionTime))
				{
					newSituationContent.VersionTime = parsedCreatedTime.ToLocalTime();
				}

				newSituationContent.Version = situation.SelectSingleNode(".//@version").Value;
				newSituationContent.ValidityState = situation.SelectSingleNode(".//ns:validity/ns:validityStatus", nsmgr).InnerText;
				newSituationContent.SituationText = situation["generalPublicComment"].InnerText;

				newSituationsList.Add(newSituationContent);
			};

			// Validate and update new situations in the list.
			newSituationsList.Reverse();
			foreach (DataGridContent newSituation in newSituationsList)
			{
				if (currentContents.Exists(v => v.Version == newSituation.Version))
				{
					if (currentContents.Find(v => v.Version == newSituation.Version).VersionTime < newSituation.VersionTime)
					{
						currentContents.Remove(currentContents.Find(v => v.Version == newSituation.Version));
						currentContents.Add(newSituation);
					}
					if (currentContents.Find(v => v.Version == newSituation.Version).ValidityState != newSituation.ValidityState)
					{
						currentContents.Remove(currentContents.Find(v => v.Version == newSituation.Version));
						currentContents.Add(newSituation);
					}
				}
				else
				{
					if (newSituation.ValidityState != "suspended")
					{
						currentContents.Add(newSituation);
					}
				}
			}

			return currentContents;
		}

		public DateTime? GetSituationVersion()
		{
			// Create new XML document for XML data.
			XmlDocument xmlDocument = new();

			// Try load the datastream into the XML document.
			try
			{
				xmlDocument.Load(URLString);	// Try load data.
			}
			catch (Exception)
			{
				return null;	// return string error.
			}

			// Create namespace manager for navigating the xml data.
			XmlNamespaceManager nsmgr = new(xmlDocument.NameTable);
			nsmgr.AddNamespace("ns", "http://datex2.eu/schema/2/2_0");

			// Simplyfied if/else return statement.
			return DateTime.TryParse(xmlDocument.SelectSingleNode("//ns:situation/ns:situationVersionTime", nsmgr).InnerText, out DateTime situationVersionTime)
				? situationVersionTime
				: null;
		}
	}
}