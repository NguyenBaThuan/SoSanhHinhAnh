using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace SerialPort
{
    public partial class Form1 : Form
    {

        System.Diagnostics.Process proc;

        private List<string> listPicture;
        private List<string> listResult;
        public Form1()
        {
            InitializeComponent();
            listPicture = new List<string>();
            listResult = new List<string>();
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            ptb1.Image = Image.FromFile(e.FullPath);
            listPicture.Add(e.FullPath);
            //lb.Items.Add(listPicture[listPicture.Count - 1]);
            lb.DataSource = listPicture.AsEnumerable().Reverse().ToList();

            if (listPicture.Count >= 2)
            {
                ptb2.Image = Image.FromFile(listPicture[listPicture.Count - 2]);
                if (ptb2.Image != null)
                {
                    int kq = CompareTwoImage(ptb1.Image as Bitmap, ptb2.Image as Bitmap);
                    double tile = (kq * (1.0) / 1024 * 100);
                    tile = Math.Round(tile, 2);
                    if (tile > 80)
                    {
                        listResult.Add("Tỉ lệ: " + tile + " - KQ: " + " Đứng yên" + " Time: " + DateTime.Now.ToString("hh:mm:ss"));
                        //lbCompare.Items.Add("Tỉ lệ: " + tile +  " - KQ: " + " Đứng yên" +" Time: "+DateTime.Now.Second);

                    }
                    else
                    {

                        listResult.Add("Tỉ lệ: " + tile + " - KQ: " + " Chuyển động" + " Time: " + DateTime.Now.ToString("hh:mm:ss"));
                        //lbCompare.Items.Add("Tỉ lệ: " + tile +  " - KQ: " + " Di chuyển" + " Time: " + DateTime.Now.Second);
                    }
                    var temp = listResult.AsEnumerable().Reverse();
                    lbCompare.DataSource = temp.ToList();


                
                }
            }



        }
        public static List<bool> GetHash(Bitmap bmpSource)
        {
            List<bool> lResult = new List<bool>();
            //create new image with 16x16 pixel
            Bitmap bmpMin = new Bitmap(bmpSource, new Size(32, 32));
            for (int j = 0; j < bmpMin.Height; j++)
            {
                for (int i = 0; i < bmpMin.Width; i++)
                {
                    //reduce colors to true / false                
                    lResult.Add(bmpMin.GetPixel(i, j).GetBrightness() < 0.25f);
                }
            }
            return lResult;
        }
        public int CompareTwoImage(Bitmap bm1, Bitmap bm2)
        {
            int equalElements = 0;
            List<bool> iHash1 = GetHash(bm1);
            List<bool> iHash2 = GetHash(bm2);

            //determine the number of equal pixel (x of 256)
            equalElements = iHash1.Zip(iHash2, (i, j) => i == j).Count(eq => eq);
            return equalElements;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int kq = CompareTwoImage(ptb1.Image as Bitmap, ptb2.Image as Bitmap);
            double tile = (kq * (1.0) / 1024 * 100);
            tile = Math.Round(tile, 2);
            if (tile > 80)
            {
                MessageBox.Show("Tỉ lệ: " + tile + " - Ảnh Hiện Tại: " + ptb1.Name + " - Ảnh trước: " + ptb2.Name + " - KQ: " + " Đứng yên");
            }
            else
            {
                MessageBox.Show("Tỉ lệ: " + tile + " - Ảnh Hiện Tại: " + ptb1.Name + " - Ảnh trước: " + ptb2.Name + " - KQ: " + " Chuyển động");
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                string path = @"C:\out";
                deleteFile(path);
            }
            catch (Exception)
            {

                //throw;
            }
            ExecuteCommandAsync("java code.SimpleRead");
        }

        void deleteFile(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public void ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
                procStartInfo.WorkingDirectory = @"C:\Program Files (x86)\Java\jdk1.8.0_74\bin";

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        public void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
                //Make the thread as background thread.
                objThread.IsBackground = true;
                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;
                //Start the thread.
                objThread.Start(command);
            }
            catch (ThreadStartException objException)
            {
                // Log the exception
            }
            catch (ThreadAbortException objException)
            {
                // Log the exception
            }
            catch (Exception objException)
            {
                // Log the exception
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!proc.HasExited)
            {
                proc.Kill();
            }

            killProcess();
        }
        void killProcess()
        {
            Process[] localByName = Process.GetProcessesByName("java");
            foreach (Process p in localByName)
            {
                p.Kill();
            }
        }
    }
}

