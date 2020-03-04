using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PicPicker
{
    public partial class Form1 : Form
    {
        private List<String> imageFileList = new List<String>();
        private int picCounter = 0;
        private Image currentImage;
        private System.Text.ASCIIEncoding enc = new ASCIIEncoding();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        try
                        {
                            Image tryLoadingAsImage = Image.FromFile(file);
                            imageFileList.Add(file);
                        }
                        catch (Exception ex)
                        {
                            // Could not load the image - probably related to Windows file system permissions.
                            MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                                + ". You may not have permission to read the file, or " +
                                "it may be corrupt.\n\nReported error: " + ex.Message);
                        }
                    }
                    label1.Visible = false;
                    
                    loadPicture();
                }
            }
            else
            {
                Console.WriteLine(PointToClient(Cursor.Position));
                label2.Size = new Size(64, 64);
                Bitmap marker = new Bitmap(Image.FromFile("Untitled.bmp"));
                marker.MakeTransparent(marker.GetPixel(10,10));
                label2.Parent = pictureBox;
                label2.BackColor = Color.Transparent;
                label2.Image = marker;
                label2.Location = PointToClient(new Point(Cursor.Position.X - 45, Cursor.Position.Y - 60));
            }
        }

        private void loadPicture()
        {
            if (picCounter >= 0 || picCounter < imageFileList.Count)
            {
                currentImage = Image.FromFile(imageFileList[picCounter]);
                pictureBox.Image = currentImage;
                //Console.WriteLine(currentImage.GetPropertyItem(271).Type);
                
                try
                {
                    hasDescriptionMeta(currentImage.GetPropertyItem(270));
                }
                catch (Exception)
                {
                    noDescriptionMeta(currentImage.GetPropertyItem(40092));
                }
                imgCounterLabel.Text = "(" + (picCounter + 1) + "/" + imageFileList.Count + ")";
            }
        }

        private void noDescriptionMeta(PropertyItem propertyItem)
        {
            string result = "";
            string temp = enc.GetString(propertyItem.Value);
            for (int i = 0; i < temp.Length; i += 2)
            {
                result += temp[i];
            }
            this.descriptionTextBox.Text = result;
            //propertyItem.Id = 270;
            //propertyItem.Type = 2;
            //currentImage.SetPropertyItem(propertyItem);
        }

        private void hasDescriptionMeta(PropertyItem propertyItem)
        {
            descriptionTextBox.Text = enc.GetString(propertyItem.Value); 
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            picCounter++;
            loadPicture();
        }

        private void prevButton_Click(object sender, EventArgs e)
        {
            picCounter--;
            loadPicture();
        }

        private void descriptionTextBox_TextChanged(object sender, EventArgs e)
        {
            PropertyItem propertyItem = currentImage.GetPropertyItem(40092);
            Console.WriteLine(descriptionTextBox.Text);
            propertyItem.Value = enc.GetBytes(descriptionTextBox.Text);
            currentImage.SetPropertyItem(propertyItem);
        }
    }
}
