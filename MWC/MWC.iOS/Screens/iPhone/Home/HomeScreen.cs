using MonoTouch.UIKit;
using System.Drawing;
using System;
using MonoTouch.Foundation;
using System.Collections.Generic;

using MWC.BL;

namespace MWC.iOS.Screens.iPhone.Home
{
	/// <summary>
	/// Home screen contains a masthead graphic/ad
	/// plus "what's on" in the next two 'timeslots'
	/// </summary>
	public partial class HomeScreen : UIViewController
	{
		Screens.Common.Session.SessionDayScheduleScreen _dayScheduleScreen;
		UI.Controls.LoadingOverlay loadingOverlay;

		public HomeScreen () : base (AppDelegate.IsPhone ? "HomeScreen_iPhone" : "HomeScreen_iPad", null)
		{
		}
		
		MWC.iOS.AL.DaysTableSource _tableSource = null;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			this.MwcLogoImageView.Image = UIImage.FromBundle("/Images/Home");
			BL.Managers.UpdateManager.UpdateFinished += HandleUpdateFinished; 

			if (AppDelegate.IsPad)
			{
				// http://forums.macrumors.com/showthread.php?t=901706
				this.SessionTable.Frame = new RectangleF(0,470, 320, 200);
				this.SessionTable.BackgroundColor = UIColor.Clear;
				//this.SessionTable.BackgroundView = null;

				this.UpNextTable.Frame = new RectangleF(0,470+210, 320, 320);
				this.UpNextTable.BackgroundColor = UIColor.Clear;
				//this.UpNextTable.BackgroundView = null;

				this.FavoritesTable.Frame = new RectangleF(768-320,470, 320, 420);
				this.FavoritesTable.BackgroundColor = UIColor.Clear;
				//this.FavoritesTable.BackgroundView = null;	
			}

			//TODO: Craig, i want to look at encapsulating this at the BL layer, 
			// i don't know if that's a feasible approach, but i think this is 
			// generally a good pattern.
			//
			// if we're in the process of updating, populate the table when it's done
			// alas, if we keep it in the app layer, it gives us an opportunity to 
			// show a spinner over the table with an "updating" message.
			if(BL.Managers.UpdateManager.IsUpdating)
			{
				loadingOverlay = new MWC.iOS.UI.Controls.LoadingOverlay ( this.SessionTable.Frame );
				this.View.AddSubview ( loadingOverlay );
				
				Console.WriteLine("Waiting for updates to finish");
				
			}
			else { this.PopulateTable(); }
		}
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			BL.Managers.UpdateManager.UpdateFinished -= HandleUpdateFinished; 
		}
		void HandleUpdateFinished(object sender, EventArgs e)
		{
			Console.WriteLine("Updates finished, going to populate table.");
			this.InvokeOnMainThread ( () => {
				this.PopulateTable ();
				if (loadingOverlay != null)
					loadingOverlay.Hide ();
			});
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return true;
		}
		
		protected void PopulateTable ()
		{
			Console.WriteLine ("PopulateTable called()");
			this._tableSource = new MWC.iOS.AL.DaysTableSource();
			this.SessionTable.Source = this._tableSource;
			this.SessionTable.ReloadData();
			this._tableSource.DayClicked += delegate(object sender, MWC.iOS.AL.DayClickedEventArgs e) {
				LoadSessionDayScreen ( e.DayName, e.Day );
			};

			//this.UpNextTable = new UITableView();
			this.UpNextTable.Source = new MWC.iOS.AL.DaysTableSource();
			this.UpNextTable.ReloadData();
			//this.FavoritesTable = new UITableView();
			this.FavoritesTable.Source = new MWC.iOS.AL.DaysTableSource();
			this.FavoritesTable.ReloadData ();

		}
		
		protected void LoadSessionDayScreen (string dayName, int day)
		{
			this._dayScheduleScreen = new MWC.iOS.Screens.Common.Session.SessionDayScheduleScreen ( dayName, day );
			this.NavigationController.PushViewController ( this._dayScheduleScreen, true );
		}
		
		/// <summary>
		/// Is called when the view is about to appear on the screen. We use this method to hide the 
		/// navigation bar.
		/// </summary>
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			this.NavigationController.SetNavigationBarHidden (true, animated);
		}
		
		/// <summary>
		/// Is called when the another view will appear and this one will be hidden. We use this method
		/// to show the navigation bar again.
		/// </summary>
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			this.NavigationController.SetNavigationBarHidden (false, animated);
		}
	}
}