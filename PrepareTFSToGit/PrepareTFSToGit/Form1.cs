using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrepareTFSToGit
{
    public partial class Form1 : Form
    {
        delegate void AddTextCallback(string text);
        Thread workerThread = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            infoListBox.Items.Clear();
            errorListBox.Items.Clear();

            this.workerThread = new Thread(new ThreadStart(this.Run));
            this.workerThread.Start();
        }

        private void Run()
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(textBox1.Text);
                WalkDirectoryTree(dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Walks the directory tree.
        /// </summary>
        /// <param name="root">The root.</param>
        private void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            var filesToDelete = new List<FileInfo>();
            var solutionFiles = new List<FileInfo>();

            // First, process all the files directly under this folder
            try
            {
                var tmp = root.GetFiles("*.vssscc");
                filesToDelete.AddRange(tmp);

                tmp = root.GetFiles("*.vspscc");
                filesToDelete.AddRange(tmp);

                tmp = root.GetFiles("*.sln");
                solutionFiles.AddRange(tmp);
            }
            catch (Exception e)
            {
                AddError(string.Format("Error processing directory {0} {1}", root.FullName, e.Message));
            }

            // delete files
            foreach (System.IO.FileInfo file in filesToDelete)
            {
                AddInfo(string.Format("Deleting file {0}", file.FullName));
                DeleteFile(file);
            }
            filesToDelete.Clear();

            // modify solution files
            foreach (FileInfo file in solutionFiles)
            {
                AddInfo(string.Format("Modifying file {0}", file.FullName));
                ModifySolutionFile(file);
            }
            solutionFiles.Clear();

            // Now find all the subdirectories under this directory.
            var subDirs = root.GetDirectories();

            foreach (System.IO.DirectoryInfo dirInfo in subDirs)
            {
                // Resursive call for each subdirectory.
                WalkDirectoryTree(dirInfo);
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        private void DeleteFile(FileInfo file)
        {
            try
            {
                file.Delete();
            }
            catch (Exception ex)
            {
                AddError(string.Format("Unable to Delete file {0}. {1}", file.FullName, ex.Message));
            }
        }
         

        /// <summary>
        /// Handles the solution file.
        /// </summary>
        /// <param name="file">The file.</param>
        private void ModifySolutionFile(FileInfo file)
        {
            // create backup
            // TODO: Do we realy need to create backup???
            CreateBackupCopyOfSolutionFile(file);

            var tempFile = file.FullName + ".tmp";
            using (var rd = new StreamReader(file.FullName))
            {
                using (var wr = new StreamWriter(tempFile))
                {
                    string line;
                    bool flag = true;
                    while (!rd.EndOfStream)
                    {
                        line = rd.ReadLine();
                        if (line.Contains("GlobalSection(TeamFoundationVersionControl)"))
                        {
                            flag = false;
                        }

                        if (flag == false && line.Contains("EndGlobalSection"))
                        {
                            flag = true;
                            line = rd.ReadLine();
                        }

                        if (flag)
                        {
                            wr.WriteLine(line);
                        }
                    }
                }
            }

            File.Delete(file.FullName);
            File.Move(tempFile, file.FullName);
        }

        private void CreateBackupCopyOfSolutionFile(FileInfo file)
        {
            if (cbBackupSolution.Checked)
            {
                file.CopyTo(file.FullName + ".backup", true);
            }
        }

        /// <summary>
        /// Adds the message to ListBox
        /// </summary>
        /// <param name="text">The text.</param>
        private void AddInfo(string text)
        {
            if (this.infoListBox.InvokeRequired)
            {
                AddTextCallback d = new AddTextCallback(AddInfo);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.infoListBox.Items.Add(text);
            }
        }

        /// <summary>
        /// Adds the error message to error ListBox
        /// </summary>
        /// <param name="text">The text.</param>
        private void AddError(string text)
        {
            if (this.errorListBox.InvokeRequired)
            {
                AddTextCallback d = new AddTextCallback(AddError);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.errorListBox.Items.Add(text);
            }
        }

        private void cbBackupSolution_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
