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
using System.IO;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TTS_Translator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TB_JSON_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Tabletop Simulator\Saves";
            TB_mod_folder_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Tabletop Simulator\Mods";
        }

        private void Button_JSON_open_Click(object sender, RoutedEventArgs e)
        {
            //Open save json and parsing
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".json";
            dlg.Filter = "JSON (*.json)|*.json";
            dlg.InitialDirectory = TB_JSON_path.Text;

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                TB_JSON_path.Text = filename;
            }

            using (StreamReader jsonSave = File.OpenText(TB_JSON_path.Text))
            using (JsonTextReader reader = new JsonTextReader(jsonSave))
            {
                JObject jsonO = (JObject)JToken.ReadFrom(reader);
                JArray Objects = (JArray)jsonO["ObjectStates"];

                SortedSet<string> urls = new SortedSet<string>();

                foreach(JObject ob in Objects)
                {
                    if (ob["Name"].ToString().Equals("Custom_Tile"))
                    {
                        JObject tmp = (JObject)ob["CustomImage"];
                        urls.Add(tmp["ImageURL"].ToString());
                        urls.Add(tmp["ImageSecondaryURL"].ToString());
                    }
                    else if (ob["Name"].ToString().Equals("Custom_Token"))
                    {
                        JObject tmp = (JObject)ob["CustomImage"];
                        urls.Add(tmp["ImageURL"].ToString());
                    }
                    else if (ob["Name"].ToString().Equals("Deck") || ob["Name"].ToString().Equals("DeckCustom"))
                    {
                        foreach (var x in (JObject)ob["CustomDeck"])
                        {
                            string name = x.Key;
                            JObject tmp = (JObject)x.Value;
                            urls.Add(tmp["FaceURL"].ToString());
                            urls.Add(tmp["BackURL"].ToString());
                        }
                    }
                }
                System.Console.WriteLine(string.Join("\n", urls.ToArray()));
                DataTable dt = new DataTable();

                dt.Columns.Add("#", typeof(int));
                dt.Columns.Add("Original", typeof(string));
                dt.Columns.Add("New", typeof(string));
            }
        }

        private void Button_mods_open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.InitialDirectory = TB_mod_folder_path.Text;
            dlg.ValidateNames = false;
            dlg.CheckFileExists = false;
            dlg.CheckPathExists = true;
            dlg.FileName = "Select Folder";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string folderpath = System.IO.Path.GetDirectoryName(dlg.FileName);
                TB_mod_folder_path.Text = folderpath;
            }
        }
    }
}
