using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosshairWindow.Model;
using Gma.System.MouseKeyHook;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using Drawing = System.Drawing;
using KeyEventArgs		= System.Windows.Forms.KeyEventArgs;
using Keys = System.Windows.Forms.Keys;


namespace CrosshairTest2.ViewModel
{
    class CrosshairVM : ObservableObject
	{
        private readonly Stopwatch _stopwatch;
		public Crosshair? CurrentCrosshair { get; set; } = null;

		public AsyncRelayCommand RelayOpenCommand { get; set; }
        public RelayCommand RelayQuitCommand { get; set; }

        private readonly IKeyboardMouseEvents _globalHook;
		static HashSet<Keys> _pressedKeys = [];

		private string _keyDisplay = "";

		public string KeyDisplay
		{
			get => _keyDisplay;
			set
			{
				_keyDisplay = value;
				OnPropertyChanged();
			}
		}




        public CrosshairVM()
        {
			//EnsureCrosshairFile();

			InitializeCrosshair();

            RelayOpenCommand = new AsyncRelayCommand(OpenApplication);
            RelayQuitCommand = new RelayCommand(QuitApplication);


			_stopwatch = new Stopwatch();
			_stopwatch.Start();

			// Subscribe to WPF's rendering event, which occurs every frame
			CompositionTarget.Rendering += Update;


            // Subscribe to global keyboard events
            _globalHook = Hook.GlobalEvents();
            _globalHook.KeyDown += GlobalHookKeyDown;
            _globalHook.KeyUp += GlobalHookKeyUp;


			// Default display text
			//KeyDisplay = "Press a key!";


		}


        private void InitializeCrosshair()
        {
			CurrentCrosshair = new()
			//Crosshair crossSettings = new()
			{
				CrosshairName = "COOL CROSSHAIR",

				Scale = 1F,
				Opacity = 1F,
				Angle = 0F,
				RotationSpeed = 0F,
				OffsetX = 0,
				OffsetY = 0,
				Visible = true,
				UsesImage = false,

				CenterDot = new()
				{
					Length = 2,
					Height = 2,
					Shape = CenterDot.ShapeEnum.Circle,
					Color = Drawing.Color.DarkRed,
					Opacity = 0.1F,
					Angle = 0F,
					Outline = new()
					{
						Thickness = 1,
						Color = Drawing.Color.Black,
						Opacity = 0.1F,
						Visible = true
					},
					OffsetX = 0,
					OffsetY = 0,
					Visible = true
				},

				Lines = new()
				{
					Thickness = 3,
					Length = 7,
					Shape = Lines.ShapeEnum.Rectangle,
					Color = ColorTranslator.FromHtml("#FF0000"),
					Opacity = 0.4F,
					Angle = 0F,
					GapX = 5,
					GapY = 100, // does nothing
					OffsetX = 0,
					OffsetY = 0,
					Visible = true,
					Outline = new()
					{
						Thickness = 2,
						Color = Drawing.Color.Black,
						Opacity = 0.2F,
						Visible = true
					},
				},

				CrosshairBitmap = new("C:\\Users\\budget\\Projects\\BudgetCrosshair\\Resources\\Crosshair_1.png"),

			};

			CurrentCrosshair.Lines.SetDirection(Lines.Direction.LeftUp, false);
			CurrentCrosshair.Lines.SetDirection(Lines.Direction.LeftDown, false);
			CurrentCrosshair.Lines.SetDirection(Lines.Direction.RightUp, false);
			CurrentCrosshair.Lines.SetDirection(Lines.Direction.RightDown, false);

			//crossSettings.Lines.SetDirection(Lines.Direction.LeftUp, false);
			//crossSettings.Lines.ToggleDirection(Lines.Direction.Up);

			CurrentCrosshair.Draw();	
			// old settings
			/*
			

			CrosshairSettings settings = new()
			{
				Scale = 1,
				Angle = 0F,
				OffsetX = 0,
				OffsetY = 0,
				RotationSpeed = 50.4F,

				CenterDot = new()
				{
					Shape = CenterDot.ShapeEnum.Circle,
					Length = 3,
					Height = 3,
					Color = System.Drawing.Color.Red,
					Opacity = 1F,
					Angle = 0F,
					Outline = new()
					{
						Thickness = 1,
						Color = System.Drawing.Color.White,
						Opacity = 1F,
						Visible = true
					},
					OffsetX = 0,
					OffsetY = 0,
					Visible = true
				},

				Lines = new()
				{
					Thickness = 5,
					Length = 50,
					Color = System.Drawing.Color.Red,
					Opacity = 1F,
					Angle = 20F,
					GapX = 10,
					GapY = 10,
					Outline = new()
					{
						Thickness = 2,
						Color = System.Drawing.Color.Black,
						Opacity = 1F,
						Visible = true
					},
					OffsetX = 50,
					OffsetY = 0,
					Visible = true
				},

				Outline = new()
				{
					Opacity = 1F,
					Thickness = 2,
					Color = System.Drawing.Color.Black,
					Visible = true
				}
			};

			*/

            using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BudgetCrosshair.Resources.Crosshair_1.png");
            if (stream == null) 
				throw new Exception("Resource not found!");

			var crosshair = new Bitmap(stream);

			//CurrentCrosshair = crossSettings;
			//string crosshairSettings = crossSettings.GetSettings();
			//CurrentCrosshair = new Crosshair("MyCrosshair", crossSettings.GetSettings(), crosshair);
            //CurrentCrosshair = new Crosshair("MyCrosshair", crossSettings.GetSettings(), "Resources/Crosshair_1.png");


		}


