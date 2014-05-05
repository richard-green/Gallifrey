﻿using Gallifrey.Serialization;

namespace Gallifrey.Settings
{
    public class AppSettings
    {
        public string JiraUrl { get; set; }
        public string JiraUsername { get; set; }
        public string JiraPassword { get; set; }

        public void SaveSettings()
        {
            AppSettingsSerializer.Serialize(this);    
        }
    }
}