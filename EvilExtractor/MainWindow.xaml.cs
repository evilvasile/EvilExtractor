using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace EvilExtractor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private Regex _filterRegex = new Regex(@"^\*\.\w+");
        private Thread _jobThread = null;
        private bool _terminateJobRequest = false;
        private bool _debugStrings = false;

        public MainWindow() {
            InitializeComponent();
        }

        #region GUI Button Events

        private void buttonBrowseTool_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Multiselect = false;
            ofd.Filter = "Executable (*.exe)|*.exe";

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                this.textBoxToolPath.Text = ofd.FileName;
            }

        }

        private void buttonBrowseInput_Click(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                this.textBoxInputPath.Text = fbd.SelectedPath;
            }
        }

        private void buttonBrowseOutput_Click(object sender, RoutedEventArgs e) {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                this.textBoxOutputPath.Text = fbd.SelectedPath;
            }
        }

        private void buttonCommand_Click(object sender, RoutedEventArgs e) {
            if (this._jobThread == null) {

                this.buttonBrowseInput.IsEnabled = false;
                this.buttonBrowseOutput.IsEnabled = false;
                this.buttonBrowseTool.IsEnabled = false;
                this.textBoxInputFileExtension.IsEnabled = false;
                this.textBoxOutputFileExtension.IsEnabled = false;
                this.textBoxLog.Clear();

                // save strings to new variables so they can be passed to job thread
                string inExtension = this.textBoxInputFileExtension.Text;
                string outExtension = this.textBoxOutputFileExtension.Text;
                string toolPath = this.textBoxToolPath.Text;
                string inPath = this.textBoxInputPath.Text;
                string outPath = this.textBoxOutputPath.Text;

                // reset job termination flag
                this._terminateJobRequest = false;

                // prepare and start job thread
                this._jobThread = new Thread(() => this.jobMainMethod(inExtension, outExtension, toolPath, inPath, outPath));
                this._jobThread.IsBackground = true;
                this._jobThread.Start();

                // set button text to cancel
                this.buttonCommand.Content = "Cancel";
            } else {
                // if thread exists, then send termination request instead
                // disable button since the request has been logged already
                this._terminateJobRequest = true;
                this.buttonCommand.IsEnabled = false;
            }
        }

        #endregion

        #region GUI Thread Safe control updates

        private void jobFinishButtonReset() {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.buttonBrowseInput.IsEnabled = true;
                this.buttonBrowseOutput.IsEnabled = true;
                this.buttonBrowseTool.IsEnabled = true;
                this.textBoxInputFileExtension.IsEnabled = true;
                this.textBoxOutputFileExtension.IsEnabled = true;
                this.buttonCommand.IsEnabled = true;
                this.buttonCommand.Content = "Start";
            }));
        }

        private void writeToLog(string str) {
            this.Dispatcher.Invoke(new Action(() => {
                this.textBoxLog.AppendText(str);
                this.textBoxLog.ScrollToEnd();
            }));
        }

        private void writeLineToLog(string str) {
            this.Dispatcher.Invoke(new Action(() => {
                this.textBoxLog.AppendText(str + Environment.NewLine);
                this.textBoxLog.ScrollToEnd();
            }));
        }

        private void setProgressBar(double min, double max, double value) {
            this.Dispatcher.Invoke(new Action(() => {
                this.progressBarJob.Minimum = min;
                this.progressBarJob.Maximum = max;
                this.progressBarJob.Value = value;
            }));
        }

        #endregion

        #region Job thread methods
        private bool checkFilterExtension(string inExtension, string outExtension) {
            this.writeToLog("> Checking input file extension... ");
            if (this._filterRegex.IsMatch(inExtension)) {
                this.writeLineToLog("OK!");
            } else {
                this.writeLineToLog("Invalid!");

                // bad filter, can not continue
                System.Windows.MessageBox.Show(
                    "Invalid input file extension: \"" + inExtension + "\" ",
                    "Oups!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            this.writeToLog("> Checking output file extension... ");
            if (this._filterRegex.IsMatch(outExtension)) {
                this.writeLineToLog("OK!");
            } else {
                this.writeLineToLog("Invalid!");

                // bad filter, can not continue
                System.Windows.MessageBox.Show(
                    "Invalid output file extension: \"" + outExtension + "\" ",
                    "Oups!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        // Checks if tool file exists
        private bool checkToolFile(string toolFile) {
            this.writeToLog("> Checking Tool File... ");
            if (File.Exists(toolFile)) {
                this.writeLineToLog("OK!");
            } else {
                this.writeLineToLog("File not found!");

                System.Windows.MessageBox.Show(
                    "Oups! Tool file not found at \"" + toolFile + "\"",
                    "Oups!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        // checks if input folder exists
        private bool checkInputFolder(string inputFolder) {
            this.writeToLog("> Checking Input Folder... ");
            if (Directory.Exists(inputFolder)) {
                this.writeLineToLog("OK!");
            } else {
                this.writeLineToLog("Folder not found!");

                System.Windows.MessageBox.Show(
                    "Oups! Folder not found at \"" + inputFolder + "\"",
                    "Oups!",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return false;
            }

            return true;
        }

        private bool checkOutputFolder(string outputFolder) {
            this.writeToLog("> Checking Output Folder... ");
            if (Directory.Exists(outputFolder)) {
                this.writeLineToLog("OK!");
            } else {
                // try to create output folder
                this.writeLineToLog("Folder not found!");
                this.writeToLog("> Creating Output Folder... ");

                bool ok = true;
                try {
                    if (Directory.CreateDirectory(outputFolder).Exists) {
                        this.writeLineToLog("OK!");
                    } else {
                        ok = false;

                    }
                } catch (Exception) {
                    ok = false;
                }

                if (!ok) {

                    this.writeLineToLog("Could not create output folder!");

                    System.Windows.MessageBox.Show(
                        "Oups! Could not create output folder at \"" + outputFolder + "\"",
                        "Oups!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        private void notifyTermination() {
            // user abort...
            this.writeLineToLog("");
            this.writeLineToLog("> Operation aborted by user.");

            // job done
            System.Windows.MessageBox.Show(
                "Operation aborted!",
                "Awww!",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void applyTool(string[] fileList, string inExtension, string outExtension, string toolFile, string inputFolder, string outputFolder) {
            // write all the found files in the log, but only if debug strings are on
            if (this._debugStrings) {
                this.writeLineToLog("");
                this.writeLineToLog("> Dumping file list...");
                for (int idx = 0; idx < fileList.Length; idx++) {
                    this.writeLineToLog(fileList[idx]);
                }
            }

            

            // some cleanup...
            inExtension = inExtension.Replace("*","");
            outExtension = outExtension.Replace("*", "");
            while (outputFolder.EndsWith("\\")) {
                outputFolder = outputFolder.Substring(0, outputFolder.Length - 1);
            }

            for (int idx = 0; idx < fileList.Length; idx++) {
                // check for termination
                if (this._terminateJobRequest) {
                    this.notifyTermination();
                    return;
                }

                // update progress bar
                this.setProgressBar(0, fileList.Length -1, idx);

                //
                this.writeLineToLog("");
                this.writeLineToLog("> Processing file " + (idx + 1).ToString() + "/" + fileList.Length.ToString());
                this.writeLineToLog(fileList[idx]);

                string inFile = fileList[idx];
                string outFile = inFile.Replace(inputFolder,outputFolder);

                // replace file extension...
                outFile = outFile.Replace(inExtension, outExtension);

                //check output folder is legit
                string outDir = System.IO.Path.GetDirectoryName(outFile);
                if (!Directory.Exists(outDir)) {
                    try {
                        Directory.CreateDirectory(outDir);
                    } catch (Exception) {
                        // I guess i'll just let the tool fail if I can't create the subdirectory?...
                    }
                }

                Process fileProcess = new Process() {
                    StartInfo = new ProcessStartInfo() {
                        FileName = toolFile,
                        Arguments = "\"" + inFile + "\" \"" + outFile + "\"",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    }
                };                
                fileProcess.OutputDataReceived += (sender, args) => this.writeToLog(args.Data);
                fileProcess.Start();
                fileProcess.BeginOutputReadLine();
                fileProcess.BeginErrorReadLine();

                while (!fileProcess.HasExited) {
                    // check if process done
                    fileProcess.Refresh();

                    // check termination request
                    if (this._terminateJobRequest) {
                        // *teleports behind process* nuthin' personel kid
                        fileProcess.Kill();

                        // check for termination
                        if (this._terminateJobRequest) {
                            this.notifyTermination();
                            return;
                        }
                    }
                    // sleep 10 miliseconds
                    Thread.Sleep(10);
                }
            }

            // job done
            this.writeLineToLog("");
            this.writeLineToLog("");
            this.writeLineToLog("");
            this.writeLineToLog("> Job done!...");
            this.writeLineToLog("");
            System.Windows.MessageBox.Show(
                "Operation complete!",
                "Yay!",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void jobMainMethod(string inputExtension, string outputExtension, string toolFile, string inputFolder, string outputFolder) {
            try {
                this.writeLineToLog("> Starting Operation...");

                // check target file extension
                if (!this.checkFilterExtension(inputExtension, outputExtension)) {
                    return;
                }

                // check tool file exists
                if (!this.checkToolFile(toolFile)) {
                    return;
                }

                // check input folder exists
                if (!this.checkInputFolder(inputFolder)) {
                    return;
                }

                // check output folder exists
                if (!this.checkOutputFolder(outputFolder)) {
                    return;
                }

                // get all the files with the target extension, hmm, this may take a while I guess?...
                this.writeLineToLog("> Scanning all files with given extension...");
                string[] allTheFiles = Directory.GetFiles(inputFolder,inputExtension, SearchOption.AllDirectories);
                this.writeLineToLog("> Found " + allTheFiles.Length.ToString() + " files!");

                if (allTheFiles.Length > 0) {
                    if (System.Windows.MessageBox.Show(
                        "Found " + allTheFiles.Length.ToString() + " files with the \"" + inputExtension + "\" extension!" + Environment.NewLine + "Continue?...", 
                        "Confirmation", 
                        MessageBoxButton.OKCancel, 
                        MessageBoxImage.Question) == MessageBoxResult.OK) {

                        // execute tool stuff
                        this.applyTool(allTheFiles, inputExtension, outputExtension, toolFile, inputFolder, outputFolder);
                    } else {
                        // user abort...
                        this.writeLineToLog("");
                        this.writeLineToLog("> Operation aborted by user.");
                    }
                } else {
                    // no files found, wew lad
                    System.Windows.MessageBox.Show(
                        "No files were found with the \"" + inputExtension + "\" extension!", 
                        "Oups!", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Warning);
                }


            } catch(Exception ex) {
                this.writeLineToLog("");
                this.writeLineToLog("> ERROR! Unexpected exception!... Operation can not continue.");
                this.writeLineToLog("> Dumping exception text...");
                this.writeLineToLog(ex.Message);
                this.writeLineToLog("");
            }
            finally {
                // make sure buttons are reset when thread is done or crashes or whatever
                this.jobFinishButtonReset();
                this._jobThread = null;
            }
        }

        #endregion
    }
}
