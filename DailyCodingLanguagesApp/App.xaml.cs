﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace DailyCodingLanguagesApp
{
    public class LanguageOfTheDay
    {
        public static LanguageOfTheDay Parse(DateTime date, string text)
        {
            string[] l = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            if (l.Length == 4)
            {
                LanguageOfTheDay tip = new LanguageOfTheDay();
                tip.Language = l[0];
                tip.Info = l[1];
                tip.Question = l[2];
                tip.Answer = l[3];
                tip.Date = date;

                return tip;
            }
            return null;
        }

        public string Language;

        public string Info;

        public string Question;

        public string Answer;

        public DateTime Date;
    };

    public partial class App : Application
    {
        private Dictionary<DateTime, LanguageOfTheDay> tips = new Dictionary<DateTime, LanguageOfTheDay>();
        private DateTime currentDate = DateTime.Now;
        protected override void OnStart()
        {
        }

        // Called from MainPage.xaml.cs
        public LanguageOfTheDay GetCurrentTip()
        {
            if (!tips.ContainsKey(currentDate))
            {
                if (tips.Count() == 0)
                {
                    // If there are not tips for today or any other day
                    return null;
                }
                currentDate = tips.Keys.Last();
            }
            return tips[currentDate];
        }

        public App()
        {
            InitializeComponent();
            LoadTips();
            MainPage = new MainPage();
            //added for navigation
            MainPage = new NavigationPage(new MainPage());
        }

        private DateTime GetDateFromAssetPath(string path)
        {
            try
            {
                var dateString = path.Split('.');
                if (dateString.Length < 5)
                {
                    Debug.Assert(false, "Date string not valid!");
                    throw new ArgumentOutOfRangeException(path);
                }

                var year = dateString[2].Trim('_');
                var month = dateString[3];
                var day = dateString[4];
                return DateTime.Parse(string.Format("{0}/{1}/{2}", year, month, day));
            }
            catch(Exception ex)
            {
                Debug.Assert(false, string.Format("Couldnt parse the date: {0}: {1}", path, ex.Message));
                throw ex;
            }
        }
        private void LoadTips()
        {
            // Read
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(MainPage)).Assembly;

            foreach (var assets in assembly.GetManifestResourceNames())
            {
                System.Diagnostics.Debug.WriteLine("found resource: " + assets);

                try
                {
                    var date = GetDateFromAssetPath(assets);            
                    // Grab asset
                    System.IO.Stream s = assembly.GetManifestResourceStream(assets);
                    System.IO.StreamReader sr = new System.IO.StreamReader(s);

                    // date as key first then as parameter
                    tips.Add(date, LanguageOfTheDay.Parse(date, sr.ReadToEnd()));
                    sr.Close();               
                }
                catch(Exception ex)
                {
                    Debug.Assert(false, String.Format("Invalid Tip File: {0}:{1}", assets, ex.Message));
                }
            }
        }

        //stackoverflow.com/a/18297009
        public void ChangeTip(int dir)
        { // param "dir" can be 1 or -1 to move index forward or backward
            List<DateTime> keys = new List<DateTime>(tips.Keys);
            int newIndex = keys.IndexOf(currentDate) - dir;
            if (newIndex < 0)
            {
                newIndex = 0;
            }
            else if (newIndex > tips.Count - 1)
            {
                newIndex = (tips.Count - 1);
            }

            currentDate = keys[newIndex];
        }

        public struct TipChangeStatus
        {
            public bool forwardPos;
            public bool backwardPos;
        };

        public TipChangeStatus TipChangePossible()
        {
            TipChangeStatus status;
            status.forwardPos = false;
            status.backwardPos = false;
            
            List<DateTime> keys = new List<DateTime>(tips.Keys);
           
            if (keys.IndexOf(currentDate) > 0)
            {
                status.backwardPos = true;
            }
            
            if (keys.IndexOf(currentDate) < (tips.Count - 1))
            {
                status.forwardPos = true;       
            }

            return status;
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
