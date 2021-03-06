﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using TELMAGEN.IO;
using TELMAGEN.GUI;
using TELMAGEN.Library;

namespace TELMAGEN
{
    public partial class Main : Form
    {
        private TelmagenProject project = null;
        private int shown_ttime = -1;
        private Bitmap working_bm = null;
        private Bitmap sunmoon_bm = null;
        public Main()
        {
            InitializeComponent();
        }

        #region private helper methods
        private void RefreshCanvas()
        {
            // reset the GUI to defaults
            if (this.project == null)
            {
                pbcanvas.Image = new Bitmap(TELMAGEN.Properties.Resources.telmagen_splash);

                lblTime.Text = "00:00";
                lblPosition.Text = "0 of 0";

                SetSunMoon(90); // default to daytime
                return;
            }

            if (this.project.Image != null)
            {
                // draw the base image
                pbcanvas.Image = this.project.Image;
            }

            // if we have a current ttime lets refresh its values into the GUI
            if (this.shown_ttime > 0)
            {
                // get the ttime object and draw the time of day label
                TTime ttime = this.project.GetTTime(this.shown_ttime);
                lblTime.Text = ttime.ToString();

                // draw the time slice number label (i.e. position)
                int minttime = this.shown_ttime;
                int maxttime = this.project.GetLastTTime();
                lblPosition.Text = minttime.ToString() + " of " + maxttime.ToString();

                // plot the saved points
                PlotPoints(ttime.TPoints);
                // list all the points
                ListPoints(ttime.TPoints);

                // refresh the image of the sun and moon
                #region night
                if (
                    ((ttime.hour >= 20) && (ttime.hour <= 24)) ||
                    ((ttime.hour >= 0) && (ttime.hour <= 5))
                    )
                {
                    // nite time 8pm to 5am
                    SetSunMoon(270);
                }
                #endregion
                #region day
                else if ((ttime.hour >= 8) && (ttime.hour <= 17))
                {
                    // day time 8am to 5pm
                    SetSunMoon(90);
                }
                #endregion
                #region dawn
                else if (ttime.hour == 6)
                {
                    // dawn
                    SetSunMoon(315);
                }
                else if (ttime.hour == 7)
                {
                    // dawn
                    SetSunMoon(45);
                }
                #endregion
                #region dusk
                else if (ttime.hour == 18)
                {
                    // dusk
                    SetSunMoon(135);
                }
                else if (ttime.hour == 19)
                {
                    // dusk
                    SetSunMoon(225);
                }
                #endregion
            }

            RefreshLabels();
        }
        //private void DrawSunMoon(int hour)
        //{
        //    // clear the last image if there was one
        //    if (this.sun_moon_bm != null) { this.sun_moon_bm.Dispose(); }

        //    // sun moon graphic is oriented so that the sun is on the left and the moon is on the right
        //    this.sun_moon_bm = new Bitmap(TELMAGEN.Properties.Resources.sun_moon);

        //    int angle = 90;

        //    // rotate the graphic
        //    //make a graphics object from the empty bitmap
        //    using (Graphics g = Graphics.FromImage(sun_moon_bm))
        //    {
        //        //move rotation point to center of image
        //        g.TranslateTransform((float)sun_moon_bm.Width / 2, (float)sm.Height / 2);
        //        //rotate
        //        g.RotateTransform(angle);
        //        //move image back
        //        g.TranslateTransform(-(float)sun_moon_bm.Width / 2, -(float)sm.Height / 2);
        //        //draw passed in image onto graphics object
        //        g.DrawImage(this.sun_moon_bm, new Point(0, 0));



        //        // take the top half of the image
        //        System.Drawing.Imaging.BitmapData half = this.sun_moon_bm.LockBits(
        //            new Rectangle(0, 0, sun_moon_bm.Width, sun_moon_bm.Height / 2),
        //            System.Drawing.Imaging.ImageLockMode.ReadOnly,
        //            this.sun_moon_bm.PixelFormat);

        //        // copy the partial image
        //        Bitmap h = new Bitmap(half.Width, half.Height, half.Stride, half.PixelFormat, half.Scan0);

