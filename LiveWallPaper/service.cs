using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

namespace LiveWallPaper
{
    public partial class service : Form
    {
        public service()
        {
            InitializeComponent();
        }
        private void entry_Load(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                add_to_startup();
                Functions fn = new Functions();

                while (true)
                {
                    fn.check_background();
                    Thread.Sleep(5000);
                }
            });
        }
        private void add_to_startup()
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("LiveWallpaperByAhmedAbd", Application.ExecutablePath);
        }
    }
}
