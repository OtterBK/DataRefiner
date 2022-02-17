
using DataRefinerModule.Refiner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace DataRefinerModule {
    public partial class DataRefinerForm : Form {

        public static int addressType = 0;
        private string filePath;

        public DataRefinerForm() {
            InitializeComponent();
        }

        private void btn_refine_Click(object sender, EventArgs e) {

            Refine();

        }

        private void Refine() {

            if (rd_address_type_reg.Checked)
                addressType = 0;
            else if (rd_address_type_load.Checked)
                addressType = 1;
            else if (rd_address_type_sgg.Checked)
                addressType = 2;

            DataRefiner dataRefiner = new DataRefiner();

            string dataListString = rich_tb_base_data.Text;
            string[] dataList = dataListString.Split('\n');

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string resultString = string.Empty;
            foreach (string data in dataList) {
                string baseData = data.Trim();
                if (baseData == string.Empty) {
                    resultString += "\n";
                    continue;
                }
                RefinedData refinedData = dataRefiner.DataRefine(data);

                if(refinedData.DataType != DataType.UNKNOWN && refinedData.DataType != DataType.TEXT) {
                    resultString += $"[S]{refinedData.Refined}";
                    if (refinedData.AdditionalData != null) {
                        foreach (RefinedData additionalData in refinedData.AdditionalData) {
                            resultString += " / " + additionalData.Refined;
                        }
                    }
                } else {
                    resultString += $"[F]{baseData}"; ;
                }

                resultString += "\n";
            }

            stopwatch.Stop();

            rich_tb_refined_data.Text = resultString;

            processTime.Text = "소요시간\n"+stopwatch.ElapsedMilliseconds + "ms";
        }

        private void btn_open_file_dialog_Click(object sender, EventArgs e) {

            string filePath = null;
            openFileDialog1.InitialDirectory = "C:\\";

            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                filePath = openFileDialog1.FileName;
                tb_file_path.Text = filePath;
                this.filePath = filePath;
            }

        }

        private void btn_refine_dataset_Click(object sender, EventArgs e) {

            string filePath = tb_file_path.Text;
            if(filePath == string.Empty) {
                MessageBox.Show("csv 파일을 먼저 선택하세요.");
            } else {
                //BackgroundWorker backgroundWorker = new BackgroundWorker();
                //backgroundWorker.DoWork += new DoWorkEventHandler(DataSetRefineWorker);
                //backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DataSetRefineWorkerCompleted);

                DataSetRefine(this.filePath);
            }

        }

        private void DataSetRefine(string filePath) {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<string[]> refinedRows = null;
            string columnLine = string.Empty;

            using (StreamReader sr = new StreamReader(filePath, System.Text.Encoding.GetEncoding("euc-kr"))) {
                columnLine = sr.ReadLine();

                DataSetRefiner dataSetRefiner = new DataSetRefiner();

                string[] fields = columnLine.Split(',');
                foreach (string field in fields) {
                    dataSetRefiner.AddFields(field);
                }

                List<string[]> rows = new List<string[]>();
                while (!sr.EndOfStream) {

                    string rowLine = sr.ReadLine();
                    string[] values = rowLine.Split(',');
                    dataSetRefiner.AddRows(values);
                }

                progressBar1.Minimum = 0;
                progressBar1.Maximum = rows.Count;
                progressBar1.Step = 1;

                refinedRows = dataSetRefiner.RefineDataSet();
            }

            stopwatch.Stop();

            if (refinedRows != null) {
                filePath = filePath.Replace(".csv", "_refined.csv");

                using (StreamWriter file = File.CreateText(filePath)) {
                    file.WriteLine(columnLine);

                    foreach (string[] row in refinedRows) {
                        foreach (string value in row) {
                            file.Write(value);
                        }
                        file.WriteLine("\r\n");
                    }

                    file.Flush();
                }

                processTime.Text = "소요시간\n" + stopwatch.ElapsedMilliseconds + "ms\n성공";
            }
        }

        private void DataSetRefineWorker(object sender, DoWorkEventArgs e) {

            DataSetRefine(this.filePath);
        }

        private void DataSetRefineWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            string time = e.Result.ToString();
            processTime.Text = "소요시간\n" + time + "ms\n";
        }
    }
}