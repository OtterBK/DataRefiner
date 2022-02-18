using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataRefinerModule {
    partial class DataRefinerForm {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.rich_tb_base_data = new System.Windows.Forms.RichTextBox();
            this.rich_tb_refined_data = new System.Windows.Forms.RichTextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.processTime = new System.Windows.Forms.Label();
            this.btn_refine = new System.Windows.Forms.Button();
            this.rd_address_type_reg = new System.Windows.Forms.RadioButton();
            this.rd_address_type_load = new System.Windows.Forms.RadioButton();
            this.rd_address_type_sgg = new System.Windows.Forms.RadioButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btn_open_file_dialog = new System.Windows.Forms.Button();
            this.tb_file_path = new System.Windows.Forms.TextBox();
            this.btn_refine_dataset = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rich_tb_base_data
            // 
            this.rich_tb_base_data.Location = new System.Drawing.Point(33, 27);
            this.rich_tb_base_data.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rich_tb_base_data.Name = "rich_tb_base_data";
            this.rich_tb_base_data.Size = new System.Drawing.Size(320, 339);
            this.rich_tb_base_data.TabIndex = 0;
            this.rich_tb_base_data.Text = "";
            // 
            // rich_tb_refined_data
            // 
            this.rich_tb_refined_data.Location = new System.Drawing.Point(584, 27);
            this.rich_tb_refined_data.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.rich_tb_refined_data.Name = "rich_tb_refined_data";
            this.rich_tb_refined_data.Size = new System.Drawing.Size(320, 339);
            this.rich_tb_refined_data.TabIndex = 1;
            this.rich_tb_refined_data.Text = "";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(224, 394);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(500, 17);
            this.progressBar1.TabIndex = 2;
            // 
            // processTime
            // 
            this.processTime.AutoSize = true;
            this.processTime.Location = new System.Drawing.Point(445, 85);
            this.processTime.Name = "processTime";
            this.processTime.Size = new System.Drawing.Size(53, 12);
            this.processTime.TabIndex = 3;
            this.processTime.Text = "소요시간";
            this.processTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btn_refine
            // 
            this.btn_refine.Location = new System.Drawing.Point(422, 318);
            this.btn_refine.Name = "btn_refine";
            this.btn_refine.Size = new System.Drawing.Size(90, 23);
            this.btn_refine.TabIndex = 4;
            this.btn_refine.Text = "데이터 정제";
            this.btn_refine.UseVisualStyleBackColor = true;
            this.btn_refine.Click += new System.EventHandler(this.btn_refine_Click);
            // 
            // rd_address_type_reg
            // 
            this.rd_address_type_reg.Location = new System.Drawing.Point(422, 236);
            this.rd_address_type_reg.Name = "rd_address_type_reg";
            this.rd_address_type_reg.Size = new System.Drawing.Size(47, 16);
            this.rd_address_type_reg.TabIndex = 5;
            this.rd_address_type_reg.Text = "지번";
            this.rd_address_type_reg.UseVisualStyleBackColor = true;
            // 
            // rd_address_type_load
            // 
            this.rd_address_type_load.Location = new System.Drawing.Point(422, 258);
            this.rd_address_type_load.Name = "rd_address_type_load";
            this.rd_address_type_load.Size = new System.Drawing.Size(59, 16);
            this.rd_address_type_load.TabIndex = 5;
            this.rd_address_type_load.Text = "도로명";
            this.rd_address_type_load.UseVisualStyleBackColor = true;
            // 
            // rd_address_type_sgg
            // 
            this.rd_address_type_sgg.Checked = true;
            this.rd_address_type_sgg.Location = new System.Drawing.Point(422, 280);
            this.rd_address_type_sgg.Name = "rd_address_type_sgg";
            this.rd_address_type_sgg.Size = new System.Drawing.Size(59, 16);
            this.rd_address_type_sgg.TabIndex = 5;
            this.rd_address_type_sgg.TabStop = true;
            this.rd_address_type_sgg.Text = "시군구";
            this.rd_address_type_sgg.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "csv|*.csv|모든파일|*.*";
            // 
            // btn_open_file_dialog
            // 
            this.btn_open_file_dialog.Location = new System.Drawing.Point(95, 443);
            this.btn_open_file_dialog.Name = "btn_open_file_dialog";
            this.btn_open_file_dialog.Size = new System.Drawing.Size(80, 40);
            this.btn_open_file_dialog.TabIndex = 6;
            this.btn_open_file_dialog.Text = "파일 열기";
            this.btn_open_file_dialog.UseVisualStyleBackColor = true;
            this.btn_open_file_dialog.Click += new System.EventHandler(this.btn_open_file_dialog_Click);
            // 
            // tb_file_path
            // 
            this.tb_file_path.Location = new System.Drawing.Point(224, 452);
            this.tb_file_path.Name = "tb_file_path";
            this.tb_file_path.Size = new System.Drawing.Size(500, 21);
            this.tb_file_path.TabIndex = 7;
            // 
            // btn_refine_dataset
            // 
            this.btn_refine_dataset.Location = new System.Drawing.Point(405, 348);
            this.btn_refine_dataset.Name = "btn_refine_dataset";
            this.btn_refine_dataset.Size = new System.Drawing.Size(127, 27);
            this.btn_refine_dataset.TabIndex = 8;
            this.btn_refine_dataset.Text = "데이터셋 정제";
            this.btn_refine_dataset.UseVisualStyleBackColor = true;
            this.btn_refine_dataset.Click += new System.EventHandler(this.btn_refine_dataset_Click);
            // 
            // DataRefinerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(941, 549);
            this.Controls.Add(this.btn_refine_dataset);
            this.Controls.Add(this.tb_file_path);
            this.Controls.Add(this.btn_open_file_dialog);
            this.Controls.Add(this.rich_tb_base_data);
            this.Controls.Add(this.rich_tb_refined_data);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.processTime);
            this.Controls.Add(this.btn_refine);
            this.Controls.Add(this.rd_address_type_reg);
            this.Controls.Add(this.rd_address_type_load);
            this.Controls.Add(this.rd_address_type_sgg);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DataRefinerForm";
            this.Text = "데이터 정제 테스트";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private RichTextBox rich_tb_base_data;
        private RichTextBox rich_tb_refined_data;
        private ProgressBar progressBar1;
        private Label processTime;
        private Button btn_refine;
        private RadioButton rd_address_type_reg;
        private RadioButton rd_address_type_load;
        private RadioButton rd_address_type_sgg;
        private OpenFileDialog openFileDialog1;
        private Button btn_open_file_dialog;
        private TextBox tb_file_path;
        private Button btn_refine_dataset;
    }
}