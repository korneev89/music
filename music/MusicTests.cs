using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace music
{ 
	[TestFixture]
	public class MusicTests
	{
		private IWebDriver driver;
		private WebDriverWait wait;

		[SetUp]
		public void Start()
		{
			ChromeOptions options = new ChromeOptions();
			//options.AddArgument("headless");

			driver = new ChromeDriver(options);
			wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
			
			driver.Manage().Window.Size = new System.Drawing.Size(1600, 1000);
		}

		[Test]
		public void DownloadPlaylistFromYandexMusic()
		{
			driver.Url = "https://music.yandex.ru/users/pro100dimon12/playlists/3";

			List<TrackInfo> tracks = new List<TrackInfo>();

			int last = 0;
			int prevLast = 1;

			while (last != prevLast)
			{
				prevLast = last;
				int tracksCount = driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable")).Count;

				for (int i = 0; i < tracksCount; i++)
				{
					if ( int.Parse(driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[i].GetAttribute("data-b")) > last)
					{
						var trackInfo = new TrackInfo
						{
							Title = driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[i].FindElement(By.CssSelector(".d-track__title")).Text,
							//Secondary = driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[i].Text,
							Artist = driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[i].FindElement(By.CssSelector(".d-track__artists")).Text,
							Duration = driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[i].FindElement(By.CssSelector(".typo-track")).Text
						};
						tracks.Add(trackInfo);
					}
				}
				last = int.Parse(driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[tracksCount - 1].GetAttribute("data-b"));
				Utils.ScrollElementIntoView(driver, driver.FindElements(By.CssSelector(".d-track.typo-track.d-track_selectable"))[tracksCount-1]);
			};

			string playlistFileName = "tracks.csv";

			Utils.TracksToFile(tracks, playlistFileName);
		}

		[Test]
		public void DownloadPlaylistFromZaycevNet()
		{
			driver.Url = "http://zaycev.net/search.html";

			string playlistFileName = "tracks.csv";
			List<TrackInfo> tracks = Utils.TracksFromFile(playlistFileName);

			foreach (var t in tracks)
			{
				driver.FindElement(By.CssSelector("input#search-form-query")).SendKeys($"{t.Artist} - {t.Title}" + Keys.Enter);
				var firstTrack = driver.FindElement(By.CssSelector(".musicset-track__title"));
				Utils.MoveMouseOverElement(driver, firstTrack);
				driver.FindElement(By.CssSelector(".musicset-track__download")).Click();

				wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("a#audiotrack-download-link")));

				// SCROLL to element

				driver.FindElement(By.CssSelector("a#audiotrack-download-link")).Click();

				driver.Url = "http://zaycev.net/search.html";
			}
		}

		[TearDown]
		public void Stop()
		{
			driver.Quit();
			driver = null;
		}
	}
}