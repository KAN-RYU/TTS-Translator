using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TTS_Translator
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool saveOpened = false;

        //Delete Special Characters
        string DeleteSpecial(string s)
        {
            return s.Replace(":", "").Replace("/", "").Replace("-", "").Replace("=", "").Replace("?", "").Replace(".", "").Replace("%", "").Replace("&", "").Replace("_","");
        }

        public MainWindow()
        {
            InitializeComponent();
            //TB_JSON_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Tabletop Simulator\Saves";
            TB_JSON_path.Text = @"F:\SteamLibrary\steamapps\common\Tabletop Simulator\Tabletop Simulator_Data\Mods";
            //TB_mod_folder_path.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Tabletop Simulator\Mods";
            TB_mod_folder_path.Text = @"F:\SteamLibrary\steamapps\common\Tabletop Simulator\Tabletop Simulator_Data\Mods";

            string strVersionText = Assembly.GetExecutingAssembly().FullName
                                    .Split(',')[1]
                                    .Trim()
                                    .Split('=')[1];

            Title = Title + " " +  strVersionText;
        }

        //Open save json and parsing
        private void Button_JSON_open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "JSON (*.json)|*.json",
                Title = "Open TTS save",
                InitialDirectory = TB_JSON_path.Text
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                TB_JSON_path.Text = filename;
                using (StreamReader jsonSave = File.OpenText(TB_JSON_path.Text))
                using (JsonTextReader reader = new JsonTextReader(jsonSave))
                {
                    string[] FindURL(JArray Objects)
                    {
                        SortedSet<string> urls = new SortedSet<string>();
                        if (Objects == null) return urls.ToArray();
                        foreach (JObject ob in Objects)
                        {
                            if (ob["Name"].ToString().Equals("Custom_Tile") ||
                                ob["Name"].ToString().Equals("Custom_Token") ||
                                ob["Name"].ToString().Equals("Figurine_Custom"))
                            {
                                JObject tmp = (JObject)ob["CustomImage"];
                                if (!urls.Add(tmp["ImageURL"].ToString()).Equals(""))
                                {
                                    urls.Add(tmp["ImageURL"].ToString());
                                }
                                if (!tmp["ImageSecondaryURL"].ToString().Equals(""))
                                {
                                    urls.Add(tmp["ImageSecondaryURL"].ToString());
                                }
                                if (ob.ContainsKey("States"))
                                {
                                    JObject tmpO = (JObject)ob["States"];
                                    JArray tmpA = new JArray();
                                    foreach (JProperty t in tmpO.Properties())
                                    {
                                        tmpA.Add(t.First);
                                    }
                                    string[] tmp1 = FindURL(tmpA);
                                    foreach (string s in tmp1)
                                    {
                                        urls.Add(s);
                                    }
                                }
                            }
                            else if (ob["Name"].ToString().Equals("Custom_Model"))
                            {
                                JObject tmp = (JObject)ob["CustomMesh"];
                                if (!tmp["DiffuseURL"].ToString().Equals(""))
                                {
                                    urls.Add(tmp["DiffuseURL"].ToString());
                                }
                            }
                            else if (ob["Name"].ToString().Equals("Deck") ||
                                     ob["Name"].ToString().Equals("DeckCustom") ||
                                     ob["Name"].ToString().Equals("Card") ||
                                     ob["Name"].ToString().Equals("CardCustom"))
                            {
                                foreach (var x in (JObject)ob["CustomDeck"])
                                {
                                    string name = x.Key;
                                    JObject tmp = (JObject)x.Value;
                                    urls.Add(tmp["FaceURL"].ToString());
                                    urls.Add(tmp["BackURL"].ToString());
                                }
                            }
                            else if (ob["Name"].ToString().Equals("Custom_Model_Infinite_Bag") ||
                                     ob["Name"].ToString().Equals("Custom_Model_Bag"))
                            {
                                JObject tmpo = (JObject)ob["CustomMesh"];
                                if (!tmpo["DiffuseURL"].ToString().Equals(""))
                                {
                                    urls.Add(tmpo["DiffuseURL"].ToString());
                                }
                                string[] tmp = FindURL((JArray)ob["ContainedObjects"]);
                                foreach (string s in tmp)
                                {
                                    urls.Add(s);
                                }
                            }
                            else if (ob["Name"].ToString().Equals("Bag") ||
                                     ob["Name"].ToString().Equals("Infinite_Bag"))
                            {
                                string[] tmp = FindURL((JArray)ob["ContainedObjects"]);
                                foreach (string s in tmp)
                                {
                                    urls.Add(s);
                                }
                            }
                        }
                        return urls.ToArray();
                    }

                    JObject jsonO = (JObject)JToken.ReadFrom(reader);
                    
                    //System.Console.WriteLine(string.Join("\n", urls.ToArray()));
                    DataTable dt = new DataTable();

                    dt.Columns.Add("#", typeof(int));
                    dt.Columns.Add("Original", typeof(string));
                    dt.Columns.Add("New", typeof(string));
                    string[] ua = FindURL((JArray)jsonO["ObjectStates"]);
                    for (int idx = 0; idx < ua.Count(); idx++)
                    {
                        dt.Rows.Add(new string[] { idx.ToString(), ua[idx], "" });
                    }
                    URLtable.ItemsSource = dt.DefaultView;
                    URLtable.IsReadOnly = true;
                    URLtable.SelectionMode = DataGridSelectionMode.Single;
                    URLtable.Columns[0].Width = DataGridLength.SizeToCells;
                    URLtable.Columns[1].MaxWidth = 380;
                    saveOpened = true;
                }
            }
        }

        //Select mods folder
        private void Button_mods_open_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = TB_mod_folder_path.Text,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select Mods folder",
                FileName = "Select Folder"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string folderpath = System.IO.Path.GetDirectoryName(dlg.FileName);
                TB_mod_folder_path.Text = folderpath;
            }
        }

        //Update image
        private void URLtable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            Image_Original.Source = new BitmapImage();
            Image_New.Source = new BitmapImage();

            DataRowView row;
            try
            {
                row = (DataRowView)URLtable.SelectedItems[0];
            }
            catch
            {
                return;
            }
            
            try
            {
                string[] files = Directory.GetFiles(TB_mod_folder_path.Text + @"\Images\", DeleteSpecial(row["original"].ToString()) + ".*");
                Image_Original.Source = new BitmapImage(new Uri(files[0], UriKind.Absolute));
            }
            catch
            {
                Image_Original.Source = new BitmapImage();
            }

            row = (DataRowView)URLtable.SelectedItems[0];
            if (!row["New"].ToString().Equals(""))
            {
                try
                {
                    Image_New.Source = new BitmapImage(new Uri(row["New"].ToString(), UriKind.Absolute));
                }
                catch
                {
                    Image_New.Source = new BitmapImage();
                }
            }
        }

        //Add image
        private void URLtable_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (URLtable.CurrentCell.Column.Header.ToString().Equals("New"))
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".png",
                    Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                    InitialDirectory = TB_JSON_path.Text
                };

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    string filename = dlg.FileName;
                    ((DataRowView)URLtable.CurrentCell.Item)["New"] = filename;
                }
                URLtable_SelectionChanged(null, null);
            }
        }

        //Dynamic image size
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Image_Original.Height = (Stackpanel_image.ActualHeight - 26) / 2;
            Image_New.Height = (Stackpanel_image.ActualHeight - 26) / 2;
        }

        //Load work
        private void Button_load_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".tss",
                Filter = "TTS-translator save(*.tss)|*.tss",
                Title = "Load work",
                InitialDirectory = Environment.CurrentDirectory
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;
                using (StreamReader jsonSave = File.OpenText(filename))
                using (JsonTextReader reader = new JsonTextReader(jsonSave))
                {
                    JObject jsonO = (JObject)JToken.ReadFrom(reader);
                    TB_JSON_path.Text = (string)jsonO["Original JSON"];
                    TB_mod_folder_path.Text = (string)jsonO["Mods folder"];

                    DataTable dt = new DataTable();

                    dt.Columns.Add("#", typeof(int));
                    dt.Columns.Add("Original", typeof(string));
                    dt.Columns.Add("New", typeof(string));

                    JArray jArray = (JArray)jsonO["data"];
                    for (int idx = 0; idx < jArray.Count(); idx++)
                    {
                        JObject tmp = (JObject)jArray[idx];
                        dt.Rows.Add(new string[] { idx.ToString(), (string)tmp["Original"], (string)tmp["New"]});
                    }
                    URLtable.ItemsSource = dt.DefaultView;
                    URLtable.IsReadOnly = true;
                    URLtable.SelectionMode = DataGridSelectionMode.Single;
                    URLtable.Columns[0].Width = DataGridLength.SizeToCells;
                    URLtable.Columns[1].MaxWidth = 380;
                    saveOpened = true;
                }
            }
        }

        //Save current work
        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!this.saveOpened) return;
            var savejson = new JObject();
            savejson.Add("Original JSON", TB_JSON_path.Text);
            savejson.Add("Mods folder", TB_mod_folder_path.Text);
            var dttable = new JArray();
            foreach (DataRowView s in URLtable.Items)
            {
                var jo = new JObject
                {
                    { "Original", s[1].ToString() },
                    { "New", s[2].ToString() }
                };
                dttable.Add(jo);
            }

            savejson.Add("data", dttable);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = Environment.CurrentDirectory,
                Title = "Save Current Work",
                DefaultExt = ".tss",
                Filter = "TTS-translator save(*.tss)|*.tss"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                File.WriteAllText(dlg.FileName, savejson.ToString());
            }
        }

        //Backup Images
        private void Button_Backup_Images_Click(object sender, RoutedEventArgs e)
        {
            if (!this.saveOpened) return;
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = Environment.CurrentDirectory,
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Title = "Select Backup folder",
                FileName = "Select Folder"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string folderpath = System.IO.Path.GetDirectoryName(dlg.FileName);
                ProgressB.Value = 0;
                ProgressB.Maximum = URLtable.Items.Count;
                foreach (DataRowView s in URLtable.Items)
                {
                    try
                    {
                        string[] files = Directory.GetFiles(TB_mod_folder_path.Text + @"\Images\", DeleteSpecial(s["original"].ToString()) + ".*");
                        File.Copy(files[0], folderpath + @"\" + Path.GetFileName(files[0]), true);
                        ProgressB.Value += 1;
                    }
                    catch
                    {
                        ProgressB.Value += 1;
                    }
                }
            }
        }

        private void Button_Export_Click(object sender, RoutedEventArgs e)
        {
            if (!saveOpened) return;
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                InitialDirectory = TB_JSON_path.Text,
                Title = "Export JSON",
                DefaultExt = ".json",
                Filter = "TTS save(*.json)|*.json"
            };

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ProgressB.Value = 0;
                ProgressB.Maximum = URLtable.Items.Count;
                using (StreamReader jsonSave = File.OpenText(TB_JSON_path.Text))
                {
                    string saveData = jsonSave.ReadToEnd();
                    foreach (DataRowView s in URLtable.Items)
                    {
                        if(!((string)s["New"]).Equals(""))
                        {
                            saveData = saveData.Replace(s["Original"].ToString(), @"file:///" + s["New"].ToString().Replace("\\", "\\\\"));
                        }
                        ProgressB.Value += 1;
                    }
                    File.WriteAllText(dlg.FileName, saveData);
                }
            }
        }

        private void Button_Analyze_Click(object sender, RoutedEventArgs e)
        {
            if (!saveOpened) return;
            foreach (DataRowView s in URLtable.ItemsSource)
            {
                try
                {
                    string[] files = Directory.GetFiles(TB_mod_folder_path.Text + @"\Images\", DeleteSpecial(s["original"].ToString()) + ".*");
                    DataGridRow dgr = URLtable.ItemContainerGenerator.ContainerFromItem(s) as DataGridRow;
                    dgr.Background = System.Windows.Media.Brushes.Green;
                    if (files.Length == 0)
                    {
                        dgr.Background = System.Windows.Media.Brushes.Red;
                    }
                }
                catch
                {
                    DataGridRow dgr = URLtable.ItemContainerGenerator.ContainerFromItem(s) as DataGridRow;
                    dgr.Background = System.Windows.Media.Brushes.Red;
                }
            }
            
        }
    }
}