        //        // unlock area
        //        sun_moon_bm.UnlockBits(half);

                

                
        //        pbSunMoon.Image = this.sun_moon_bm;
        //    }

           
        //    // take the top half of it

        //    // assign to the paintbox

        //}
        private void ListPoints(TPoint[] tpoints)
        {
            if (tpoints == null) { return; }
            
            // clear any existing points
            lstPoints.Items.Clear();

            foreach(TPoint p in tpoints)
            {
                lstPoints.Items.Add(p);
            }

        }
        private void PlotPoints(TPoint[] tpoints)
        {
            // abort if we dont have valid input
            if (this.project == null) { return; }
            if (this.project.Image == null) { return; }
            if (tpoints == null) { return; }

            //
            // while we have to convert the x/y of the paintbox mouse click event
            // no such conversion is required for the paintbox refresh as all points are plotted
            // on the original image and then the paintbox is set to contain the image
            //

            // clear the last image if there was one
            if (this.working_bm != null) { this.working_bm.Dispose(); }

            // make a clone of the base image
            this.working_bm = this.project.Image.Clone() as Bitmap;

            // draw the points on the clone then set the canvas
            using (Graphics g = Graphics.FromImage(working_bm))
            {
                foreach (TPoint tpoint in tpoints)
                {
                    SolidBrush b = ColorCodeAttribute.GetBrush(tpoint.TColor);
                    g.FillEllipse(b, tpoint.x, tpoint.y, 10, 10);
                }

                // if (pbcanvas.Image != null) { pbcanvas.Image.Dispose(); }
                pbcanvas.Image = this.working_bm;
            }

        }
        private void RefreshLabels()
        {
            if (this.project == null) { return; }
            if (this.project.Labels == null) { return; }

            if (!string.IsNullOrEmpty(this.project.Labels.red)) { lblRed.Text = this.project.Labels.red; }
            if (!string.IsNullOrEmpty(this.project.Labels.blue)) { lblBlue.Text = this.project.Labels.blue; }
            if (!string.IsNullOrEmpty(this.project.Labels.yellow)) { lblYellow.Text = this.project.Labels.yellow; }
            if (!string.IsNullOrEmpty(this.project.Labels.green)) { lblGreen.Text = this.project.Labels.green; }
            if (!string.IsNullOrEmpty(this.project.Labels.pink)) { lblPink.Text = this.project.Labels.pink; }
            if (!string.IsNullOrEmpty(this.project.Labels.purple)) { lblPurple.Text = this.project.Labels.purple; }
            if (!string.IsNullOrEmpty(this.project.Labels.orange)) { lblOrange.Text = this.project.Labels.orange; }
            if (!string.IsNullOrEmpty(this.project.Labels.ltblue)) { lblLtBlue.Text = this.project.Labels.ltblue; }
            if (!string.IsNullOrEmpty(this.project.Labels.ltgreen)) { lblLtGreen.Text = this.project.Labels.ltgreen; }
            if (!string.IsNullOrEmpty(this.project.Labels.ltpink)) { lblLtPink.Text = this.project.Labels.ltpink; }
            if (!string.IsNullOrEmpty(this.project.Labels.grey)) { lblGrey.Text = this.project.Labels.grey; }
            if (!string.IsNullOrEmpty(this.project.Labels.white)) { lblWhite.Text = this.project.Labels.white; }
            if (!string.IsNullOrEmpty(this.project.Labels.black)) { lblBlack.Text = this.project.Labels.black; }
        }
        private TColor GetSelectedTColor()
        {
            if (rbBlue.Checked)
            {
                return TColor.BLUE;
            }
            else if (rbRed.Checked)
            {
                return TColor.RED;
            }
            else if (rbGreen.Checked)
            {
                return TColor.GREEN;
            }
            else if (rbYellow.Checked)
            {
                return TColor.YELLOW;
            }
            else if (rbPink.Checked)
            {
                return TColor.PINK;
            }
            else if (rbPurple.Checked)
            {
                return TColor.PURPLE;
            }
            else if (rbOrange.Checked)
            {
                return TColor.ORANGE;
            }
            else if (rbLtBlue.Checked)
            {
                return TColor.BABYBLUE;
            }
            else if (rbLtPink.Checked)
            {
                return TColor.LTPINK;
            }
            else if (rbLtGreen.Checked)
            {
                return TColor.LTGREEN;
            }
            else if (rbGrey.Checked)
            {
                return TColor.GREY;
            }
            else if (rbWhite.Checked)
            {
                return TColor.WHITE;
            }
            else if (rbBlack.Checked)
            {
                return TColor.BLACK;
            }
            else
            {
                throw new InvalidOperationException("Color not selected in toolbox");
            }
        }
        private bool ProcessKeyDown(Keys key_code)
        {
            switch (key_code)
            {
                case Keys.Left:
                    {
                        btnBack_Click(null, null);
                        return true;
                    }
                case Keys.Right:
                    {
                        btnNext_Click(null, null);
                        return true;
                    }
            }
            return false;
        }
        private void SetSunMoon(float angle)
        {
            // clear the last image if there was one
            if (this.sunmoon_bm != null) { this.sunmoon_bm.Dispose(); }
            
            // reset the image
            this.sunmoon_bm = new Bitmap(TELMAGEN.Properties.Resources.sun_moon);

            #region rotate the image
            using (Graphics g = Graphics.FromImage(this.sunmoon_bm))
            {
                g.TranslateTransform((float)this.sunmoon_bm.Width / 2, (float)this.sunmoon_bm.Height / 2);
                //rotate
                g.RotateTransform(angle);
                //move image back
                g.TranslateTransform(-(float)this.sunmoon_bm.Width / 2, -(float)this.sunmoon_bm.Height / 2);
                //draw passed in image onto graphics object
                g.DrawImage(this.sunmoon_bm, new Point(0, 0));
            }
            #endregion

            // take the top half of the picture
            Rectangle cloneRect = new Rectangle(0, 0, this.sunmoon_bm.Width, this.sunmoon_bm.Height / 2);
            System.Drawing.Imaging.PixelFormat format = this.sunmoon_bm.PixelFormat;
            this.sunmoon_bm = this.sunmoon_bm.Clone(cloneRect, format);

            pbSunMoon.Image = this.sunmoon_bm;
        }
        #endregion
        #region event handlers
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (NewProject form = new NewProject())
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        string name = form.ProjectName;
                        int interval = form.Interval;
                        string full_image_name = form.ImageName;
                        ColorLabel labels = form.ColorLabel;

