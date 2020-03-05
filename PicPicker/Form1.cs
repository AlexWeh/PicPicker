using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace PicPicker
{
    public partial class Form1 : Form
    {
        private List<String> imageFileList = new List<String>();
        private int picCounter = 0;
        private Image currentImage;
        private System.Text.ASCIIEncoding enc = new ASCIIEncoding();
        private bool editMode = true;
        private BindingList<Label> markerList = new BindingList<Label>();
        private Bitmap marker = new Bitmap(Properties.Resources.Marker);
        private Bitmap highlightedMarker = new Bitmap(Properties.Resources.HighlightedMarker);
        private int currentMarker = 0;
        Label tempMarker;

        public Form1()
        {
            marker.MakeTransparent(marker.GetPixel(10, 10));
            highlightedMarker.MakeTransparent(highlightedMarker.GetPixel(10, 10));
            InitializeComponent();
            InitListBox();
        }

        private void InitListBox()
        {
            markerListBox.DataSource = markerList;
            markerListBox.DisplayMember = "Text";
            markerListBox.ValueMember = "Text";
            markerListBox.SelectionMode = SelectionMode.MultiExtended;

            //markerLabelTextBox.LostFocus += new EventHandler(markerLabelTextBox_LostFocus);
            markerLabelTextBox.KeyPress += new KeyPressEventHandler(markerLabelTextBox_KeyPress);
        }

        private void markerLabelTextBox_LostFocus(object sender, EventArgs e)
        {
            tempMarker.Dispose();
            markerLabelTextBox.Text = "";
            markerLabelTextBox.Visible = false;
        }

        private void markerLabelTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("Fire!");
            Console.WriteLine(e.KeyChar);
            if (e.KeyChar == (char)Keys.Enter)
            {
                tempMarker.Text = markerLabelTextBox.Text;
                markerList.Add(tempMarker);

                markerLabelTextBox.Text = "";
                markerLabelTextBox.Visible = false;
                
                currentMarker++;
                pictureBox.Focus();
                Console.WriteLine("TempMarker: " + tempMarker.Text);
                Console.WriteLine("MarkerList_Count: " + markerList.Count);
                Console.WriteLine("MarkerList: " + markerList[markerList.Count-1]);
                Console.WriteLine("--------");
                markerListBox.ClearSelected();
            }
        }
        
        private void deleteMarkerButton_Click(object sender, EventArgs e)
        {
            //TODO - Fix deletion of last element
            for (int x = markerListBox.Items.Count - 1; x >= 0; x--)
            {
                if (markerListBox.GetSelected(x) == true)
                {
                    markerList[x].Dispose();
                    markerList.RemoveAt(x);
                    currentMarker--;
                } 
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
            {
                openLoadDialog();
                waitingForOpenLabel.Visible = false;
            }
            else
            {
                if (editMode == true)
                {
                    if (tempMarker != null)
                    {
                        tempMarker.Focus();
                    }
                    tempMarker = new Label();
                    tempMarker.Size = new Size(marker.Width, marker.Height);
                    tempMarker.Parent = pictureBox;
                    tempMarker.BackColor = Color.Transparent;
                    tempMarker.Image = marker;
                    tempMarker.Location = PointToClient(new Point(Cursor.Position.X - 45, Cursor.Position.Y - 60));

                    markerLabelTextBox.Visible = true;
                    markerLabelTextBox.Location = PointToClient(new Point(Cursor.Position.X + 5, Cursor.Position.Y + 5));
                    markerLabelTextBox.Focus();
                }
            }
        }

        private void openLoadDialog()
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
                loadPicture();
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

        private void nextButton_Click(object sender, EventArgs e)
        {
            savePicture();
            picCounter++;
            loadPicture();
        }

        private void savePicture()
        {
            Console.WriteLine();
        }

        private void loadPicture()
        {
            if (picCounter >= 0 && picCounter < imageFileList.Count)
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
                if ((picCounter + 1) == imageFileList.Count)
                {
                    nextButton.Text = "Save";
                }
            }
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

        private void markerCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (markerCheckbox.Checked)
            {
                foreach (Label marker in markerList)
                {
                    marker.Visible = true;
                }
            }
            else
            {
                foreach (Label marker in markerList)
                {
                    marker.Visible = false;
                }
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openLoadDialog();
        }

        private void markerListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = markerListBox.SelectedIndex;
            foreach (Label label in markerList)
            {
                label.Image = marker;
            }
            if (index > -1)
            {
                markerList[index].Image = highlightedMarker;
            }
        }
    }
}