		private void Update(object? sender, EventArgs e)
        {
			float elapsedSec = _stopwatch.ElapsedMilliseconds / 1000f;

			CurrentCrosshair?.Update(elapsedSec);


			_stopwatch.Restart();
		}

        private async Task OpenApplication()
        {
            await Task.Delay(1000);

            await Task.Run(() =>
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() =>
				{


					// do nothing
					var openFileDialog = new Microsoft.Win32.OpenFileDialog
					{
						Filter = "Image Files|*.bmp;*.png;*.jpg;*.jpeg|All Files|*.*",
						Title = "Select A Crosshair"
					};

					bool? result = openFileDialog.ShowDialog();

					if (result == false)
					{
						Console.WriteLine("Failed to show dialog");
						Console.WriteLine(openFileDialog);
						return;
					}

					//ReplaceCrosshair(openFileDialog.FileName);


					//if (true)
						//return;

					using (var bitmap = Crosshair.GetStoredCrosshair())
					{
						// Save bitmap to a MemoryStream, then convert to Base64 or save it as a file
						using (var memoryStream = new MemoryStream())
						{
							bitmap.Save(memoryStream, ImageFormat.Png);
							var base64String = Convert.ToBase64String(memoryStream.GetBuffer());

							// Use the base64String as the ImageUrl (this can be used as an ImageSource in XAML)
							if (CurrentCrosshair == null)
								return; 

                            //CurrentCrosshair.ImageUrl = $"data:image/png;base64,{base64String}";
							//CurrentCrosshair.CrosshairBitmap = new Bitmap()  
						}
					}



                });
			});

        }

        private void QuitApplication()
        {
            Environment.Exit(0);
        }


		


		private void GlobalHookKeyDown(object? sender, KeyEventArgs e)
		{
			// Detect non-character keys (e.g., F1, Shift, etc.)
			_pressedKeys.Add(e.KeyCode);	
		
			
			Console.WriteLine("KeyDown: {0}", e.KeyCode);
			// KeyDisplay = e.KeyCode.ToString(); // Hidden bc build 


            if (_pressedKeys.Contains(Keys.ControlKey) && _pressedKeys.Contains(Keys.Alt))
                Console.WriteLine("TESTER");

			//if (e.KeyCode == Keys.F1)
			if (_pressedKeys.Contains(Keys.F1))
				Console.WriteLine("F1 key pressed");


		}



		private void GlobalHookKeyUp(object? sender, KeyEventArgs e)
		{
			// Detect non-character keys (e.g., F1, Shift, etc.)
			_pressedKeys.Remove(e.KeyCode);	
		}


		// Cleanup the hook when no longer needed
		public void Dispose()
		{
            _globalHook?.Dispose();
		}


	}
}
