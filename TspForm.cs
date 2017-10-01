//////////////////////////////////////////////////////////////////////////////////////////////////
// File Name: TspForm.cs 
///////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Globalization;
using System.Xml;
using System.Text;


namespace Tsp
{
    /// <summary>
    /// Main form for the Travelling Salesman Problem
    /// </summary>
    public partial class TspForm : Form
    {
        /// <summary>
        /// The list of cities where we are trying to find the best tour.
        /// </summary>
        Cities cityList = new Cities();

        /// <summary>
        /// The class that does all the work in the TSP algorithm.
        /// If this is not null, then the algorithm is still running.
        /// </summary>
        Tsp tsp;

        /// <summary>
        /// The image that we draw the tour on.
        /// </summary>
        Image cityImage;

        /// <summary>
        /// The graphics object for the image that we draw the tour on.
        /// </summary>
        Graphics cityGraphics;

        static bool isBusy = false; //флажок "занято" для исключения лишней обработки событий
        PictureBox[] listcity = new PictureBox[10000]; //список картинок        
        PictureBox activePBox; //указатель на активную картинку
        Point clickPoint; //координаты клика мышью
        PictureBox ListCityIm = new PictureBox();
        List<int> mLoc_x = new List<int>();
        List<int> mLoc_y = new List<int>();
        List<int> mRLoc_x = new List<int>();
        List<int> mRLoc_y = new List<int>();
        static string writeFileName = "";
        int inc = 1;
        Form cityNumForm = new Form();
        TextBox tb = new TextBox();
      

        /// <summary>
        /// Delegate for the thread that runs the TSP algorithm.
        /// We use a separate thread so the GUI can redraw as the algorithm runs.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void DrawEventHandler(Object sender, TspEventArgs e);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TspForm()
        {
            InitializeComponent();
            activePBox = null;
        }

        /// <summary>
        /// TSP algorithm raised an event that a new best tour was found.
        /// We need to do an invoke on the GUI thread before doing any draw code.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        private void tsp_foundNewBestTour(object sender, TspEventArgs e)
        {
            if ( this.InvokeRequired )
            {
                try
                {
                    this.Invoke(new DrawEventHandler(DrawTour), new object[] { sender, e });
                    return;
                }
                catch (Exception)
                {
                    // This will fail when run as a control in IE due to a security exception.
                }
            }

            DrawTour(sender, e);
        }

        /// <summary>
        /// A new "best" tour from the TSP algorithm has been received.
        /// Draw the tour on the form, and update a couple of status labels.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        public void DrawTour(object sender, TspEventArgs e)
        {
            this.lastFitnessValue.Text = Math.Round(e.BestTour.Fitness, 2).ToString(CultureInfo.CurrentCulture);
            this.lastIterationValue.Text = e.Generation.ToString(CultureInfo.CurrentCulture);

            if (cityImage == null)
            {
                cityImage = new Bitmap(tourDiagram.Width, tourDiagram.Height);
                cityGraphics = Graphics.FromImage(cityImage);
            }

            int lastCity = 0;
            int nextCity = e.BestTour[0].Connection1;

            cityGraphics.FillRectangle(Brushes.White, 0, 0, cityImage.Width, cityImage.Height);
            foreach( City city in e.CityList )
            {
                // Draw a circle for the city.
                //cityGraphics.DrawEllipse(Pens.Black, city.Location.X - 2, city.Location.Y - 2, 7, 7);

                // Draw the line connecting the city.
                cityGraphics.DrawLine(Pens.Red, cityList[lastCity].Location, cityList[nextCity].Location);

                // figure out if the next city in the list is [0] or [1]
                if (lastCity != e.BestTour[nextCity].Connection1)
                {
                    lastCity = nextCity;
                    nextCity = e.BestTour[nextCity].Connection1;
                }
                else
                {
                    lastCity = nextCity;
                    nextCity = e.BestTour[nextCity].Connection2;
                }
            }

            this.tourDiagram.Image = cityImage;

            if (e.Complete)
            {
                StartButton.Text = "Begin";
                StatusLabel.Text = "Нажмите на кнопку Добавить города или правой кнопкой мыши нажмите на пустое поле.";
                StatusLabel.ForeColor = Color.Black;
            }
        }
        	
