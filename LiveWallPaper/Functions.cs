using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using IniParser;
using IniParser.Model;
using LiveWallPaper.Modals;
using System.IO;

namespace LiveWallPaper
{
    class Functions
    {
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, String pvParam, uint fWinIni);

        private const uint SPI_SETDESKWALLPAPER = 0x14;
        private const uint SPIF_UPDATEINIFILE = 0x1;
        private const uint SPIF_SENDWININICHANGE = 0x2;

        private SettingsModal check_api(SettingsModal s)
        {
            if (s.LastAPICheck == null || DateTime.Now > s.LastAPICheck.Value.AddDays(1))
            {
                var r = get_sunset();

                s = new SettingsModal()
                {
                    LastAPICheck = DateTime.Now,
                    Dawn = Convert.ToDateTime(r.results.astronomical_twilight_begin).ToLocalTime().TimeOfDay,
                    Sunrise = Convert.ToDateTime(r.results.sunrise).ToLocalTime().TimeOfDay,
                    Sunset = Convert.ToDateTime(r.results.civil_twilight_end).ToLocalTime().TimeOfDay
                };
                set_setting(s);
                return get_setting();
            }
            return s;
        }

        public void check_background()
        {
            SettingsModal s = get_setting();
            s = check_api(s);

            TimeSpan now = DateTime.Now.TimeOfDay;
            TimeSpan before_sunrise = s.Sunrise.Value.Add(TimeSpan.FromHours(-1));
            TimeSpan before_sunset = s.Sunset.Value.Add(TimeSpan.FromHours(-1));
            TimeSpan night1 = s.Sunset.Value;
            TimeSpan night2 = s.Sunset.Value.Add(TimeSpan.FromHours(1));
            TimeSpan night3 = s.Sunset.Value.Add(TimeSpan.FromHours(3));

            string image_name = "night-3";

            if (now > s.Dawn && now < s.Sunrise)
            {
                image_name = "dawn";
            }

            if (now > before_sunrise && now < s.Sunrise)
            {
                image_name = "before-sunrise";
            }

            if (now > s.Sunrise && now < before_sunset)
            {
                image_name = "day";
            }

            if (now > before_sunset && now < s.Sunset)
            {
                image_name = "before-sunset";
            }

            if (now > night1 && now < night2)
            {
                image_name = "night-1";
            }
            if (now > night2 && now < night3)
            {
                image_name = "night-2";
            }
            if (now > night3 || now < before_sunrise)
            {
                image_name = "night-3";
            }


            if (image_name != s.LastAppliedImage)
            {
                if (set_background(image_name))
                {
                    s.LastAppliedImage = image_name;
                    set_setting(s);
                }
            }
        }
        public bool set_background(string image_name)
        {
            try
            {
                uint flags = SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE;
                string image_path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", image_name + ".jpg");
                // Set the desktop background to this file.
                if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, image_path, flags))
                {
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", "Failed to set background !" + Environment.NewLine);
                    return false;
                }
                return true;
            }
            catch (Exception err)
            {
                File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + "log.txt", "Error while set background : " + err.Message + Environment.NewLine);
                return false;
            }
        }
        private dynamic get_sunset()
        {
            var responseString = "";
            using (var client = new WebClient())
            {
                responseString = client.DownloadString("https://api.sunrise-sunset.org/json?lat=33.285768&lng=44.399466&date=today");
            }
            return JsonConvert.DeserializeObject<dynamic>(responseString);
        }
        private SettingsModal get_setting()
        {
            SettingsModal r = new SettingsModal();
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini");

            if (!string.IsNullOrEmpty(data["Config"]["LastAPICheck"]))
            {
                r.LastAPICheck = DateTime.Parse(data["Config"]["LastAPICheck"]);
            }

            if (!string.IsNullOrEmpty(data["Config"]["Dawn"]))
            {
                r.Dawn = TimeSpan.Parse(data["Config"]["Dawn"]);
            }

            if (!string.IsNullOrEmpty(data["Config"]["Sunrise"]))
            {
                r.Sunrise = TimeSpan.Parse(data["Config"]["Sunrise"]);
            }

            if (!string.IsNullOrEmpty(data["Config"]["Sunset"]))
            {
                r.Sunset = TimeSpan.Parse(data["Config"]["Sunset"]);
            }
            r.LastAppliedImage = data["Config"]["LastAppliedImage"];

            return r;
        }
        private void set_setting(SettingsModal s)
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini");

            data["Config"]["LastAPICheck"] = s.LastAPICheck.ToString();
            data["Config"]["Dawn"] = s.Dawn.ToString();
            data["Config"]["Sunrise"] = s.Sunrise.ToString();
            data["Config"]["Sunset"] = s.Sunset.ToString();
            data["Config"]["LastAppliedImage"] = s.LastAppliedImage;

            parser.WriteFile(AppDomain.CurrentDomain.BaseDirectory + "config.ini", data);
        }
    }
}
