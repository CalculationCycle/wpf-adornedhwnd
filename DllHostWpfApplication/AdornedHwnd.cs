using System;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;
using cw = ChildWindowInDll;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Input;

namespace AdornedHwnd
{
    static class w32w
    {
        #region consts
        public const int GWLP_ID = -12;
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        public const uint WS_OVERLAPPED = 0x00000000;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x08000000;
        public const uint WS_CLIPSIBLINGS = 0x04000000;
        public const uint WS_CLIPCHILDREN = 0x02000000;
        public const uint WS_MAXIMIZE = 0x01000000;
        public const uint WS_BORDER = 0x00800000;
        public const uint WS_DLGFRAME = 0x00400000;
        public const uint WS_VSCROLL = 0x00200000;
        public const uint WS_HSCROLL = 0x00100000;
        public const uint WS_SYSMENU = 0x00080000;
        public const uint WS_THICKFRAME = 0x00040000;
        public const uint WS_GROUP = 0x00020000;
        public const uint WS_TABSTOP = 0x00010000;

        public const uint WS_MINIMIZEBOX = 0x00020000;
        public const uint WS_MAXIMIZEBOX = 0x00010000;

        public const uint WS_CAPTION = WS_BORDER | WS_DLGFRAME;
        public const uint WS_TILED = WS_OVERLAPPED;
        public const uint WS_ICONIC = WS_MINIMIZE;
        public const uint WS_SIZEBOX = WS_THICKFRAME;
        public const uint WS_TILEDWINDOW = WS_OVERLAPPEDWINDOW;

        public const uint WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
        public const uint WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU;
        public const uint WS_CHILDWINDOW = WS_CHILD;

        public const uint WS_EX_CONTROLPARENT = 0x00010000;
        #endregion

        #region DllImports
        [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        public static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        #endregion
    }

    internal class AdornerGuiWindow : Window
    {
        private bool _wschild = false;
        
        public AdornerGuiWindow(bool wschild)
        {
            _wschild = wschild;            
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (_wschild)
            {
                WindowInteropHelper wndHelper = new WindowInteropHelper(this);

                IntPtr wLong = w32w.GetWindowLongPtr(wndHelper.Handle, w32w.GWLP_ID);
                IntPtr wLongStyle = w32w.GetWindowLongPtr(wndHelper.Handle, w32w.GWL_STYLE);
                IntPtr wLongExtStyle = w32w.GetWindowLongPtr(wndHelper.Handle, w32w.GWL_EXSTYLE);

                int newLongStyle = wLongStyle.ToInt32();
                newLongStyle |= (int)(w32w.WS_CHILD | w32w.WS_CLIPCHILDREN | w32w.WS_CLIPSIBLINGS);
                newLongStyle &= ~unchecked((int)(w32w.WS_POPUP));
                newLongStyle &= ~unchecked((int)(w32w.WS_POPUPWINDOW));
                IntPtr setwLongStyle = new IntPtr(newLongStyle);
                IntPtr setWindowLongResult = w32w.SetWindowLongPtr(wndHelper.Handle, w32w.GWL_STYLE, setwLongStyle);
                if (setWindowLongResult.ToInt32() == 0)
                {
                    int winErr = Marshal.GetLastWin32Error();
                    Console.WriteLine("WinError: " + winErr.ToString());
                }
                int newLongExtStyle = wLongExtStyle.ToInt32();
                newLongExtStyle |= (int)w32w.WS_EX_CONTROLPARENT;
                IntPtr setWLongExtStyle = new IntPtr(newLongExtStyle);
                setWindowLongResult = w32w.SetWindowLongPtr(wndHelper.Handle, w32w.GWL_EXSTYLE, setWLongExtStyle);
                if (setWindowLongResult.ToInt32() == 0)
                {
                    int winErr = Marshal.GetLastWin32Error();
                    Console.WriteLine("WinError: " + winErr.ToString());
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true;
        }
    }

    internal class ChildWindowHwndHost : HwndHost
    {
        private int childHwnd = 0;

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                childHwnd = cw.DllIntf.NewChildWindow((int)hwndParent.Handle);
            }
            return new HandleRef(this, new IntPtr(childHwnd));
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            if (childHwnd > 0)
            {
                cw.DllIntf.DeleteChildWindow(childHwnd);
            }
        }
    }

    [ContentProperty(nameof(Children))]
    public class AdornedHwndHost : ContentControl
    {
        private Window _ownerWindow;
        private ChildWindowHwndHost _hwndHost;
        private bool _guiOverlayVisible = true;
        private Grid _guiWindowGrid;
        private readonly TextBlock _designModeText;
        private AdornerGuiWindow _guiWindow;
        private double _dpiWidthFactor = 1;
        private double _dpiHeightFactor = 1;
        private bool _hostChildWin = false;
        private bool _guiWinWSChild = false;


        #region Properties
        [Description("Description"), Category("AdornedHwnd")]
        public bool GuiOverlayVisible
        {
            get { return _guiOverlayVisible; }
            set
            {
                _guiOverlayVisible = value;
                ShowHideGuiWindow();
            }
        }

