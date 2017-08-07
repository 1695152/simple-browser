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
using System.Xml;
using mshtml;
using System.IO;

namespace SimpleBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string PAGE_DEFAUT = "https://duckduckgo.com/";
        const string URL_RECHERCHE = "https://duckduckgo.com?q=";

        // utilisé pour référence après chaque transaction
        private static string _url = String.Empty;

        // historique de session
        List<string> historique = new List<string>();
        // position_visite_historiqu
        int historique_position = 0;


        public MainWindow()
        {
            setBrowserFeatureControl();
            InitializeComponent();          
        }

        private void Fenetre_chargee(object sender, RoutedEventArgs e)
        {
            ajusterTailleGUI();
            mettreAJourButtonsHistorique();
            hideScriptErrors(this.webBrowser, true);
            naviguer(this.webBrowser, PAGE_DEFAUT);
            this.historique.Add(PAGE_DEFAUT);
            textbox_url.Text = PAGE_DEFAUT;
        }

        /// <summary>
        /// verifie si lien est valide
        /// </summary>
        /// <param name="url">lien</param>
        /// <returns>la reponse</returns>
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
            var url = this.textbox_url.Text;

            var isPrefixHTTP = (url).StartsWith("http://");
            var isPrefixHTTPS = (url).StartsWith("https://");

            if(isPrefixHTTP == false && isPrefixHTTPS == isPrefixHTTP)
            {
                // si le url ne possde pas de prefixe, ajouter prefixe
                url = "https://" + (url);
                this.textbox_url.Text = url;
            }

            if (checkUrl(url))
            {   // si le url est valide, naviguer vers url
                naviguer(this.webBrowser, url);

                // ajouter a historique
                this.historique.Add(url);
                // mettre  a jour la position par rapport a l'historique
                this.historique_position = this.historique.Count() - 1;
            }
            else
            {   // faire recherche avec le lien fautif pour permettre une suggestion
                naviguer(this.webBrowser, URL_RECHERCHE + textbox_url.Text);
            }
            mettreAJourButtonsHistorique();
        }

        /// <summary>
        /// alias, appelle btn_aller_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textbox_url_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                this.btn_aller_Click(sender, e);
            }
        }

        // charger temporairement les parametres
        private void setBrowserFeatureControlKey(string feature, string appName, uint value)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(
                String.Concat(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\", feature),
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                key.SetValue(appName, (UInt32)value, RegistryValueKind.DWord);
            }
        }
        
        // initialisation des parametres pour webbrowser
        private void setBrowserFeatureControl()
        {
            // http://msdn.microsoft.com/en-us/library/ee330720(v=vs.85).aspx

            // FeatureControl settings are per-process
            var fileName = System.IO.Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

            setBrowserFeatureControlKey("FEATURE_BROWSER_EMULATION", fileName, getBrowserEmulationMode()); // Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode.
            setBrowserFeatureControlKey("FEATURE_AJAX_CONNECTIONEVENTS", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_ENABLE_CLIPCHILDREN_OPTIMIZATION", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_MANAGE_SCRIPT_CIRCULAR_REFS", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_DOMSTORAGE ", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_GPU_RENDERING ", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_IVIEWOBJECTDRAW_DMLT9_WITH_GDI  ", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_DISABLE_LEGACY_COMPRESSION", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_LOCALMACHINE_LOCKDOWN", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_OBJECT", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_BLOCK_LMZ_SCRIPT", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_DISABLE_NAVIGATION_SOUNDS", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_SCRIPTURL_MITIGATION", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_SPELLCHECKING", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_STATUS_BAR_THROTTLING", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_TABBED_BROWSING", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_VALIDATE_NAVIGATE_URL", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_WEBOC_DOCUMENT_ZOOM", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_WEBOC_POPUPMANAGEMENT", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_WEBOC_MOVESIZECHILD", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_ADDON_MANAGEMENT", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_WEBSOCKET", fileName, 1);
            setBrowserFeatureControlKey("FEATURE_WINDOW_RESTRICTIONS ", fileName, 0);
            setBrowserFeatureControlKey("FEATURE_XMLHTTP", fileName, 1);
        }

        // https://stackoverflow.com/a/28626667
        // en lien avec setBrowserFeatureControl
        private UInt32 getBrowserEmulationMode()
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

        // https://stackoverflow.com/a/24377136
        // cacher les messages derreurs javascript (bugifx)
        private void hideScriptErrors(WebBrowser wb, bool Hide)
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

        private void naviguer(WebBrowser wb, string url)
        {
            wb.Navigate(url);
            textbox_url.Text = url;
            _url = url;
        }

        // https://stackoverflow.com/a/8005898
        // https://stackoverflow.com/a/6222430
        // effectuer des changements dans le dom du navigateur
        private void webBrowser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            var wb = sender as WebBrowser;
            var document = wb.Document as mshtml.HTMLDocument;
            string path = Environment.CurrentDirectory + @"\injectedJavascript.js"; 

            mshtml.IHTMLDocument2 htmldoc = this.webBrowser.Document as IHTMLDocument2;
            IHTMLElement el = (IHTMLElement)htmldoc.createElement("script");
            IHTMLScriptElement scriptEl = (IHTMLScriptElement)el;
            scriptEl.text = File.ReadAllText(path);

            el.id = "injectedScript";

            IHTMLDOMNode node = (IHTMLDOMNode)el;
            mshtml.HTMLBodyClass body = htmldoc.body as mshtml.HTMLBodyClass;
            body.appendChild(node);

            // mettre a jour la barre d'adresse
            textbox_url.Text = document.location.toString();

            // debugging
            //document.body.innerText = document.body.innerHTML.ToString();

        }

        /// <summary>
        /// ajuste les elements du GUI pour une taille optimale par rapport a l'ecran
        /// </summary>
        private void ajusterTailleGUI()
        {
            int width = int.Parse(System.Windows.SystemParameters.PrimaryScreenWidth.ToString());
            int height = int.Parse(System.Windows.SystemParameters.PrimaryScreenHeight.ToString());

            // stretching
            Application.Current.MainWindow.Width = width;
            Application.Current.MainWindow.Height = height;

            this.webBrowser.Height = (height * 0.92);
            this.webBrowser.Width = (width * 0.99);
        }

        // fonctionalite pour historique, btn precendent
        private void btn_historique_precedent_Click(object sender, RoutedEventArgs e)
        {
            if(this.historique_position > 0)
            {
                this.historique_position -= 1;
                naviguer(this.webBrowser, this.historique[this.historique_position]);
            }
            mettreAJourButtonsHistorique();
        }

        // fonctionalite pour historique, btn suivant
        private void btn_historique_suivant_Click(object sender, RoutedEventArgs e)
        {
            if(this.historique_position + 1 < this.historique.Count())
            {
                this.historique_position += 1;
                naviguer(this.webBrowser, this.historique[this.historique_position]);
            }
            mettreAJourButtonsHistorique();
        }

        // fonctionalite bouton page acceuil
        private void btn_accueil_Click(object sender, RoutedEventArgs e)
        {
            naviguer(this.webBrowser, PAGE_DEFAUT);
        }

        // mettre a jour ui pour historique
        private void mettreAJourButtonsHistorique()
        {
            // la premiere fois
            if(this.historique.Count() == 0)
            {
                this.btn_historique_precedent.Visibility = Visibility.Hidden;
                this.btn_historique_suivant.Visibility = Visibility.Hidden;
            }else
            { // les autres fois
                this.btn_historique_suivant.Visibility = Visibility.Visible;
                this.btn_historique_precedent.Visibility = Visibility.Visible;
            }

           
        }

    }
}