        /// <summary>
        /// Draw just the list of cities.
        /// </summary>
        /// <param name="cityList">The list of cities to draw.</param>
        private void DrawCityList(Cities cityList)
        {
            int i = 0;             
            Random rnd = new Random();
        
            foreach (City city in cityList)
            {
                // Draw a circle for the city.
               // graphics.DrawEllipse(Pens.Black, city.Location.X - 7, city.Location.Y - 7, 7, 7);
                PictureBox ListCityIm = new PictureBox();
                ListCityIm.Location = new System.Drawing.Point(city.Location.X, city.Location.Y);
                ListCityIm.Size = new System.Drawing.Size(10,10);
                Color clr = Color.FromArgb(rnd.Next(0,255),rnd.Next(0,255),rnd.Next(0,255));
                ListCityIm.BackColor = clr;
                listcity[i] = ListCityIm;
                tourDiagram.Controls.Add(listcity[i]);
                i++;
                ListCityIm.LocationChanged += new System.EventHandler(pictureBox_LocationChanged);
                
                ListCityIm.MouseDown += new MouseEventHandler(pictureBox_MouseDown);
                ListCityIm.MouseMove += new MouseEventHandler(pictureBox_MouseMove);          
            }
            
            updateCityCount();
        }
         private void pictureBox_LocationChanged (Object sender, EventArgs e)  {
                if (isBusy) return;
                 isBusy = true;
                 drawXY((PictureBox)sender);
                isBusy = false;
                
        }

         private void drawXY (PictureBox pBox) {
               toolStripStatusLabel1.Text =  String.Format("X={0}, Y={1}",pBox.Location.X, pBox.Location.Y);
                statusStrip1.Refresh();
          }


         //Метод активации картинки pictBox
         private void setActive(PictureBox pictBox) {
            if (activePBox != null) {
		         activePBox.BorderStyle = BorderStyle.None;
	         }
	        activePBox = pictBox;
            activePBox.BorderStyle = BorderStyle.FixedSingle;
            activePBox.Tag = activePBox.Location.X + "," + activePBox.Location.Y;
            drawXY(activePBox);
	        activePBox.Focus();
        }


         private void pictureBox_MouseMove(Object sender, System.Windows.Forms.MouseEventArgs e)
         {
            if (activePBox!=null && e.Button == MouseButtons.Left) {
            if (isBusy) return;
            isBusy = true;
           // activePBox.Left += e.X - clickPoint.X;
            //activePBox.Top += e.Y - clickPoint.Y;
            int mouseX = e.Location.X+activePBox.Location.X-clickPoint.X;
            if (mouseX<0) mouseX=0; 
            if (mouseX>this.tourDiagram.Width-activePBox.Width) mouseX=this.tourDiagram.Width-activePBox.Width;
            int mouseY = e.Location.Y+activePBox.Location.Y-clickPoint.Y;
            if (mouseY<0) mouseY=0; 
            if (mouseY>this.tourDiagram.Height-activePBox.Height) mouseY=this.tourDiagram.Height-activePBox.Height;
            activePBox.Left = mouseX;
            activePBox.Top = mouseY;    
           /* if (!AnyBarCrossing((PictureBox )sender)) {
                activePBox.Tag = activePBox.Location.X + "," + activePBox.Location.Y;
                activePBox.Location.X = point;
             }
            else activePBox.Location  = oldPosition ();*/
            drawXY(activePBox);
            isBusy = false;         
           }          
       }

