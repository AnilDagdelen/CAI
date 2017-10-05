using Microsoft.CognitiveServices.SpeechRecognition;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Microsoft.Cognitive.LUIS;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System.Threading;
using System.Windows.Media;
using System;
using System.Windows.Media.Animation;
using System.IO.Compression;

namespace WpfApplication2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Moco"))
                System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Moco");
        }
        CloudBlockBlob blockBlob;
        #region Microphone recognition
        private MicrophoneRecognitionClient micClient;
        private void CreateMicrophoneRecoClient()
        {
            this.micClient = SpeechRecognitionServiceFactory.CreateMicrophoneClient(
                SpeechRecognitionMode.ShortPhrase,
                "en-US",
                "ca07f01db0c54f1b9758ec0f5184f5a6");

            // Event handlers for speech recognition results
            this.micClient.OnMicrophoneStatus += this.OnMicrophoneStatus;
            this.micClient.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            this.micClient.OnResponseReceived += this.OnMicLongPhraseResponseReceivedHandler;
            this.micClient.OnConversationError += this.OnConversationErrorHandler;


        }
        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            this.WriteLine("Error text: {0}", e.SpeechErrorText);
        }
        private void WriteLine(string format, params object[] args)
        {
            var formattedStr = string.Format(format, args);
            Dispatcher.Invoke(() =>
            {
                _logText.Content = (formattedStr);
            });
        }
        private void OnMicLongPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                this.micClient.EndMicAndRecognition();
                if (e.PhraseResponse.Results.Length > 0)
                {
                    WriteLine(e.PhraseResponse.Results[0].DisplayText);
                    LUISPrediction(e.PhraseResponse.Results[0].DisplayText);
                }
                //this.WriteResponseResult(e);
            });
        }
        //private void WriteResponseResult(SpeechResponseEventArgs e)
        //{
        //    if (e.PhraseResponse.Results.Length == 0)
        //    {
        //        this.WriteLine("No phrase response is available.");
        //    }
        //    else
        //    {
        //        this.WriteLine("********* Final n-BEST Results *********");
        //        for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
        //        {
        //            this.WriteLine(
        //                "[{0}] Confidence={1}, Text=\"{2}\"",
        //                i,
        //                e.PhraseResponse.Results[i].Confidence,
        //                e.PhraseResponse.Results[i].DisplayText);
        //        }

        //    }
        //}
        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            this.WriteLine("{0}", e.PartialResult);
        }
        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (e.Recording)
                {
                    WriteLine("Please start speaking.");
                    System.Media.SoundPlayer player = new System.Media.SoundPlayer(@"E:\SystemFolders\documents\visual studio 2015\Projects\WpfApplication2\WpfApplication2\readysound.wav");
                    player.Play();
                }
            });
        }
        #endregion
        async void LUISPrediction(string query)
        {
            Microsoft.Cognitive.LUIS.LuisClient LUISC = new Microsoft.Cognitive.LUIS.LuisClient("0a45def3-dc76-4224-a8b2-140a583c5347", "6be6ec11807e4768ae1f870b6db99be6");
            LuisResult r = await LUISC.Predict(query);
            Color s = ((SolidColorBrush)GridBG.Background).Color;
            float score = (float)(r.Intents[0].Score);
            byte R = 255, G = 255;
            if (score > 0.5f)
                R -= (byte)(255 * score);
            else
                G -= (byte)(255 * score);
            this.Dispatcher.Invoke(() =>
            {
                ColorAnimation ca = new ColorAnimation(Color.FromRgb(R, G, 0), new Duration(TimeSpan.FromSeconds(6)));
                GridBG.Background = new SolidColorBrush(s);
                GridBG.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                ButtonControl.Background = new SolidColorBrush(s);
                ButtonControl.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);

            });

            GetIntentFromKnowledge("Win" + r.Intents[0].Name);
            string Arguments = "";
            if (r.Entities.Count > 0)
            {
                var sa = r.Entities.Values.ToList();
                Arguments = sa[0][0].Value + "|******|" + sa[0][0].Name;
                for (int i = 1; i < r.Entities.Values.Count; i++)
                {
                    Arguments += "|------|" + sa[i][0].Value + "|******|" + sa[i][0].Name;
                }
                Arguments = Arguments.Replace(" ", "|++++++|");
            }
            ExecuteFromAssemblyIntent(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Moco" + "\\Win" + r.Intents[0].Name + "\\main.exe", Arguments);

        }
        private void GetIntentFromKnowledge(string Intent)
        {
            #region AzureBlobStorage

            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=projectcody;AccountKey=R2IB8OU2nCBpjj0IRyBuK/1ngUTo6/kuYZs2lcYA50xJ4GSe5A/DsPkaCU3tpoWNyhVhBe45+an/hVAGY/AGdQ==;");

            //CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //CloudBlobContainer container = blobClient.GetContainerReference(Intent.ToLower());
            //IEnumerable<IListBlobItem> ListBlobs = container.ListBlobs(string.Empty, true);



            //foreach (CloudBlockBlob item in ListBlobs)
            //{
            //    string name = item.Name;
            //    blockBlob = container.GetBlockBlobReference(name);
            //    blockBlob.FetchAttributes();
            //    string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Moco" + "\\" + name;
            //    if (!File.Exists(path) || (blockBlob.Properties.Length != new FileInfo(path).Length))
            //        blockBlob.DownloadToFile(path, FileMode.OpenOrCreate);
            //}
            #endregion

            ZipArchiveExtensions.ExtractToDirectory(ZipFile.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Moco" + "\\" + Intent + ".zip"), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\" + "Moco", true);


        }
        private void ExecuteFromAssemblyIntent(string assembly, string arguments)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = assembly,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            try
            {
                proc.Start();
            }
            catch (Exception)
            {
                WriteLine("Module couldn't be started.");
            }
            string ret = "";
            while (!proc.StandardOutput.EndOfStream)
                ret = proc.StandardOutput.ReadLine();
            WriteLine(ret);
            if (t != null)
                t.Abort();
            t = null;
            Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Microphone;
        }
        Thread t;
        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (!SnackbarFive.IsActive)
                SnackbarFive.IsActive = !SnackbarFive.IsActive;
            if (t == null)
            {
                Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.MicrophoneOff;
                t = new Thread(delegate ()
                {
                    WriteLine("Connecting to speech service ...");
                    this.CreateMicrophoneRecoClient();
                    this.micClient.StartMicAndRecognition();
                });
                t.Start();
            }
            else
            {
                micClient.Dispose();
                WriteLine("Canceled ...");
                Icon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Microphone;
                micClient.EndMicAndRecognition();
                t.Abort();
                t = null;
            }
        }

    }
}
