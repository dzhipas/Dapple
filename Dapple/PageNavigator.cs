using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

namespace Dapple
{
   internal partial class PageNavigator : UserControl
   {
      #region Events

      /// <summary>
      /// Invoked when the back button is pressed.
      /// </summary>
      internal event EventHandler PageBack;

      /// <summary>
      /// Invoked when the forward button is pressed.
      /// </summary>
      internal event EventHandler PageForward;

      #endregion

      #region Constructors

      internal PageNavigator()
      {
         InitializeComponent();
      }

      #endregion

      #region Event Handlers

      private void cBackButton_Click(object sender, EventArgs e)
      {
         if (PageBack != null) PageBack(sender, e);
      }

      private void cForwardButton_Click(object sender, EventArgs e)
      {
         if (PageForward != null) PageForward(sender, e);
      }

      #endregion


		#region Display methods

		internal void SetState(int iPage, int iNumResults)
      {
         int iNumPages = PagesFromResults(iNumResults);
         SetState(String.Format(CultureInfo.InvariantCulture, "Results {0}-{1} of {2}", iPage * m_iResultsPerPage + 1, Math.Min((iPage + 1) * m_iResultsPerPage, iNumResults), iNumResults), iPage > 0, iPage < iNumPages - 1);
      }

      internal void SetState(String szMessage)
      {
         SetState(szMessage, false, false);
      }

		internal void SetPageSize(int newSize)
		{
			m_iResultsPerPage = newSize;
		}

      private delegate void SetStateDelegate(String szMessage, bool blCanBack, bool blCanForward);
      internal void SetState(String szMessage, bool blCanBack, bool blCanForward)
      {
         if (InvokeRequired)
         {
            Invoke(new SetStateDelegate(SetState), new object[] { szMessage, blCanBack, blCanForward });
         }
         else
         {
            c_bBack.Enabled = blCanBack;
            c_bForward.Enabled = blCanForward;
            c_lStatusMessage.Text = szMessage;
         }
      }

      #endregion

      #region Statics

      internal int m_iResultsPerPage = 10;

      internal int PagesFromResults(int iNumResults)
      {
         int result = iNumResults / m_iResultsPerPage;
         if (iNumResults % m_iResultsPerPage != 0) result++;
         return result;
      }

      #endregion
   }
}
