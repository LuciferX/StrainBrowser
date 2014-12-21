using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Xml.Linq;

namespace StrainBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public XDocument Xdoc;
        public ArrayList AllInfoAL = new ArrayList();
        public ArrayList BreederAL = new ArrayList();
        public ArrayList StrainAL = new ArrayList();
        public String PubBreederStr = String.Empty;
        public String PubStrainStr = String.Empty;
        public Strain ShowStrain = new Strain();
        public Loading L = new Loading();
        public System.ComponentModel.BackgroundWorker BG = new System.ComponentModel.BackgroundWorker();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            L.Show();

            TextBlock1.Text = "";
            TextBlock2.Text = "";
            TextBlock3.Text = "";
            TextBlock4.Text = "";
            TextBlock5.Text = "";
            TextBlock6.Text = "";
            FlowDocument FD = new FlowDocument();
            Paragraph P = new Paragraph();
            P.Inlines.Add(new Run(""));
            FD.Blocks.Add(P);
            InfoRTB.Document = FD;

            BG.WorkerSupportsCancellation = true;
            BG.DoWork += BG_DoWork;
            BG.RunWorkerCompleted += BG_RunWorkerCompleted;
     
            InfoRTB.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            System.ComponentModel.BackgroundWorker InitBG = new System.ComponentModel.BackgroundWorker();
            InitBG.DoWork += InitBG_DoWork;
            InitBG.RunWorkerCompleted += InitBG_RunWorkerCompleted;
            InitBG.RunWorkerAsync();
        }

        void InitBG_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            foreach (String thisBreeder in BreederAL)
            {
                BreederListBox.Items.Add(thisBreeder);
            }

            BreederListBox.SelectionChanged += BreederListBox_SelectionChanged;
            StrainListBox.SelectionChanged += StrainListBox_SelectionChanged;

            L.Close();

            BreederListBox.SelectedIndex = 0;
        }

        void InitBG_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Xdoc = XDocument.Load("PLANTCACHE.XML");
            var query = from p in Xdoc.Descendants()
                        select p;
            foreach (XElement Xl in query)
            {
                foreach (XElement Xll in Xl.Descendants())
                {
                    try
                    {
                        String Name = Xll.Element("Name").Value;
                        String Type = Xll.Element("Type").Value;
                        String pic = Xll.Element("pic").Value;
                        String desc = Xll.Element("desc").Value;
                        String auto = Xll.Element("auto").Value;
                        String days = Xll.Element("days").Value;
                        String info = Xll.Element("info").Value;
                        String BreederName = Xll.Element("BreederName").Value;

                        Strain S = new Strain();
                        S.Name = Name;
                        S.Type = Type;
                        S.pic = pic;
                        S.desc = desc;
                        S.auto = auto;
                        S.days = days;
                        S.info = info;
                        S.BreederName = BreederName;
                        AllInfoAL.Add(S);

                        if (BreederAL.Contains(BreederName) == false)
                        {
                            BreederAL.Add(BreederName);
                        }
                    }
                    catch
                    { }
                }
            }

            BreederAL.Sort();
        }

        void StrainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StrainListBox.SelectedValue != null)
            {
                PubStrainStr = StrainListBox.SelectedValue.ToString();
                System.ComponentModel.BackgroundWorker BG2 = new System.ComponentModel.BackgroundWorker();
                BG2.DoWork += BG2_DoWork;
                BG2.RunWorkerCompleted += BG2_RunWorkerCompleted;
                BG2.RunWorkerAsync();
            }
        }

        void BG2_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            TextBlock1.Text = ShowStrain.BreederName;
            TextBlock4.Text = ShowStrain.days;
            FlowDocument FD = new FlowDocument();
            Paragraph P = new Paragraph();
            P.Inlines.Add(new Run(ShowStrain.desc.Replace("<br />", "\r\n").Replace("<strong>", "").Replace("</strong>", "").Replace("<b>", "").Replace("</b>", "")));
            FD.Blocks.Add(P);
            InfoRTB.Document = FD;
            TextBlock5.Text = ShowStrain.auto;
            TextBlock6.Text = ShowStrain.info;
            TextBlock2.Text = ShowStrain.Name;
            TextBlock3.Text = ShowStrain.Type;
            if (File.Exists("pics\\" + ShowStrain.pic))
            {
                try
                {
                    ImageObject.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\pics\\" + ShowStrain.pic));
                    picpath.Text = Environment.CurrentDirectory + "\\pics\\" + ShowStrain.pic;
                }
                catch
                {
                    ImageObject.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\pics\\blank.jpg"));
                    picpath.Text = Environment.CurrentDirectory + "\\pics\\blank.jpg";
                }
            }
            else
            {
                ImageObject.Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "\\pics\\blank.jpg"));
                picpath.Text = Environment.CurrentDirectory + "\\pics\\" + ShowStrain.pic;
            }
        }

        void BG2_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var query = from Strain S in AllInfoAL
                        where S.BreederName == PubBreederStr && S.Name == PubStrainStr
                        select S;
            foreach (Strain S in query)
            {
                ShowStrain.BreederName = S.BreederName;
                ShowStrain.days = S.days;
                ShowStrain.desc = S.desc;
                ShowStrain.auto = S.auto;
                ShowStrain.info = S.info;
                ShowStrain.Name = S.Name;
                ShowStrain.pic = S.pic;
                ShowStrain.Type = S.Type;
            }
        }

  
        void BreederListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            StrainListBox.Items.Clear();
            StrainAL.Clear();

            PubBreederStr = BreederListBox.SelectedValue.ToString();
  
            if (BG.IsBusy == false)
            BG.RunWorkerAsync();
        }

        private void BG_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if (StrainAL != null)
            {
                foreach (String thisStrain in StrainAL)
                {
                    StrainListBox.Items.Add(thisStrain);
                }
            }

            if(StrainListBox.Items.Count > 0)
            StrainListBox.SelectedIndex = 0;
        }

        void BG_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var query = from Strain S in AllInfoAL
                        orderby S.Name
                        where S.BreederName == PubBreederStr
                        select S;
            foreach(Strain S in query)
            {
                StrainAL.Add(S.Name);
            }
        }

        private void ImageObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(picpath.Text);
            }
            catch { }
        }

        private void picpath_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start(picpath.Text);
            }
            catch { }
        }
    }

    public class Strain
    {
        public String Name { get; set; }
        public String Type { get; set; }
        public String pic { get; set; }
        public String desc { get; set; }
        public String auto { get; set; }
        public String days { get; set; }
        public String info { get; set; }
        public String BreederName { get; set; }
    }
}