                        //get a project folder name!
                        string folder_name = DataHelpers.FilterStringForFileName(name);

                        // create a folder for the project
                        DirectoryPath projects_folder = FilePaths.Projects;
                        DirectoryPath project_folder = new DirectoryPath(projects_folder, folder_name);

                        if (project_folder.Exists)
                        {
                            MessageBox.Show("Cannot create this project as it already exists");
                            return;
                        }
                        else
                        {
                            project_folder.Create(); 
                        }

                        // create the new project object
                        this.project = new TelmagenProject(name, interval, full_image_name, labels);
                        // init this form to what we are looking at (the first ttime)
                        this.shown_ttime = this.project.GetFirstTTime();

                        #region write the xml file that represents the project
                        string s = this.project.Serialize;
                        FilePath p = new FilePath(project_folder, folder_name + ".xml");
                        p.WriteAllText(s);
                        #endregion
                        #region save the image file to disk
                        // save the image to disk, into the new project folder
                        FilePath i = new FilePath(full_image_name);
                        this.project.Image.Save(project_folder.FullName + "\\" + this.project.ImageName);
                        #endregion

                        RefreshCanvas();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.project == null)
                {
                    MessageBox.Show("Please create a new project or open an existing project before attempting a save operation");
                    return;
                }

                //get a project folder name
                string name = DataHelpers.FilterStringForFileName(project.Name);

                // create a folder for the project
                DirectoryPath projects_folder = FilePaths.Projects;
                DirectoryPath project_folder = new DirectoryPath(projects_folder, name);

                if (!project_folder.Exists)
                {
                    MessageBox.Show("Cannot save project as project folder does not exist (" + project_folder.FullName + ")");
                    return;
                }

                // write the xml file that represents the project
                string s = this.project.Serialize;
                FilePath p = new FilePath(project_folder, name + ".xml");
                p.WriteAllText(s);

