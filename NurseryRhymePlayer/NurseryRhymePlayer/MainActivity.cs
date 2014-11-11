using System;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;
using System.Collections;
using System.Reflection;
using Android.Graphics;
using System.Text.RegularExpressions;
using Android.Content.PM;
using Android.Gms.Ads;

namespace NurseryRhymePlayer
{
	[Activity (Label = "Lullaby", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{

		//TODO:
		//Make gradient from image to list smaller
		//Get images for player bar
		//Player bar image swop functionality
		//Layout swops


		MediaPlayer player = null;
		FieldInfo[] arrFilenames;
		ArrayList xFilenamesList;
		ArrayList yFilenamesList;
		Color red = new Color (200, 50, 100, 150);
		Color green = new Color (100, 50, 100, 150);
		Color white = new Color (255, 255, 255, 150);
		Boolean repeat=false;
		Int32 songNameId;
		String songName;
		int hour;
		int minute;
		const int TIME_DIALOG_ID = 0;
		private System.Timers.Timer _timer;
		private int _countSeconds;
		String audioType;
		ListView songList;

		protected override void OnCreate (Bundle bundle)
		{
			RequestWindowFeature(WindowFeatures.NoTitle);

			base.OnCreate (bundle);

			SetContentView (Resource.Layout.Main);

			xFilenamesList = new ArrayList();
			yFilenamesList = new ArrayList();

			CreateAd();

			try 
			{
  				if (player == null) 
				{
					player = new MediaPlayer ();
				} 
				else 
				{
					player.Reset ();
				}

				//Get timer times stored in a file
				GetStoredTimes();

				//Populate arrays of songlists from filenames
				PopulateFileNameArrays();

				//Populate filename arrays and load listview
				songList= FindViewById<ListView> (Resource.Id.Songs);
				if(songList != null)
				{
					songList.Adapter = PopulateSongListAdapter(xFilenamesList);
				}
					
				//Handle Click Event of ListView - Play file 
				songList.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) =>
				{
					ListItemClick(e.Position);
				};

				//Play\Pause Button
				ImageButton playPauseButton = FindViewById<ImageButton> (Resource.Id.Pause);
				//playPauseButton.SetBackgroundColor(white);

				//Handle Click event of Pause button - Pause and play music and change imaage
				playPauseButton.Click += delegate 
				{
					if (player != null)
					{
						if(player.IsPlaying)
						{
							player.Pause();
							SetPlayPauseImage("play");
							//playpauseButton.Text = "Play";
							//playpauseButton.SetBackgroundColor (white); 
						}
						//Handle first click of play button when nothing else is playing
						else if(player.Duration == 0)
						{
							ListItemClick(0);
						}
						else
						{
							player.Start();
							SetPlayPauseImage("pause");
							//playpauseButton.Text = "Pause";
							//playpauseButton.SetBackgroundColor (white);
						}
					}
				};

				//Repeat Button
				Button repeatButton = FindViewById<Button> (Resource.Id.Repeat);
				repeatButton.SetBackgroundColor (white);

				//Handle Click event of repeat button - Set repeat paramenter and change image
				repeatButton.Click += delegate
				{
					if(repeat==false)
					{
						repeat = true;
					}
					else
					{
						repeat = true;
					}
				};

				//Timer Button
				Button timerButton = FindViewById<Button> (Resource.Id.Timer);
				timerButton.SetBackgroundColor (white);

				//Handle Click event of repeat button - Set repeat paramenter and change image
				timerButton.Click += (s, arg) =>
				{
//					PopupMenu menu = new PopupMenu(this, timerButton);
//					//menu.Inflate (Resource.Menu.popup_menu);
//					menu.MenuInflater.Inflate (Resource.Layout.popup_menu, menu.Menu);
//					menu.Show ();
					ShowDialog (TIME_DIALOG_ID);

//					var alert = new AlertDialog.Builder(this);
//					alert.SetView(LayoutInflater.Inflate(Resource.Layout.popup_menu, null));
//					alert.Create().Show();
				};	

				//Swop Button
				Button swopButton = FindViewById<Button> (Resource.Id.Swoping);
				swopButton.SetBackgroundColor (white);

				//Handle Click event of swap button - Swop list paramenter and change image
				swopButton.Click += delegate
				{

					if (player == null) 
					{
						player = new MediaPlayer ();
					} 
					else 
					{
						player.Reset ();
					}
						
					songList= FindViewById<ListView> (Resource.Id.Songs);
					if(songList != null)
					{
						if(audioType == "Noise")
						{
							audioType = "Music";
							songList.Adapter = PopulateSongListAdapter(xFilenamesList);
						}
						else
						{
							audioType = "Noise";
							songList.Adapter = PopulateSongListAdapter(yFilenamesList);
						}
					}
				};	
			} 
			catch (Exception ex) 
			{
				Console.Out.WriteLine (ex.StackTrace);
			}
		}


		#region Retrievals

		private void GetStoredTimes()
		{
			//Get stored time from file:
			var prefs = this.GetSharedPreferences("NurseryRhymePlayer", FileCreationMode.Private);
			hour = prefs.GetInt("hour", 1);
			minute = prefs.GetInt("minute", 0);
			audioType = prefs.GetString("audioType", "Music");
		}

		private void PopulateFileNameArrays()
		{
			//PopulateListView;
			var allFilenames = typeof(Resource.Raw).GetFields();
			//var songListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1);

			foreach (var fieldInfo in allFilenames)
			{
				if(fieldInfo.Name.Substring(0,1) == "x")
				{
					xFilenamesList.Add(fieldInfo);
//					if (audioType == "Music") 
//					{
//						songListAdapter.Add(Regex.Replace(fieldInfo.Name.Substring(1), "(\\B[A-Z])", " $1"));
//					}
				}
				else if(fieldInfo.Name.Substring(0,1) == "y")
				{
					yFilenamesList.Add(fieldInfo);
//					if (audioType == "Noise") 
//					{
//						songListAdapter.Add(Regex.Replace(fieldInfo.Name.Substring(1), "(\\B[A-Z])", " $1"));
//					}
				}
			}
			//return songListAdapter;
		}

		private ArrayAdapter<String> PopulateSongListAdapter(ArrayList fileNames)
		{
			var songListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1);

			FieldInfo[] afilenames = fileNames.ToArray(typeof(FieldInfo)) as FieldInfo[];

			foreach (var fieldInfo in afilenames)
			{
				if((fieldInfo.Name.Substring(0,1) == "x") || (fieldInfo.Name.Substring(0,1) == "y"))
				{
					songListAdapter.Add(Regex.Replace(fieldInfo.Name.Substring(1), "(\\B[A-Z])", " $1"));
				}

			}

			return songListAdapter;
		}

		#endregion

		#region SetImages

		private void SetPlayPauseImage(string playPause)
		{
			ImageButton playPauseBut = FindViewById<ImageButton> (Resource.Id.Pause);

			if (playPause == "pause") 
			{
				playPauseBut.SetImageResource(Resource.Drawable.pause);
			} 
			else 
			{
				playPauseBut.SetImageResource(Resource.Drawable.play);
			}
		}

		private void ChangeLayoutBackground(String gender)
		{
			if (gender == "boy") {
				var header = FindViewById<ImageView> (Resource.Id.imgHeader);
				header.SetImageResource(Resource.Drawable.moonbabyboycolouredTransparentWithBackground_Unlayered_Rectangle_720);

				var TopListBar = FindViewById<ImageView> (Resource.Id.imgToplistbar);
				TopListBar.SetImageResource(Resource.Drawable.toplistbar);

				var lstView = FindViewById<ListView> (Resource.Id.Songs);
				//lstView.SetBackgroundColor('#CBCBF6');

				var BottomListBar = FindViewById<ImageView> (Resource.Id.bottomListbar);
				BottomListBar.SetImageResource(Resource.Drawable.bottomlistbar);
			} 
			else 
			{
				//Set girl images
			}
		}

		#endregion

		#region Timer and Dialog

		protected override Dialog OnCreateDialog (int id)
		{
			if (id == TIME_DIALOG_ID)
				return new TimePickerDialog (this, TimePickerCallback, hour, minute, false);

			return null;
		}

		private void TimePickerCallback (object sender, TimePickerDialog.TimeSetEventArgs e)
		{
			hour = e.HourOfDay;
			minute = e.Minute;

			//Save to config
			var prefs = this.GetSharedPreferences("NurseryRhymePlayer", FileCreationMode.Private);
			var editor = prefs.Edit();

			editor.PutInt("hour", hour);
			editor.PutInt("minute", minute);

			editor.Commit ();
		}



		#endregion

		#region ListItemClick

		private void ListItemClick(int pos)
		{
			{
				//Play\Pause Button
				SetPlayPauseImage("pause");

				_timer = new System.Timers.Timer();
				//Trigger event every second
				_timer.Interval = 1000;
				_timer.Elapsed += delegate
				{
					_countSeconds--;

					if (_countSeconds == 0)
					{
						_timer.Stop();
						player.Stop();
						SetPlayPauseImage("play");
					}
				};
				//count down 5 seconds
				//_countSeconds = ((hour * 60)*60) + (minute * 60);
				_countSeconds = 0 + (minute * 60);
				_timer.Enabled = true;
			}

			player.Reset ();
			if (audioType == "Music") 
			{
				arrFilenames = xFilenamesList.ToArray(typeof(FieldInfo)) as FieldInfo[];
			}
			else
			{
				arrFilenames = yFilenamesList.ToArray(typeof(FieldInfo)) as FieldInfo[];
			}
			songName = arrFilenames[pos].Name;
			songNameId = Convert.ToInt32(arrFilenames [pos].GetValue(arrFilenames[pos]));
			player = MediaPlayer.Create(this, songNameId);
			//player.Prepared += player_Prepared;
			player.Completion +=  delegate 
			{
				Int32 position = pos + 1;
				if(position > arrFilenames.Length - 1)
				{
					position = 0;
				}

				if (repeat==true)
				{
					player.Reset ();
					player = MediaPlayer.Create(this, songNameId);
					player.Start();
				}
				else
				{
					songNameId = Convert.ToInt32(arrFilenames [position].GetValue(arrFilenames[position]));
					player.Reset ();
					player = MediaPlayer.Create(this, songNameId);
					player.Start();
				}
			};
			player.Start();
		}

		#endregion

		#region Adverts


		public void CreateAd()
		{
			var ad = new AdView(this);
			ad.AdSize = AdSize.SmartBanner;
			ad.AdUnitId = "ca-app-pub-8305178619637580/4095477951";
			var requestbuilder = new AdRequest.Builder();
			ad.LoadAd(requestbuilder.Build());
			//var layout = FindViewById<RelativeLayout>(Resource.Id.Main);
			var layout = FindViewById<LinearLayout>(Resource.Id.Main);
			layout.AddView(ad, 0);		
		}


		#endregion

		#region useful commented functions

//		protected override void OnResume()
//		{
//			base.OnResume();
//		}

//		private static void player_Prepared( object sender, EventArgs e )
//		{
//				Android.Media.MediaPlayer player = (Android.Media.MediaPlayer)sender;
//
//				player.SeekTo( 0 );
//				player.Start();
//		}

//		private static void player_Completion( object sender, EventArgs e )
//		{
//			 Comments player = MediaPlayer.Create(this, songNameId);
//			Android.Media.MediaPlayer player = (Android.Media.MediaPlayer)sender;
//
//			player.Completion -= player_Completion;
//
//			player.Stop();
//			player.Release();
//		}

		//protected void PopulateFileNames(String audioType)
		//		{
		//			FieldInfo[] tempFileNames;
		//			tempFileNames = typeof(Resource.Raw).GetFields();
		//
		//			if (audioType == "Noise") 
		//			{
		//				foreach (var fieldInfo in tempFileNames)
		//				{
		//					if(fieldInfo.Name.Substring(0,1) == "x")
		//					{
		//						filenames.
		//					}
		//				}
		//
		//				for (int i = 0; i < filenames.Length; i++) 
		//				{
		//					if(filenames.GetValue[i].
		//				}
		//
		//			} 
		//			else
		//			{
		//
		//			}
		//
		//		}

		#endregion
	}
}


