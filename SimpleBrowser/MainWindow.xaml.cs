using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.Win32;
using System.Reflection;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string PAGE_DEFAUT = "https://www.whatismybrowser.com/";
        public MainWindow()
        {
            SetBrowserFeatureControl();
            InitializeComponent();          
        }

        private void Fenetre_chargee(object sender, RoutedEventArgs e)
        {
            HideScriptErrors(this.webBrowser, true);
            this.webBrowser.Navigate(PAGE_DEFAUT);
            textbox_url.Text = PAGE_DEFAUT;
        }

        private bool checkUrl(string url)
        {          
            if(Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                var req = (HttpWebRequest)HttpWebRequest.Create(url);
                bool isUrlValid = false;
                req.AllowAutoRedirect = false;
                try
                {
                    using (var resp = req.GetResponse())
                    {
                        var location = resp.Headers["Location"];
                        if (!String.IsNullOrEmpty(location))
                        {
                            isUrlValid = true;
                        }
                    }
                }
                catch
                {
                    isUrlValid = false;
                }
                return isUrlValid;
            }else
            {
                return false;
            }
            
        }

      
        private void btn_aller_Click(object sender, RoutedEventArgs e)
        {
            if (!((textbox_url.Text).StartsWith("http://") && !((textbox_url.Text).StartsWith("https://"))))
            {
                (textbox_url.Text) = "http://" + (textbox_url.Text);
            }

            if (checkUrl(textbox_url.Text))
            {
                this.webBrowser.Navigate(textbox_url.Text);
            }
        }

        private void textbox_url_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                if (!((textbox_url.Text).StartsWith("http://") && !((textbox_url.Text).StartsWith("https://"))))
                {
                    (textbox_url.Text) = "http://" + (textbox_url.Text);
                    if (checkUrl(textbox_url.Text))
                    {
                        this.webBrowser.Navigate(textbox_url.Text);
                    }
                }
    
            }
        }


        // charger temporairement les parametres
        private void SetBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }


        private void SetBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            SetBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, GetBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            SetBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            SetBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            SetBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        private UInt32 GetBrowserEmulationMode()
        {
            int browserVersion = 7;
            using (var ieKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer",
                RegistryKeyPermissionCheck.ReadSubTree,
                System.Security.AccessControl.RegistryRights.QueryValues))
            {
                var version = ieKey.GetValue("svcVersion");
                if (null == version)
                {
                    version = ieKey.GetValue("Version");
                    if (null == version)
                        throw new ApplicationException("Microsoft Internet Explorer is required!");
                }
                int.TryParse(version.ToString().Split('.')[0], out browserVersion);
            }

            UInt32 mode = 11000; // Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 Standards mode. Default value for Internet Explorer 11.
            switch (browserVersion)
            {
                case 7:
                    mode = 7000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
                    break;
                case 8:
                    mode = 8000; // Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8
                    break;
                case 9:
                    mode = 9000; // Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
                    break;
                case 10:
                    mode = 10000; // Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 mode. Default value for Internet Explorer 10.
                    break;
                default:
                    // use IE11 mode by default
                    break;
            }

            return mode;
        }

        // cacher les messages derreurs javascript
        public void HideScriptErrors(WebBrowser wb, bool Hide)
        {
            FieldInfo fiComWebBrowser = typeof(WebBrowser)
                .GetField("_axIWebBrowser2",
                          BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            object objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null) return;
            objComWebBrowser.GetType().InvokeMember(
                "Silent", BindingFlags.SetProperty, null, objComWebBrowser,
                new object[] { Hide });
        }


    }
}