                // image save is purposely skipped, the base image never changes after initial project creation
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during save: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                dlgOpenFile.Title = "Select XML Project File";
                dlgOpenFile.InitialDirectory = FilePaths.Projects.FullName;

                if (DialogResult.OK == dlgOpenFile.ShowDialog())
                {
                    #region data checks on input
                    if (string.IsNullOrEmpty(dlgOpenFile.FileName))
                    {
                        MessageBox.Show("You must select a file");
                        return;
                    }

                    FilePath file = new FilePath(dlgOpenFile.FileName);
                    if (!file.Exists)
                    {
                        MessageBox.Show("File does not exist");
                        return;
                    }
                    #endregion

                    #region get the project file and load it into a project object
                    FilePath path = new FilePath(dlgOpenFile.FileName);
                    string s = path.ReadAllText();

                    TelmagenProject p = new TelmagenProject();
                    p.Serialize = s;
                    #endregion

                    #region we restored the project settings, lets restore the image now
                    FilePath image_path = new FilePath(path.Directory.FullName + "\\" + p.ImageName);
                    if (image_path.Exists)
                    {
                        p.Image = new Bitmap(image_path.FullName);
                    }
                    else
                    {
                        MessageBox.Show("Found project file, but could not load image " + image_path.FullName + " " + System.Environment.NewLine +
                            "Ensure the image file exists and try again.");
                        return;
                    }
                    #endregion

                    this.project = p;
                    this.shown_ttime = p.GetFirstTTime();

                    RefreshCanvas();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during save: " + ex.Message + System.Environment.NewLine + ex.StackTrace);
            }
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (DialogResult.No == MessageBox.Show("Are you sure?", "Exit TELMAGEN", MessageBoxButtons.YesNo))
            //{
            //    return;
            //}
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (this.project == null) { return; }

            int max_ttime = this.project.GetLastTTime();
            if (shown_ttime >= max_ttime) { shown_ttime = 0; }

            this.shown_ttime += 1;