         private void pictureBox_MouseDown(Object sender, System.Windows.Forms.MouseEventArgs e)
         {           
               if (isBusy) return;
                 isBusy = true;
               if (e.Button == MouseButtons.Right) {
                PictureBox pBox = (PictureBox )sender;
                pBox.Dispose();          
                Controls.Remove(pBox);
               }
               else if (e.Button == MouseButtons.Left) {
                   int count = mLoc_x.Count;
                    setActive ((PictureBox)sender);
                    clickPoint = e.Location;
                    Xml_Write(activePBox.Location.X, activePBox.Location.Y, count);
                    activePBox.BackColor = Color.Orange;
                    activePBox.Size =new System.Drawing.Size(15, 15);
                    Label lbl = new Label();
                    activePBox.Controls.Add(lbl);
                    lbl.Text = Convert.ToString(inc);
                    lbl.Size = new System.Drawing.Size(15, 15);
                    lbl.Location = new System.Drawing.Point(0,2);
                    lbl.Font = new System.Drawing.Font("Verdana", 5, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                    activePBox.Refresh();
                    inc++;
               }
            isBusy = false;
            //Xml_Read();
         }


         private void Xml_Read(PictureBox pBox)
         {
             string readFileName = "";
             
             readFileName = this.fileNameTextBox.Text;
             int loc_x = pBox.Location.X;
             int loc_y = pBox.Location.Y;
             int locx1 = loc_x - 1, locx2 = loc_x + 1;
             int locy1 = loc_y - 1, locy2 = loc_y + 1;
             int xml_x, xml_y;            
             
             XmlTextReader reader = new XmlTextReader(readFileName);
             while (true)
             {
                 reader.ReadToFollowing("City");
                 reader.MoveToFirstAttribute();
                 xml_x = Convert.ToInt32(reader.GetAttribute("X"));
                 xml_y = Convert.ToInt32(reader.GetAttribute("Y"));
                 if ((xml_x == loc_x || xml_x == locx1 || xml_x == locx2) && (xml_y == loc_y || xml_y == locy1 || xml_y == locy2))
                 {
                     NumberCitiesValue.Text = reader.GetAttribute("X");                    
                     break;
                 }
             
             }          
                 
         }

         public void Xml_Write(int loc_x, int loc_y, int count)
         {
             writeFileName = "../../ChooseCity.xml";
             int i;
             mLoc_x.Add(loc_x);
             mLoc_y.Add(loc_y); 

             XmlTextWriter writer = new XmlTextWriter(writeFileName, Encoding.UTF8);
             writer.Formatting = Formatting.Indented;
             writer.WriteStartDocument();

             writer.WriteStartElement("CityList");
             for (i = 0; i <= count; i++)
             {
                 writer.WriteStartElement("City");


                 writer.WriteAttributeString("X", "" + mLoc_x[i]);

                 writer.WriteAttributeString("Y", "" + mLoc_y[i]);


                 writer.WriteEndElement();
             }
             writer.WriteEndElement();
             writer.WriteEndDocument();
             writer.Flush();
             writer.Close();
         }


         public void Xml_RandWrite(int count)
         {
             writeFileName = "../../ChooseCity.xml";
             int i;

             Random rand = new Random();
             for (int j = 0; j < count; j++)
             {
                 mRLoc_x.Add(rand.Next(400));
                 mRLoc_y.Add(rand.Next(420));
             }

             int count_c = mRLoc_x.Count;
             XmlTextWriter writer = new XmlTextWriter(writeFileName, Encoding.UTF8);
             writer.Formatting = Formatting.Indented;
             writer.WriteStartDocument();

             writer.WriteStartElement("CityList");
             for (i = 0; i < count_c; i++)
             {
                 writer.WriteStartElement("City");


                 writer.WriteAttributeString("X", "" + mRLoc_x[i]);

                 writer.WriteAttributeString("Y", "" + mRLoc_y[i]);


                 writer.WriteEndElement();
             }
             writer.WriteEndElement();
             writer.WriteEndDocument();
             writer.Flush();
             writer.Close();
         }
        /// <summary>
        /// User clicked the start button to start the TSP algorithm.
        /// If it is already running, then this button will say stop and we will stop the TSP.
        /// Otherwise, 
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        private void StartButton_Click(object sender, EventArgs e)
        {
            // we are already running, so tell the tsp thread to halt.
            if (tsp != null)
            {
                tsp.Halt = true;
                return;
            }

            int populationSize = 0;
            int maxGenerations = 0;
            int mutation = 0;
            int groupSize = 0;
            int numberOfCloseCities = 0;
            int chanceUseCloseCity = 0;
            int seed = 0;

            try
            {
                populationSize = Convert.ToInt32(populationSizeTextBox.Text, CultureInfo.CurrentCulture);
                maxGenerations = Convert.ToInt32(maxGenerationTextBox.Text, CultureInfo.CurrentCulture);
                mutation = Convert.ToInt32(mutationTextBox.Text, CultureInfo.CurrentCulture);
                groupSize = Convert.ToInt32(groupSizeTextBox.Text, CultureInfo.CurrentCulture);
                numberOfCloseCities = Convert.ToInt32(NumberCloseCitiesTextBox.Text, CultureInfo.CurrentCulture);
                chanceUseCloseCity = Convert.ToInt32(CloseCityOddsTextBox.Text, CultureInfo.CurrentCulture);
                seed = 0;//Convert.ToInt32(randomSeedTextBox.Text, CultureInfo.CurrentCulture);
            }
            catch (FormatException)
            {
            }

            if (populationSize <= 0)
            {
                MessageBox.Show("Укажите численность популяции", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1 ,MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (maxGenerations <= 0)
            {
                MessageBox.Show("Укажите максимальное число поколений", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if ((mutation < 0) || (mutation > 100))
            {
                MessageBox.Show("Коэффициент мутации должен быть от 0 до 100.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if ((groupSize < 2) || ( groupSize > populationSize ))
            {
                MessageBox.Show("You must specify a Group (Neighborhood) Size between 2 and the population size.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if ((numberOfCloseCities < 3) || (numberOfCloseCities >= cityList.Count))
            {
                MessageBox.Show("The number of nearby cities to evaluate for the greedy initial populations must be more than 3 and less than the total number of cities.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if ((chanceUseCloseCity < 0) || (chanceUseCloseCity > 95))
            {
                MessageBox.Show("The odds of using a nearby city when creating the initial population must be between 0% - 95%.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (seed < 0)
            {
                MessageBox.Show("You must specify a Seed for the Random Number Generator", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (cityList.Count < 5)
            {
                MessageBox.Show("Вы должны добавить больше 5 городов. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            this.StartButton.Text = "Stop";
            ThreadPool.QueueUserWorkItem( new WaitCallback(BeginTsp));
        }

        /// <summary>
        /// Starts up the TSP class.
        /// This function executes on a thread pool thread.
        /// </summary>
        /// <param name="stateInfo">Not used</param>
        private void BeginTsp(Object stateInfo)
        {
            int Col;
            string nCol = string.Empty;
            // Assume the StartButton_Click did all the error checking
            int populationSize = Convert.ToInt32(populationSizeTextBox.Text, CultureInfo.CurrentCulture);
            int maxGenerations = Convert.ToInt32(maxGenerationTextBox.Text, CultureInfo.CurrentCulture); ;
            int mutation = Convert.ToInt32(mutationTextBox.Text, CultureInfo.CurrentCulture);
            int groupSize = Convert.ToInt32(groupSizeTextBox.Text, CultureInfo.CurrentCulture);
            int seed = 0; //Convert.ToInt32(randomSeedTextBox.Text, CultureInfo.CurrentCulture);
            int numberOfCloseCities = Convert.ToInt32(NumberCloseCitiesTextBox.Text, CultureInfo.CurrentCulture);
            int chanceUseCloseCity = Convert.ToInt32(CloseCityOddsTextBox.Text, CultureInfo.CurrentCulture);

            Col = cityList.CalculateCityDistances(numberOfCloseCities);

            nCol = Convert.ToString(Col);

            if (label1.InvokeRequired) 
                label1.Invoke(new Action<string>((s) => label1.Text = s), nCol);
            else 
                label1.Text = nCol;

            tsp = new Tsp();
            tsp.foundNewBestTour += new Tsp.NewBestTourEventHandler(tsp_foundNewBestTour);
            tsp.Begin(populationSize, maxGenerations, groupSize, mutation, seed, chanceUseCloseCity, cityList, Col);
            tsp.foundNewBestTour -= new Tsp.NewBestTourEventHandler(tsp_foundNewBestTour);
            tsp = null;
           
        }

        /// <summary>
        /// User is selecting a new city list XML file.
        /// Not allowed if running the TSP algorithm.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        private void selectFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileOpenDialog = new OpenFileDialog();
            fileOpenDialog.Filter = "XML(*.xml)|*.xml";
            fileOpenDialog.InitialDirectory = ".";
            fileOpenDialog.ShowDialog();
            fileNameTextBox.Text = fileOpenDialog.FileName;
        }

        /// <summary>
        /// User has chosen to open the 
        /// Not allowed if running the TSP algorithm.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        private void openCityListButton_Click(object sender, EventArgs e)
        {
            if (tsp != null)
            {
                StatusLabel.Text = "Невозможно добавить город во время работы";
                StatusLabel.ForeColor = Color.Red;
                return;
            }
            cityForm();
        }


        public void cityForm()
        {
            cityNumForm = new Form();
            cityNumForm.Show();
            cityNumForm.ClientSize = new System.Drawing.Size(250, 120);

               Button button = new Button();
			   cityNumForm.Controls.Add(button);
               button.Location = new System.Drawing.Point(10, 60);
			   button.Name = "button1";
			   button.Size = new System.Drawing.Size(100, 50);
			   button.TabIndex = 1;
			   button.Text = "Добавить новые";
			   button.UseVisualStyleBackColor = true;


               Button button1 = new Button();
               cityNumForm.Controls.Add(button1);
               button1.Location = new System.Drawing.Point(140, 60);
               button1.Name = "button1";
               button1.Size = new System.Drawing.Size(100, 50);
               button1.TabIndex = 1;
               button1.Text = "Добавить из файла";
               button1.UseVisualStyleBackColor = true;

              button.Click += new EventHandler(button_Click);
              button1.Click += new EventHandler(button1_Click);
        }


        private void button_Click(object sender, EventArgs e)
        {           
                tb = new TextBox();
               cityNumForm.Controls.Add(tb);            
			   tb.Location = new System.Drawing.Point(60, 25);
			   tb.Name = "textBox1";
			   tb.Size = new System.Drawing.Size(35, 13);
			   tb.TabIndex = 0;

               Label lbl = new Label();
               cityNumForm.Controls.Add(lbl);
               lbl.AutoSize = true;
               lbl.Location = new System.Drawing.Point(20, 10);
               lbl.Name = "label1";
               lbl.Size = new System.Drawing.Size(35, 13);
               lbl.Text = "Введите количество городов";
               lbl.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));

               Button bt = new Button();
               cityNumForm.Controls.Add(bt);
               bt.Location = new System.Drawing.Point(100, 25);
               bt.Name = "button1";
               bt.Size = new System.Drawing.Size(35, 20);
               bt.Text = "Ok";

               bt.Click += new EventHandler(button2_Click);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            string fileName = "";

            try
            {             
                fileName = this.fileNameTextBox.Text;

                cityList.OpenCityList(fileName);
                DrawCityList(cityList);
                cityNumForm.Close();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Файл не найден: " + fileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            catch (InvalidCastException)
            {
                MessageBox.Show("Файл Cities XML невозможно открыть", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int cityCol;
            string fileName = "../../ChooseCity.xml";

            cityCol = Convert.ToInt32(tb.Text);

            if (cityCol<5)
            {
                MessageBox.Show("Вы должны добавить больше 5 городов. ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            else
            {
                mRLoc_x.Clear();
                mRLoc_y.Clear();
                Xml_RandWrite(cityCol);
                
                try
                {
                    cityList.OpenCityList(fileName);
                    DrawCityList(cityList);
                    cityNumForm.Close();
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Файл не найден: " + fileName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                catch (InvalidCastException)
                {
                    MessageBox.Show("Файл Cities XML невозможно открыть", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                cityNumForm.Close();
            }
        }

        /// <summary>
        /// User has selected to clear the city list.
        /// Not allowed if running the TSP algorithm.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        private void clearCityListButton_Click(object sender, EventArgs e)
        {
            int i=0;
            if (tsp != null)
            {
                StatusLabel.Text = "Невозможно добавить город во время работы";
                StatusLabel.ForeColor = Color.Red;
                return;
            }
            foreach (PictureBox pBox in listcity)
            {

                tourDiagram.Controls.Remove(listcity[i]);
                i++;
                tourDiagram.Update();
            }
            
            cityList.Clear();
            this.DrawCityList(cityList);
           
           // writeFileName = this.fileNameTextBox.Text;
            cityImage = new Bitmap(tourDiagram.Width, tourDiagram.Height);
            cityGraphics = Graphics.FromImage(cityImage);

            cityGraphics.FillRectangle(Brushes.White, 0, 0, cityImage.Width, cityImage.Height);
            this.tourDiagram.Image = cityImage;
            inc = 1;
            mLoc_x.Clear();
            mLoc_y.Clear();
            mRLoc_x.Clear();
            mRLoc_y.Clear();

         
        }

        /// <summary>
        /// User clicked a point on the city map.
        /// As long as we aren't running the TSP algorithm,
        /// place a new city on the map and add it to the city list.
        /// </summary>
        /// <param name="sender">Object that generated this event.</param>
        /// <param name="e">Event arguments.</param>
        private void tourDiagram_MouseDown(object sender, MouseEventArgs e)
        {
            int i = 0;
            if(e.Button==MouseButtons.Left){
                if (tsp != null)
              {
                StatusLabel.Text = "Невозможно добавить город во время работы";
                StatusLabel.ForeColor = Color.Red;
                return;
              }
                foreach (PictureBox pBox in listcity)
                {
                    tourDiagram.Controls.Remove(listcity[i]);
                    i++;
                    tourDiagram.Update();
                }
              cityList.Add(new City(e.X, e.Y));
              this.DrawCityList(cityList);
            }
           
        }

        /// <summary>
        /// Display the number of cities on the form.
        /// </summary>
        private void updateCityCount()
        {
            this.NumberCitiesValue.Text = cityList.Count.ToString();
        }

        private void TspForm_Load(object sender, EventArgs e)
        {          
            mLoc_x.Clear();
            mLoc_y.Clear();
        }

        private void newAddCity_Click(object sender, EventArgs e)
        {
            if (tsp != null)
            {
                StatusLabel.Text = "Невозможно добавить город во время работы";
                StatusLabel.ForeColor = Color.Red;
                return;
            }
            cityList.OpenCityList(writeFileName);
        }       

       
    }
}