using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Windows;

namespace profileEditor
{
    public class classSettings
    {
        string regPath = "Software\\files";
        RegistryKey regkey;

        public string ProfilesDir { get; set; }
        public string LayoutsDir { get; set; }
        public string FilesPath { get; }


        public classSettings()
        {

            FilesPath = "\\";
          //  ProfilesDir = "profiles";
          //  LayoutsDir = "templates";
          //  regkey = Registry.CurrentUser.OpenSubKey(regPath, true);
          //  FilesPath = (string)regkey.GetValue("filesPath");
          //  if (FilesPath == null)
          //      MessageBox.Show("FilesPath Missing");
        }
    }
}
