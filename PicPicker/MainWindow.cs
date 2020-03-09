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

        private List<string> imageFileList = new List<string>();
        private BindingList<Label> markerList = new BindingList<Label>();

        private Bitmap marker = new Bitmap(Properties.Resources.Marker);
        private Bitmap highlightedMarker = new Bitmap(Properties.Resources.HighlightedMarker);

        private ASCIIEncoding enc = new ASCIIEncoding();
        private PropertyItem bestPropItem;
        private Label tempMarker;

        private bool editMode = true;
        private bool hasNoImageLoaded = true;
        

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

            markerLabelTextBox.KeyPress += new KeyPressEventHandler(MarkerLabelTextBox_KeyPress);
        }

        private void MarkerLabelTextBox_KeyPress(object sender, KeyPressEventArgs e)
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
                UnselectTextbox();
            }
        }

        private void UnselectTextbox()
        {
            tempMarker.Dispose();
            tempMarker = null;
            markerLabelTextBox.Text = "";
            markerLabelTextBox.Visible = false;
        }

        private void DeleteMarkerButton_Click(object sender, EventArgs e)
        {

            markerList[markerListBox.SelectedIndex].Dispose();
            markerList.RemoveAt(markerListBox.SelectedIndex);
            currentMarker--;
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            if (hasNoImageLoaded)
            {
                OpenLoadDialog();
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

        private void OpenLoadDialog()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string fileName in openFileDialog.FileNames)
                {
                    try
                    {
                        //Image tryLoadingAsImage = Image.FromFile(file);
                        imageFileList.Add(fileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Cannot display the image: " + fileName.Substring(fileName.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }
                LoadPicture();
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            SavePicture();
            picCounter++;
            LoadPicture();
        }

        private void SavePicture()
        {
            //stackoverflow.com_questions_48716515_how-to-get-real-image-pixel-point-x-y-from-picturebox
            int realW = pictureBox.Image.Width;
            int realH = pictureBox.Image.Height;
            int currentW = pictureBox.ClientRectangle.Width;
            int currentH = pictureBox.ClientRectangle.Height;
            double zoomW = (currentW / (double)realW);
            double zoomH = (currentH / (double)realH);
            double zoomActual = Math.Min(zoomW, zoomH);
            double padX = zoomActual == zoomW ? 0 : (currentW - (zoomActual * realW)) / 2;
            double padY = zoomActual == zoomH ? 0 : (currentH - (zoomActual * realH)) / 2;

            for (int i = 0; i < markerList.Count; i++)
            {
                int realX = (int)(((markerList[i].Location.X + 32) - padX) / zoomActual);
                int realY = (int)(((markerList[i].Location.Y + 32) - padY) / zoomActual);

                string PosXval = realX < 0 || realX > realW ? "-" : realX.ToString();
                string PosYval = realY < 0 || realY > realH ? "-" : realY.ToString();
                Console.WriteLine("X: " + PosXval + " //Y: " + PosYval);
            }
            string description = descriptionTextBox.Text;
            if (markerList.Count > 0)
            {
                description += MarkerToJson();
            }
            //bestPropItem.Value = enc.GetBytes(description);
            Bitmap copy = new Bitmap(pictureBox.Image);
            pictureBox.Image.Dispose();
            pictureBox.Image = null;
            if (System.IO.File.Exists(imageFileList[picCounter]))
            {
                System.IO.File.Delete(imageFileList[picCounter]);
            }

            copy.Save(imageFileList[picCounter]);
            Reset();
        }

        private void Reset()
        {
            waitingForOpenLabel.Visible = true;
            hasNoImageLoaded = true;
            imgCounterLabel.Text = "(0/0)";
            if (nextButton.Text == "Save")
            {
                imageFileList.Clear();
            }

        }

        private void LoadPicture()
        {
            if (picCounter >= 0 && picCounter < imageFileList.Count)
            {
                waitingForOpenLabel.Visible = false;
                pictureBox.Image = Image.FromFile(imageFileList[picCounter]);

                hasNoImageLoaded = false;
                
                PropertyItem[] propItems = pictureBox.Image.PropertyItems;
                List<int> propIds = new List<int>();
                foreach (PropertyItem item in propItems)
                {
                    propIds.Add(item.Id);
                }

                if (propIds.Contains(270) )
                {
                    bestPropItem = pictureBox.Image.GetPropertyItem(270);
                    List<byte> tempChars = new List<byte>();
                    foreach (byte character in bestPropItem.Value)
                    {
                        if (character != 0)
                        {
                            tempChars.Add(character);
                        }
                    }
                    bestPropItem.Value = tempChars.ToArray();
                }
                else if (propIds.Contains(40092))
                {
                    bestPropItem = pictureBox.Image.GetPropertyItem(40092);
                    List<byte> tempChars = new List<byte>();
                    foreach (byte character in bestPropItem.Value)
                    {
                        if (character != 0)
                        {
                            tempChars.Add(character);
                        }
                    }
                    bestPropItem.Value = tempChars.ToArray();
                }
                else
                {
                    bestPropItem = propItems[0];
                    bestPropItem.Id = 270;
                    bestPropItem.Type = 2;
                    bestPropItem.Value = enc.GetBytes("");
                }

                JsonToMarker(enc.GetString(bestPropItem.Value));
                descriptionTextBox.Text = enc.GetString(bestPropItem.Value);

                imgCounterLabel.Text = "(" + (picCounter + 1) + "/" + imageFileList.Count + ")";

                if ((picCounter + 1) == imageFileList.Count)
                {
                    nextButton.Text = "Save";
                }
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            picCounter--;
            LoadPicture();
        }

        private void MarkerCheckbox_CheckedChanged(object sender, EventArgs e)
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

        private void OpenFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenLoadDialog();
        }

        private void MarkerListBox_SelectedIndexChanged(object sender, EventArgs e)
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

        private string MarkerToJson() {
            string jsonMarkerString = "{\"Person\": [{\"Name\": \"Alex\",\" Position\":},\"{Name\": \"Peter\", \"xPos\": 300, yPos: 300}]";
            return jsonMarkerString;
        }

        private void JsonToMarker(string markerString) {
            if (markerString.Contains("{\"Person\": [{"))
            {

            }
        }
    }
}