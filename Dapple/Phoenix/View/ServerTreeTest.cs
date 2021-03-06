﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Dapple.LayerGeneration;

namespace NewServerTree.View
{
	internal partial class ServerTreeTest : Form
	{
		DappleModel m_oModel;

		internal ServerTreeTest()
		{
			InitializeComponent();

			m_oModel = new DappleModel(null);

			serverTree1.Attach(m_oModel);
			serverTree2.Attach(m_oModel);
		}

		internal static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new ServerTreeTest());
		}

		private void button1_Click(object sender, EventArgs e)
		{
			System.Media.SystemSounds.Beep.Play();
			//m_oModel.LoadTestView();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			m_oModel.LoadFromView(new Dapple.DappleView(@"C:\Documents and Settings\chrismac\Local Settings\Application Data\DappleData\Config\lastview.dapple"));
		}

		private void bSearch_Click(object sender, EventArgs e)
		{
			m_oModel.SetSearchFilter(textBox1.Text, null);
		}
	}
}
