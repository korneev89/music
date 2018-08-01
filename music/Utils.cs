using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace music
{
	public static class Utils
	{
		public static void TracksToFile(List<TrackInfo> tracks, string playlistFileName)
		{
			StreamWriter writer = new StreamWriter(CreateFileInfoToAssemblyDirectory(playlistFileName).FullName);

			foreach (var t in tracks)
			{
				writer.WriteLine($"{t.Title};{t.Artist};{t.Duration};{t.Bitrate};{t.AddedToPlaylist};{t.Downloaded};{t.IsDownloaded};{t.Skip};{t.TitleFromZaycev}");
			}
			writer.Close();
		}

		public static List<TrackInfo> TracksFromFile(string playlistFileName)
		{
			List<TrackInfo> ts = new List<TrackInfo>();

			var lines = File.ReadAllLines(CreateFileInfoToAssemblyDirectory(playlistFileName).FullName);
			foreach (string l in lines)
			{
				string[] parts = l.Split(';');
				ts.Add(new TrackInfo(parts[0], parts[1], parts[2], parts[3], DateTime.Parse(parts[4]), DateTime.Parse(parts[5]), Boolean.Parse(parts[6]), Boolean.Parse(parts[7]), parts[8]));
			}
			return ts;
		}

		public static FileInfo CreateFileInfoToAssemblyDirectory(string filename)
		{
			return new FileInfo(Path.Combine(
					Path.GetDirectoryName(
						Assembly.GetExecutingAssembly().Location),
						filename));
		}

		public static void ScrollElementIntoView(IWebDriver driver, IWebElement element)
		{
			// scroll element into viewport
			IJavaScriptExecutor jse = (IJavaScriptExecutor)driver;
			jse.ExecuteScript("arguments[0].scrollIntoView(true);", element);

			// wait for scroll action
			Thread.Sleep(500);
		}

		public static void MoveMouseOverElement(IWebDriver driver, IWebElement element)
		{
			Actions action = new Actions(driver);
			action.MoveToElement(element).Perform();
		}

		public static bool IsElementVisible(IWebElement element)
		{
			return element.Displayed && element.Enabled;
		}

		public static bool TryFindElement(By by, out IWebElement element, IWebDriver driver)
		{
			try
			{
				element = driver.FindElement(by);
			}
			catch (NoSuchElementException ex)
			{
				element = null;
				return false;
			}
			return true;
		}

		public static void SendTelegramToRMAdmins(string message, IWebDriver driver)
		{
			var telegramURL = @"https://api.telegram.org";
			var token = ConfigurationManager.AppSettings["RM_bot_token"];

			List<string> addressees = new List<string>
			{
				"168694373", //dkorneev
				"347947909" //avb
			};

			foreach (string a in addressees)
			{
				string url = $"{telegramURL}/bot{token}/sendMessage?chat_id={a}&text={message}";
				driver.Url = url;
			}
		}
	}
}
