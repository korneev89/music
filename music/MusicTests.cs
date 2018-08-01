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
			wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
			
			driver.Manage().Window.Size = new System.Drawing.Size(1600, 1000);
			//driver.Manage().Window.Minimize();
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
		//[Retry(100)]
		public void DownloadPlaylistFromZaycevNet()
		{
			driver.Url = "http://zaycev.net/search.html";

			string playlistFileName = "tracks.csv";
			List<TrackInfo> tracks = Utils.TracksFromFile(playlistFileName);

			bool sendPulse = true;
			try
			{
				foreach (var t in tracks)
				{
					if (t.IsDownloaded || t.Skip)
					{
						continue;
					}

					// skip track if any error and start downloading with next one
					t.Skip = true;

					t.Downloaded = DateTime.Now;
					bool findTrack = true;
					var i = 1;

					do
					{
						driver.Url = "http://zaycev.net/search.html";
						driver.FindElement(By.CssSelector("input#search-form-query")).SendKeys($"{t.Artist} - {t.Title}" + Keys.Enter);

						if (sendPulse)
						{
							Thread.Sleep(4000);
							SendPulseDiscard();
							sendPulse = false;
						}

						try
						{
							// there is no search results
							if (driver.FindElements(By.CssSelector(":not(.track-is-banned) > .musicset-track__title a")).Count == 0)
							{
								findTrack = false;
								continue;
							}

							driver.Url = driver.FindElements(By.CssSelector(":not(.track-is-banned) > .musicset-track__title a"))[i].GetAttribute("href");
							var a = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("a#audiotrack-download-link")));

							Thread.Sleep(200);
							Utils.ScrollElementIntoView(driver, driver.FindElement(By.CssSelector(".audiotrack__buttons")));
							Utils.ScrollElementIntoView(driver, driver.FindElement(By.CssSelector(".audiotrack__size")));

							string bitrate = driver.FindElement(By.CssSelector(".audiotrack__bitrate")).Text.Replace("битрейт ", String.Empty)
								.Replace(" kbps", String.Empty);

							string titleFromZaycev = driver.FindElement(By.CssSelector("h1")).Text;

							if (driver.FindElement(By.CssSelector("a#audiotrack-download-link")).GetAttribute("href").Contains("play.google"))
							{
								i += 2;
								continue;
							}
							driver.FindElement(By.CssSelector("a#audiotrack-download-link")).Click();
							findTrack = false;

							t.TitleFromZaycev = titleFromZaycev;
							t.Bitrate = bitrate;
							t.IsDownloaded = true;
							t.Downloaded = DateTime.Now;
							t.Skip = false;
						}
						catch (Exception e)
						{
							i +=2 ;
						}

						if (i > 10)
						{
							findTrack = false;
							//Assert.Fail($"Больше 6 попыток скачать трек {t.Title}");
						}
					}
					while (findTrack);
				}
			}

			catch (Exception e)
			{
				Assert.Fail(e.Message);
			}
			finally
			{
				Utils.TracksToFile(tracks, playlistFileName);
			}
		}

		private void SendPulseDiscard()
		{
			if (Utils.TryFindElement(By.CssSelector(".sendpulse-disallow-btn"), out IWebElement sendpulse, driver))
			{
				bool visible = Utils.IsElementVisible(sendpulse);
				if (visible)
				{
					sendpulse.Click();
				}
			}
		}

		[TearDown]
		public void Stop()
		{
			Thread.Sleep(10000);

			driver.Quit();
			driver = null;
		}
	}
}