            RefreshCanvas();
        }
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (this.project == null) { return; }
            if (this.shown_ttime <= 1)
            {
                this.shown_ttime = this.project.GetLastTTime();
            }
            else
            {
                this.shown_ttime -= 1;
            }

            RefreshCanvas();
        }
        private void btnFirst_Click(object sender, EventArgs e)
        {
            if (this.project == null) { return; }
            this.shown_ttime = 1;
            RefreshCanvas();
        }
        private void btnLast_Click(object sender, EventArgs e)
        {
            if (this.project == null) { return; }

            int max_ttime = this.project.GetLastTTime();
            this.shown_ttime = max_ttime;

            RefreshCanvas();
        }
        private void closeProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.project = null;
            this.shown_ttime = -1;
            //this.uncommitted_points = new List<TPoint>();

            RefreshCanvas();
        }
        private void pbcanvas_MouseUp(object sender, MouseEventArgs e)
        {
            bool multi_points = false;
            if (Form.ModifierKeys == Keys.Control)
            {
                // Do Ctrl-Left Click Work
                multi_points = true;
            }

            if (this.project == null) { return; }

            int xcoor = e.X;
            int ycoor = e.Y;

            // convert the x/y to map to the original image
            xcoor = this.project.ConvertDisplayedXCoor(pbcanvas.Width, xcoor);
            ycoor = this.project.ConvertDisplayedYCoor(pbcanvas.Height, ycoor);

            // locate the proper color
            TColor c = GetSelectedTColor();

            // get the current ttime
            TTime t = this.project.GetTTime(this.shown_ttime);
            // add the new point
            t.AddTPoint(xcoor, ycoor, c);

            // if we are multi plotting (ctrl-click)
            if (multi_points)
            {
                t.AddTPoint(xcoor - 10, ycoor - 10, c);
                t.AddTPoint(xcoor - 10, ycoor + 10, c);
                t.AddTPoint(xcoor + 10, ycoor - 10, c);
                t.AddTPoint(xcoor + 10, ycoor + 10, c);
            }

            // refresh the GUI
            RefreshCanvas();
        }
        private void btnClearPoints_Click(object sender, EventArgs e)
        {
            if (this.project == null) { return; }
            if (this.shown_ttime <= 0) { return; }

            if (MessageBox.Show("Delete all points in this time?", "Confirm Clear Points", MessageBoxButtons.YesNo) == 
                System.Windows.Forms.DialogResult.Yes)
            {
                TTime t = this.project.GetTTime(this.shown_ttime);
                t.TPoints = null;

                RefreshCanvas();
            }
        }
        private void slideshowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.project == null) 
            {
                MessageBox.Show("Please open or create a project before attempting a slideshow");
                return; 
            }

            //save changes just in case
            if (DialogResult.Yes == MessageBox.Show("Would you like to save first", "Save Project", MessageBoxButtons.YesNo))
            {
                saveToolStripMenuItem_Click(null, null);
            }

            try
            {
                using (Slideshow form = new Slideshow())
                {
                    form.Project = this.project;
                    form.Text = "Slideshow " + this.project.Name;
                    form.RefreshLabels();

                    form.ShowDialog();
                    form.DisposeBitmaps(); // reclaim any memory taken by the bitmaps
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.project == null)
            {
                slideshowToolStripMenuItem.Enabled = false;
            }
            else
            {
                slideshowToolStripMenuItem.Enabled = true;
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (About form = new About())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    // nothing to do yet
                }
            }
        }
        private void btnSetLabels_Click(object sender, EventArgs e)
        {
            if (this.project == null) { return; }
            if (this.project.Labels == null) { return; }

            using (SetLabels form = new SetLabels())
            {
                form.ColorLabel = this.project.Labels;

                if (form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ColorLabel c = form.ColorLabel;
                    this.project.Labels = c;
                    RefreshLabels();
                }
            }
        }
        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ProcessKeyDown(e.KeyCode);
        }
        private void bt_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }
        private void rb_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                case Keys.Right:
                    e.IsInputKey = true;
                    break;
            }
        }
        private void rb_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ProcessKeyDown(e.KeyCode);
        }
        private void btn_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = ProcessKeyDown(e.KeyCode);
        }
        private void lstPoints_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPoints.SelectedItem is TPoint)
            {
                TPoint tpoint = (TPoint)lstPoints.SelectedItem;

                // if invalid input then exit
                if (tpoint == null) { return; }
                if (this.project == null) { return; }

                // redraw the base image
                if (this.project.Image != null) { pbcanvas.Image = this.project.Image; }

                // plot all the data points for the current time
                if (this.shown_ttime > 0)
                {
                    // get the ttime object
                    TTime ttime = this.project.GetTTime(this.shown_ttime);
                   
                    // plot the saved points
                    PlotPoints(ttime.TPoints);
                }

                // mark the selected point
                using (Graphics g = Graphics.FromImage(this.working_bm))
                {
                    SolidBrush b = ColorCodeAttribute.GetBrush(tpoint.TColor);
                    int coorx = tpoint.x;
                    int coory = tpoint.y;

                    if (coorx > 20) { coorx = coorx - 10; }
                    if (coory > 20) { coory = coory - 10; }

                    g.FillEllipse(b, coorx, coory, 30, 30);

                    // if (pbcanvas.Image != null) { pbcanvas.Image.Dispose(); }
                    pbcanvas.Image = this.working_bm;
                }

            }
        }
        private void lstPoints_DoubleClick(object sender, EventArgs e)
        {
            if (lstPoints.SelectedItem is TPoint)
            {
                int i = lstPoints.SelectedIndex;

                TPoint tpoint = (TPoint)lstPoints.SelectedItem;

                // if invalid input then exit
                if (tpoint == null) { return; }
                if (this.project == null) { return; }
                if (this.shown_ttime <= 0) { return; }

                // get the current ttime
                TTime ttime = this.project.GetTTime(this.shown_ttime);

                // remove the point from our project
                ttime.RemoveTPoint(tpoint.ID);

                // redraw the base image
                if (this.project.Image != null) { pbcanvas.Image = this.project.Image; }

                // plot all the data points for the current time
                PlotPoints(ttime.TPoints);

                // remove this item from the list box
                lstPoints.Items.RemoveAt(i);
            }
        }

        #endregion

        
    }
}