        [Description("Description"), Category("AdornedHwnd")]
        public bool HostChildWin
        {
            get { return _hostChildWin; }
            set { _hostChildWin = value; }
        }


        [Description("Description"), Category("AdornedHwnd")]
        public bool GuiWinWSChild
        {
            get { return _guiWinWSChild; }
            set { _guiWinWSChild = value; }
        }

        public static readonly DependencyPropertyKey ChildrenProperty =
            DependencyProperty.RegisterReadOnly(nameof(Children),
            typeof(UIElementCollection), typeof(AdornedHwndHost), new PropertyMetadata());

        public UIElementCollection Children
        {
            get
            {
                if ((DesignerProperties.GetIsInDesignMode(this)) && (GetValue(ChildrenProperty.DependencyProperty) == null))
                {
                    throw new Exception("cgsMonitor not allowed to have children.");
                }
                else
                {
                    return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty);
                }
            }
            private set { SetValue(ChildrenProperty, value); }
        }
        #endregion

        public AdornedHwndHost()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;
            }
            LayoutUpdated += OnLayoutUpdated;

            _guiWindowGrid = new Grid();
            _guiWindowGrid.VerticalAlignment = VerticalAlignment.Stretch;
            _guiWindowGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                Content = _guiWindowGrid;

                SolidColorBrush _gridBackgruondBrush = new SolidColorBrush();
                Byte bgColor = 235;
                _gridBackgruondBrush.Color = Color.FromRgb(bgColor, bgColor, bgColor);
                _guiWindowGrid.Background = _gridBackgruondBrush;
                _designModeText = new TextBlock();
                _designModeText.Text = "ChildWin32";
                _guiWindowGrid.Children.Add(_designModeText);
                _designModeText.HorizontalAlignment = HorizontalAlignment.Center;
                _designModeText.VerticalAlignment = VerticalAlignment.Top;
                _designModeText.Margin = new Thickness(0, 10, 0, 0);
                _designModeText.FontFamily = new FontFamily("Arial Unicode MS");
                _designModeText.Width = 140;
                _designModeText.Height = 40;
                _designModeText.FontSize = 24;
                _designModeText.FontWeight = FontWeights.Bold;
                _designModeText.TextAlignment = TextAlignment.Center;
                SolidColorBrush textColorBrush = new SolidColorBrush();
                Byte textColor = 215;
                textColorBrush.Color = Color.FromRgb(textColor, textColor, textColor);
                _designModeText.Foreground = textColorBrush;

                Grid _designerModeGrid = new Grid();
                _guiWindowGrid.Children.Add(_designerModeGrid);
                Children = _designerModeGrid.Children;
            }
            else
            {
                Children = _guiWindowGrid.Children;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                CalcDpiFactors();

                _guiWindowGrid.Name = "_guiWindowGrid_" + Name;
                if (_hostChildWin)
                {
                    _hwndHost = new ChildWindowHwndHost();
                    _hwndHost.Name = "_hwndHost_" + Name;
                    Content = _hwndHost;
                }

                _ownerWindow = Window.GetWindow(this);
                _ownerWindow.LocationChanged += OwnerWindow_LocationChanged;

                _guiWindow = new AdornerGuiWindow(_guiWinWSChild)
                {
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None,
                    Background = Brushes.Transparent,
                    AllowsTransparency = true,
                    ResizeMode = ResizeMode.NoResize,
                    Owner = Window.GetWindow(this)
                };
                _guiWindow.Name = "_guiWindow_" + Name;

                _guiWindow.Content = _guiWindowGrid;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_ownerWindow != null)
            {
                _ownerWindow.LocationChanged -= OwnerWindow_LocationChanged;
            }
            if (_guiWindow != null)
            {
                _guiWindow.Close();
                _guiWindow = null;
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            ShowHideGuiWindow();
            Resize();
        }

        private void OwnerWindow_LocationChanged(object sender, EventArgs e)
        {
            Resize();
        }

        private void Resize()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (_guiOverlayVisible && (_guiWindow != null) && (_guiWindow.IsVisible == true) && (PresentationSource.FromVisual(this) != null))
                {
                    _guiWindow.Width = ActualWidth;
                    _guiWindow.Height = ActualHeight;
                    Point p = PointToScreen(new Point(0, 0));
                    _guiWindow.Top = p.Y / _dpiHeightFactor;
                    _guiWindow.Left = p.X / _dpiWidthFactor;
                }
            }
        }

        private void ShowHideGuiWindow()
        {

            if (_guiWindow != null)
            {
                if ((_guiOverlayVisible) && (_guiWindow.Visibility != Visibility.Visible))
                {
                    _guiWindow.Show();
                    Console.WriteLine("Showing " + _guiWindow.Name);
                }
                else if ((!_guiOverlayVisible) && (_guiWindow.Visibility == Visibility.Visible))
                {
                    _guiWindow.Hide();
                    Console.WriteLine("Hiding " + _guiWindow.Name);
                }
            }

        }

        private void CalcDpiFactors()
        {
            PresentationSource source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                _dpiWidthFactor = source.CompositionTarget.TransformToDevice.M11;
                _dpiHeightFactor = source.CompositionTarget.TransformToDevice.M22;
            }
        }
    }
}