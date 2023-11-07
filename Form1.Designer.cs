namespace SF2MConfigRewriteV2
{
	partial class FormMain
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			openFileDialog1 = new OpenFileDialog();
			rewriteButton = new Button();
			configsList = new ListBox();
			openButton = new Button();
			textBox1 = new TextBox();
			progressBox = new TextBox();
			clearButton = new Button();
			SuspendLayout();
			// 
			// openFileDialog1
			// 
			openFileDialog1.FileName = "openFileDialog1";
			// 
			// rewriteButton
			// 
			rewriteButton.BackgroundImageLayout = ImageLayout.Center;
			rewriteButton.FlatAppearance.BorderColor = Color.White;
			rewriteButton.FlatAppearance.BorderSize = 3;
			rewriteButton.FlatStyle = FlatStyle.Flat;
			rewriteButton.Font = new Font("Segoe UI", 27.75F, FontStyle.Bold, GraphicsUnit.Point);
			rewriteButton.ForeColor = Color.White;
			rewriteButton.Location = new Point(405, 198);
			rewriteButton.Name = "rewriteButton";
			rewriteButton.Size = new Size(184, 96);
			rewriteButton.TabIndex = 0;
			rewriteButton.Text = "Rewrite";
			rewriteButton.UseVisualStyleBackColor = true;
			rewriteButton.Click += rewriteButton_Click;
			// 
			// configsList
			// 
			configsList.BackColor = Color.Black;
			configsList.ForeColor = Color.White;
			configsList.FormattingEnabled = true;
			configsList.ItemHeight = 15;
			configsList.Location = new Point(27, 96);
			configsList.Name = "configsList";
			configsList.Size = new Size(372, 334);
			configsList.TabIndex = 2;
			// 
			// openButton
			// 
			openButton.BackgroundImageLayout = ImageLayout.Center;
			openButton.FlatAppearance.BorderColor = Color.White;
			openButton.FlatAppearance.BorderSize = 3;
			openButton.FlatStyle = FlatStyle.Flat;
			openButton.Font = new Font("Segoe UI", 27.75F, FontStyle.Bold, GraphicsUnit.Point);
			openButton.ForeColor = Color.White;
			openButton.Location = new Point(405, 96);
			openButton.Name = "openButton";
			openButton.Size = new Size(184, 96);
			openButton.TabIndex = 3;
			openButton.Text = "Open";
			openButton.UseVisualStyleBackColor = true;
			openButton.Click += openButton_Click;
			// 
			// textBox1
			// 
			textBox1.BackColor = Color.Black;
			textBox1.BorderStyle = BorderStyle.None;
			textBox1.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
			textBox1.ForeColor = Color.White;
			textBox1.Location = new Point(405, 404);
			textBox1.Name = "textBox1";
			textBox1.Size = new Size(356, 27);
			textBox1.TabIndex = 4;
			textBox1.Text = "Back up your config files just in case";
			// 
			// progressBox
			// 
			progressBox.BackColor = Color.Black;
			progressBox.BorderStyle = BorderStyle.None;
			progressBox.Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
			progressBox.ForeColor = Color.White;
			progressBox.Location = new Point(405, 63);
			progressBox.Name = "progressBox";
			progressBox.ReadOnly = true;
			progressBox.Size = new Size(356, 27);
			progressBox.TabIndex = 5;
			// 
			// clearButton
			// 
			clearButton.BackgroundImageLayout = ImageLayout.Center;
			clearButton.FlatAppearance.BorderColor = Color.White;
			clearButton.FlatAppearance.BorderSize = 3;
			clearButton.FlatStyle = FlatStyle.Flat;
			clearButton.Font = new Font("Segoe UI", 27.75F, FontStyle.Bold, GraphicsUnit.Point);
			clearButton.ForeColor = Color.White;
			clearButton.Location = new Point(405, 302);
			clearButton.Name = "clearButton";
			clearButton.Size = new Size(184, 96);
			clearButton.TabIndex = 6;
			clearButton.Text = "Clear";
			clearButton.UseVisualStyleBackColor = true;
			clearButton.Click += clearButton_Click;
			// 
			// FormMain
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			BackColor = Color.Black;
			ClientSize = new Size(800, 450);
			Controls.Add(clearButton);
			Controls.Add(progressBox);
			Controls.Add(textBox1);
			Controls.Add(openButton);
			Controls.Add(configsList);
			Controls.Add(rewriteButton);
			Icon = (Icon)resources.GetObject("$this.Icon");
			Name = "FormMain";
			Text = "SF2M Config Rewriter";
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private OpenFileDialog openFileDialog1;
		private Button rewriteButton;
		private ListBox configsList;
		private Button openButton;
		private TextBox textBox1;
		private TextBox progressBox;
		private Button clearButton;
	}
}