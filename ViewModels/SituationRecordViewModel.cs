using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using TrafikmeldingerTestApp.Delegates;
using TrafikmeldingerTestApp.Models;
using TrafikmeldingerTestApp.Services;
using TrafikmeldingerTestApp.Views;

namespace TrafikmeldingerTestApp.ViewModels
{
	public class SituationRecordViewModel : BaseViewModel
	{
		// Private properties...
		private ObservableCollection<DataGridContent> dataGridContents;
		private ObservableCollection<PushoverLogModel> pushoverMessageLog;
		private string statusText;
		private DateTime? situationVersionTime;
		private int searchLoopCounter;

		// Observerble collection for DataGrid content.
		public ObservableCollection<DataGridContent> DataGridContents
		{
			get
			{
				return dataGridContents;
			}
			set
			{
				dataGridContents = value;
			}
		}

		// Public property for pushover messsage log ObservableCollection.
		public ObservableCollection<PushoverLogModel> PushoverMessageLog
		{
			get
			{
				return pushoverMessageLog;
			}
			set
			{
				pushoverMessageLog = value;
			}
		}

		// Public property for StartButton text.
		public string StatusText
		{
			get
			{
				return statusText;
			}
			set
			{
				statusText = value;
				OnPropertyChanged(nameof(StatusText)); // PropertyChanged handler.
			}
		}

		// Public property for situation version time.
		public DateTime? SituationVersionTime
		{
			get
			{
				return situationVersionTime;
			}
			set
			{
				situationVersionTime = value;
				OnPropertyChanged(nameof(SituationVersionTime));
			}
		}

		// Public property for search loop count.
		public int SearchLoopCounter
		{
			get
			{
				return searchLoopCounter;
			}
			set
			{
				searchLoopCounter = value;
				OnPropertyChanged(nameof(SearchLoopCounter));
			}
		}

		// Public constructor for this ViewModel.
		public SituationRecordViewModel()
		{
			StatusText = "Indlæser";
			SearchLoopCounter = 0;
			DataGridContents = new();
			PushoverMessageLog = new();
			Run();
		}

		// Method for running the search for new situations.
		private async void Run()
		{
			while (true)
			{
				// Create new SituationSearch.
				SituationSearch situationSearch = new();

				// Check situation version time for updates.
				StatusText = "Tjekker for ny version";
				DateTime? newSituationVersionTime = await Task.Run(() => situationSearch.GetSituationVersion());


				if ((newSituationVersionTime != null && newSituationVersionTime > SituationVersionTime) || SituationVersionTime == null)
				{
					// Search for new situationRecords and fill list.
					StatusText = "Søger efter nye situationer";
					IList<DataGridContent> newContents = await Task.Run(() => situationSearch.SearchForNewSituations(DataGridContents.ToList()));

					// Update the observableCollection for datagrid with new content.
					DataGridContents.Clear();
					foreach (DataGridContent situation in newContents)
					{
						DataGridContents.Add(situation);
					}

					// Send pushoverNotification for new situations.
					if (SituationVersionTime != null)
					{
						Pushover pushover = new();

						// Check for situations newer than last situation update time.
						foreach (DataGridContent situation in newContents.Where(v => v.VersionTime > SituationVersionTime))
						{
							StatusText = "Sender pushover notifikation";
							PushoverMessageLog.Add(new PushoverLogModel()
							{
								Time = DateTime.Now,
								StatusMessage = await Task.Run(() => pushover.SendMessage(situation.VersionTime + " - " + situation.SituationText))
							});
						}
					}

					StatusText = "Færdig";

					// Set the new version time.
					SituationVersionTime = newSituationVersionTime;

					// Increment the search loop counter.
					SearchLoopCounter++;
				}
			}
		}
	}
}