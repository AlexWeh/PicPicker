using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;

namespace PicPicker
{
    public partial class MainWindow : Form
    {
        private int picCounter = 0;
        private int currentMarker = 0;

        private List<String> imageFileList = new List<String>();
        private BindingList<Label> markerList = new BindingList<Label>();

        private Bitmap marker = new Bitmap(Properties.Resources.Marker);
        private Bitmap highlightedMarker = new Bitmap(Properties.Resources.HighlightedMarker);

        private Image currentImage;
        private System.Text.ASCIIEncoding enc = new ASCIIEncoding();
        private bool editMode = true;
        private Label tempMarker;

        public MainWindow()
        {
            InitPreComponents();
            InitializeComponent();
            InitListBox();
        }

        private void InitPreComponents() {
            marker.MakeTransparent(marker.GetPixel(marker.Width / 2, marker.Height / 2));
            highlightedMarker.MakeTransparent(highlightedMarker.GetPixel(highlightedMarker.Width / 2, highlightedMarker.Height / 2));
        }

        private void InitListBox()
        {
            markerListBox.DataSource = markerList;
            markerListBox.DisplayMember = "Text";
            markerListBox.ValueMember = "Text";

            markerLabelTextBox.KeyPress += new KeyPressEventHandler(markerLabelTextBox_KeyPress);
        }

        private void markerLabelTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                if (markerLabelTextBox.Text != "")
                {
                    tempMarker.Text = markerLabelTextBox.Text;
                    markerList.Add(tempMarker);
                    tempMarker = null;

                    markerLabelTextBox.Text = "";
                    markerLabelTextBox.Visible = false;
                    
                    pictureBox.Focus();
                    markerListBox.ClearSelected();

                    currentMarker++;
                }
            }
            else if (e.KeyChar == (char)Keys.Escape)
            {
                unselectTextbox();
            }
        }

        private void unselectTextbox()
        {
            tempMarker.Dispose();
            tempMarker = null;
            markerLabelTextBox.Text = "";
            markerLabelTextBox.Visible = false;
        }

        private void deleteMarkerButton_Click(object sender, EventArgs e)
        {

            markerList[markerListBox.SelectedIndex].Dispose();
            markerList.RemoveAt(markerListBox.SelectedIndex);
            currentMarker--;
        }

        private void pictureBox_Click(object sender, EventArgs e)
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
                        tempMarker.Dispose();
                        tempMarker = null;
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
            //stackoverflow.com_questions_48716515_how-to-get-real-image-pixel-point-x-y-from-picturebox
            Int32 realW = pictureBox.Image.Width;
            Int32 realH = pictureBox.Image.Height;
            Int32 currentW = pictureBox.ClientRectangle.Width;
            Int32 currentH = pictureBox.ClientRectangle.Height;
            Double zoomW = (currentW / (Double)realW);
            Double zoomH = (currentH / (Double)realH);
            Double zoomActual = Math.Min(zoomW, zoomH);
            Double padX = zoomActual == zoomW ? 0 : (currentW - (zoomActual * realW)) / 2;
            Double padY = zoomActual == zoomH ? 0 : (currentH - (zoomActual * realH)) / 2;

            for (int i = 0; i < markerList.Count; i++)
            {
                Int32 realX = (Int32)(((markerList[i].Location.X + 32) - padX) / zoomActual);
                Int32 realY = (Int32)(((markerList[i].Location.Y + 32) - padY) / zoomActual);

                String PosXval = realX < 0 || realX > realW ? "-" : realX.ToString();
                String PosYval = realY < 0 || realY > realH ? "-" : realY.ToString();
                Console.WriteLine("X: " + PosXval + " //Y: " + PosYval);
            }
            
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
