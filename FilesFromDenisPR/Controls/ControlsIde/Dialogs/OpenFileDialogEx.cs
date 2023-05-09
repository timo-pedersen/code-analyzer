using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Core.Api.Service;
using Neo.ApplicationFramework.Interfaces;

namespace Neo.ApplicationFramework.Controls.Dialogs
{

    public class OpenFileDialogEx : Component
    {

        #region Dll import
        [DllImport("user32.dll", EntryPoint = "SendMessageA", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern uint SendMessage(uint Hdc, uint Msg_Const, uint wParam, uint lParam);

        [DllImport("user32.dll", EntryPoint = "FindWindowExA", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern uint FindWindowEx(uint hwndParent, uint hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "GetForegroundWindow", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        private static extern uint GetForegroundWindow();

        #endregion


        public enum DialogViewTypes
        {
            LargeIcons = 0x7029,
            List = 0x702b,
            Details = 0x702c,
            Thumbnails = 0x702d,      // Try between 0x7031 and 0x702d
            SmallIcons = 0x702a
        }

        private bool m_IsWatching = false;
        private readonly DialogViewTypes m_DialogViewTypes;
        private const int WM_COMMAND = 0x0111;
        private OpenFileDialog m_OpenFileDialog;
      
        public OpenFileDialogEx()
        {
            m_IsWatching = false;
            m_DialogViewTypes = DialogViewTypes.Thumbnails;
        }

        public OpenFileDialogEx(DialogViewTypes dialogViewTypes): this()
        {
            m_DialogViewTypes = dialogViewTypes;            
        }

        private OpenFileDialog OpenFileDialog
        {
            get
            {
                if (m_OpenFileDialog == null)
                {
                    m_OpenFileDialog = new OpenFileDialog();
                }

                return m_OpenFileDialog;
            }
            
        }

        public bool Multiselect
        {
            get
            {
                return OpenFileDialog.Multiselect;
            }
            set
            {
                 OpenFileDialog.Multiselect = value;
            }
        }

        public bool ReadOnlyChecked
        {
            get
            {
                return OpenFileDialog.ReadOnlyChecked;
            }
            set
            {
                OpenFileDialog.ReadOnlyChecked = value;
            }
        }


        public bool ShowReadOnly
        {
            get
            {
                return OpenFileDialog.ShowReadOnly;
            }
            set
            {
                OpenFileDialog.ShowReadOnly = value;
            }
        }

        public Stream OpenFile()
        {
            return OpenFileDialog.OpenFile();
        }  

        [DefaultValue(true)]
        public bool AddExtension
        {
            get
            {
                return OpenFileDialog.AddExtension;
            }
            set
            {
                OpenFileDialog.AddExtension = value;
            }
        }
     
        [DefaultValue(true)]
        public bool CheckPathExists
        {
            get
            {
                return OpenFileDialog.CheckPathExists;
            }
            set
            {
                OpenFileDialog.CheckPathExists = value;
            }
        }

        [DefaultValue("")]
        public string DefaultExt 
       {
            get
            {
                return OpenFileDialog.DefaultExt;
            }
            set
            {
                OpenFileDialog.DefaultExt = value;
            }
        }

        [DefaultValue(true)]
        public bool DereferenceLinks 
        {
            get
            {
                return OpenFileDialog.DereferenceLinks;
            }
            set
            {
                OpenFileDialog.DereferenceLinks = value;
            }
        }

        [DefaultValue("")]
        public virtual string FileName
        {
            get
            {
                return OpenFileDialog.FileName;
            }
            set
            {
                OpenFileDialog.FileName = value;
            }
        }

        [DesignerSerializationVisibility(0)]
        [Browsable(false)]
        public virtual string[] FileNames 
        {
            get
            {
                return OpenFileDialog.FileNames;
            }
        }

        [DefaultValue("")]
        [Localizable(true)]
        public string Filter 
        {
            get
            {
                return OpenFileDialog.Filter;
            }
            set
            {
                OpenFileDialog.Filter = value;
            }
        }

        [DefaultValue(1)]
        public int FilterIndex
        {
            get
            {
                return OpenFileDialog.FilterIndex;
            }
            set
            {
                OpenFileDialog.FilterIndex = value;
            }
        }

        [DefaultValue("")]
        public string InitialDirectory
        {
            get
            {
                return OpenFileDialog.InitialDirectory;
            }
            set
            {
                OpenFileDialog.InitialDirectory = value;
            }
        }      

       
        [DefaultValue(false)]
        public bool RestoreDirectory 
        {
            get
            {
                return OpenFileDialog.RestoreDirectory;
            }
            set
            {
                OpenFileDialog.RestoreDirectory = value;
            }
        }

        [DefaultValue(false)]
        public bool ShowHelp
        {
            get
            {
                return OpenFileDialog.ShowHelp;
            }
            set
            {
                OpenFileDialog.ShowHelp = value;
            }
        }
     
        [DefaultValue(false)]
        public bool SupportMultiDottedExtensions
        {
            get
            {
                return OpenFileDialog.SupportMultiDottedExtensions;
            }
            set
            {
                OpenFileDialog.SupportMultiDottedExtensions = value;
            }
        }

        [Localizable(true)]
        [DefaultValue("")]
   
        public string Title
        {
            get
            {
                return OpenFileDialog.Title;
            }
            set
            {
                OpenFileDialog.Title = value;
            }
        }
        [DefaultValue(true)]
        public bool ValidateNames
        {
            get
            {
                return OpenFileDialog.ValidateNames;
            }
            set
            {
                OpenFileDialog.ValidateNames = value;
            }
        }

        private void StartWatching()
        {
            m_IsWatching = true;

            Thread t = new Thread(new ThreadStart(CheckActiveWindow));
            t.IsBackground = true;
            t.Start();
        }

        public virtual DialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        public virtual DialogResult ShowDialog(IWin32Window owner)
        {
            IMiniToolbarService miniToolbarService = ServiceContainerCF.GetService<IMiniToolbarService>();
            miniToolbarService.Hide();

            StartWatching();

            DialogResult dialogResult = OpenFileDialog.ShowDialog(owner);

            StopWatching();

            return dialogResult;
 
        }

        private void StopWatching()
        {
            m_IsWatching = false;
        }

        private void CheckActiveWindow()
        {
            lock (this)
            {
                uint listviewHandle = 0;

                while (listviewHandle == 0 && m_IsWatching)
                {
                    listviewHandle = FindWindowEx(GetForegroundWindow(), 0, "SHELLDLL_DefView", "");
                }

                if (listviewHandle != 0)
                {
                    SendMessage(listviewHandle, WM_COMMAND, (uint)this.m_DialogViewTypes, 0);
                }
            }
        }
    }

}
