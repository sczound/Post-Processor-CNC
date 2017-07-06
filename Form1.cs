﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Helper;



namespace CNC
{
    public partial class Form1 : Form
    {
        String path = "test.txt";
        /// <summary>
        /// Main combo box elements (Plates)
        /// </summary>
        List<String> plateList = new List<String>()
        {
            "80",
            "100",
            "60"
        };

        /// <summary>
        /// Discrete items
        /// </summary>
        List<double> discreteList = new List<double>()
        {
            0.2,
            0.5,
            1.0
        };

        /// <summary>
        /// Radius items
        /// </summary>
        List<double> radiusList = new List<double>()
        {
            0.2,
            0.4,
            0.8,
            1.2,
            1.5
        };

        public Form1()
        {
            InitializeComponent();
            //Set the collections with items and styles for comboBoxes
            comboBoxPlates.DataSource = plateList;
            comboBoxRadius.DataSource = radiusList;
            comboBoxDiscrete.DataSource = discreteList;
            comboBoxDiscrete.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPlates.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxRadius.DropDownStyle = ComboBoxStyle.DropDownList;
            //Set the initial values of combo boxes
            comboBoxPlates.SelectedIndex = 0;
            comboBoxRadius.SelectedIndex = radiusList.IndexOf(0.8);
            comboBoxDiscrete.SelectedIndex = discreteList.IndexOf(1);
        }

        private void buttonGenerateCP_Click(object sender, EventArgs e)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter writer = new StreamWriter(fs);
            CNCWriter CNC = new CNCWriter(writer);

            CNC.DefReal("GB");
            CNC.DefReal("ZZ");
            CNC.Append("GB", 0);
            CNC.MCode(3);
            CNC.MCode(8);
            CNC.Append("R11", InputHelper.GetCBDouble(comboBoxPlates)); 
            CNC.Append("R21", numericSideSize.Value);
            CNC.Append("R2", "R21/(SIN(R11))");
            CNC.Append("R3", InputHelper.GetCBDouble(comboBoxRadius));
            CNC.Append("r4", "r2*(cos (r11/2))*(sin (r11/2)-r34)");
            CNC.Append("r5", "r4/ (tan (r11/2))");
            CNC.Append("r7", InputHelper.GetCBDouble(comboBoxDiscrete));
            CNC.Append("r9", numericPasses.Value);
            CNC.Append("r91", numericAllowance.Value);

            CNC.Inline(true);
            CNC.GCode(90);
            CNC.GCode(1);
            CNC.Echo(numericX.Value);
            CNC.Echo(numericZ.Value);
            CNC.Echo(numericA.Value);
            CNC.FeedRate(9000);
            CNC.Inline(false);

            if (!checkBox1.Checked)
            {
                CNC.While("gb<r9");

                CNC.Inline(true);
                CNC.GCode(1);
                CNC.Append("z", "-r91");
            }
            else
            {
                CNC.While("gb<1");

                CNC.Inline(true);
                CNC.GCode(1);
                CNC.Append("z", 0.005);
            }
            CNC.FeedRate(2000);
            CNC.Inline(false);

            CNC.Append("ZZ", 0);
            CNC.While("zz<2");

            CNC.Inline(true);
            CNC.GCode(1);
            CNC.XCode(15);
            CNC.FeedRate(1000);
            CNC.Inline(false);

            CNC.Append("r1", 1);
            CNC.Append("r8", 0);
            CNC.While("r1<=100");
            CNC.Append("r6", "R4-(SQRT(R4*R4+R5*R5))*(SIN(R11/2))");

            CNC.Inline(true);
            CNC.GCode(1);
            CNC.Append("z", "-(R6-R8)");
            CNC.Append("a", "r7");
            CNC.FeedRate(100);
            CNC.Inline(false);

            CNC.Append("r8", "r6");
            CNC.Append("r1", "r1+r7");
            CNC.Append("r6", 0);
            CNC.EndWhile();

            CNC.Inline(true);
            CNC.GCode(1);
            CNC.XCode(15);
            CNC.FeedRate(1000);
            CNC.Inline(false);

            CNC.Inline(true);
            CNC.GCode(0);
            CNC.Echo("a80");
            CNC.Inline(false);

            CNC.Append("zz", "zz+1");
            CNC.EndWhile();
            CNC.Append("gb", "gb+1");
            CNC.EndWhile();
            CNC.Append("gb", 0);

            CNC.MCode(new object[] { 5, 9, 30 });                       
            //Close streams
            writer.Close();
            fs.Close();
            System.Diagnostics.Process.Start(path);
        }        
    }
}
