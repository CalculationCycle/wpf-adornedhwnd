#include <windows.h>
#include <set>

const char g_szClassName[] = "miniChildWindowClass";


int ChildWindow_WMEraseBkgnd(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	return 0;
}

HBRUSH _gBackgroundBrush;
HBRUSH _gLeftMouseButtonBrush;
HBRUSH _gRightMouseButtonBrush;
HBRUSH _gMouseEnterBrush;
HBRUSH _gMouseLeaveBrush;
bool _gLeftMButtonBrushSwapped;
bool _gRightMButtonBrushSwapped;
HWND _gMouseOverChildWindowHwnd;

void OnPaint(HWND hwnd, HDC hdc, RECT rc)
{
	FillRect(hdc, &rc, _gBackgroundBrush);
	int w = (rc.right - rc.left);
	int h = (rc.bottom - rc.top);
	if (w < h)
		w = w / 20;
	else
		w = h / 20;

	if (w < 8)
		w = 8;
	if (w > 30)
		w = 30;

	RECT rcLeftMButton;
	rcLeftMButton.left = w;
	rcLeftMButton.right = 3*w;
	rcLeftMButton.top = w;
	rcLeftMButton.bottom = 3*w;
	FillRect(hdc, &rcLeftMButton, _gLeftMouseButtonBrush);
	RECT rcRightMButton;
	rcRightMButton.left = 4*w - 5;
	rcRightMButton.right = 6*w - 5;
	rcRightMButton.top = w;
	rcRightMButton.bottom = 3*w;
	FillRect(hdc, &rcRightMButton, _gRightMouseButtonBrush);
}

int ChildWindow_WMPaint(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{	
	PAINTSTRUCT paint;
	HDC hDc = BeginPaint(hwnd, &paint);
	OnPaint(hwnd, hDc, paint.rcPaint);
	EndPaint(hwnd, &paint);
	return 0;
}

int ChildWindow_WMSize(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	RECT rect;
	HWND parentHwnd = GetParent(hwnd);
	GetWindowRect(parentHwnd, &rect);
	MoveWindow(hwnd, 0, 0, rect.right - rect.left, rect.bottom - rect.top, true);
	return 0;
}

int ChildWindow_WMLButtonUp(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	if (!_gLeftMButtonBrushSwapped)
	{
		_gLeftMouseButtonBrush = CreateSolidBrush(RGB(210,150,150));
	}
	else
	{
		_gLeftMouseButtonBrush = CreateSolidBrush(RGB(160,120,120));
	}
	_gLeftMButtonBrushSwapped = !_gLeftMButtonBrushSwapped;
	InvalidateRect(hwnd, 0, true);
	return 0;
}

int ChildWindow_WMRButtonUp(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	if (!_gRightMButtonBrushSwapped)
	{
		_gRightMouseButtonBrush = CreateSolidBrush(RGB(150, 210, 150));
	}
	else
	{
		_gRightMouseButtonBrush = CreateSolidBrush(RGB(120, 160, 120));
	}
	_gRightMButtonBrushSwapped = !_gRightMButtonBrushSwapped;
	InvalidateRect(hwnd, 0, true);
	return 0;
}

int ChildWindow_WMMouseMove(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	if (_gMouseOverChildWindowHwnd != hwnd)
	{
		_gMouseOverChildWindowHwnd = hwnd;
		_gBackgroundBrush = CreateSolidBrush(RGB(255, 255, 210));
		InvalidateRect(hwnd, 0, true);
		TRACKMOUSEEVENT tme;
		tme.cbSize = sizeof(TRACKMOUSEEVENT);
		tme.dwFlags = TME_LEAVE;
		tme.hwndTrack = hwnd;
		TrackMouseEvent(&tme);
	}
	return 0;
}

int ChildWindow_WMMouseLeave(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	_gMouseOverChildWindowHwnd = 0;
	_gBackgroundBrush = CreateSolidBrush(RGB(255, 255, 175));
	InvalidateRect(hwnd, 0, true);
	return 0;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
	case WM_CLOSE:
		DestroyWindow(hwnd);
		break;
	case WM_DESTROY:
		PostQuitMessage(0);
		break;
	case WM_ERASEBKGND:
		return ChildWindow_WMEraseBkgnd(hwnd, msg, wParam, lParam);
	case WM_PAINT:
		return ChildWindow_WMPaint(hwnd, msg, wParam, lParam);
	case WM_SIZE:
		return ChildWindow_WMSize(hwnd, msg, wParam, lParam);
	case WM_LBUTTONUP:
		return ChildWindow_WMLButtonUp(hwnd, msg, wParam, lParam);
	case WM_RBUTTONUP:
		return ChildWindow_WMRButtonUp(hwnd, msg, wParam, lParam);
	case WM_MOUSEMOVE:
		return ChildWindow_WMMouseMove(hwnd, msg, wParam, lParam);
	case WM_MOUSELEAVE:
		return ChildWindow_WMMouseLeave(hwnd, msg, wParam, lParam);
	default:
		return DefWindowProc(hwnd, msg, wParam, lParam);
	}
	return 0;
}



class ChildWindowManager
{
	private:
		std::set<HWND> _cHwnds;
		HINSTANCE _hInst;

	public:
		ChildWindowManager(HINSTANCE hInst);
		~ChildWindowManager();

		HWND NewChildWindow(HWND parentWin);
		BOOL DeleteChildWindow(HWND childWin);
		BOOL DestroyChildWindow(HWND childWin);
};

ChildWindowManager::ChildWindowManager(HINSTANCE hInst)
{
	_gMouseOverChildWindowHwnd = 0;
	_gBackgroundBrush = CreateSolidBrush(RGB(255, 255, 175));
	_gLeftMouseButtonBrush = CreateSolidBrush(RGB(160, 120, 120));
	_gRightMouseButtonBrush = CreateSolidBrush(RGB(120, 160, 120));
	_gMouseEnterBrush = CreateSolidBrush(RGB(127, 255, 127));
	_gMouseLeaveBrush = CreateSolidBrush(RGB(127, 255, 127));
	_gLeftMButtonBrushSwapped = false;
	_gRightMButtonBrushSwapped = false;

	_hInst = hInst;

	WNDCLASSEX wc;
	wc.cbSize = sizeof(WNDCLASSEX);
	wc.style = 0;
	wc.lpfnWndProc = WndProc;
	wc.cbClsExtra = 0;
	wc.cbWndExtra = 0;
	wc.hInstance = hInst;
	wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground = 0;
	wc.lpszMenuName = NULL;
	wc.lpszClassName = LPCWSTR(g_szClassName);
	wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);

	if (!RegisterClassEx(&wc))
	{
		MessageBox(NULL, L"False from RegisterClassEx!", L"Error!", MB_ICONEXCLAMATION | MB_OK);
	}
}

ChildWindowManager::~ChildWindowManager()
{
	for each (HWND h in _cHwnds)
	{
		DestroyChildWindow(h);
	}
	UnregisterClass(LPCWSTR(g_szClassName), _hInst);
}

HWND ChildWindowManager::NewChildWindow(HWND parentWin)
{
	if (IsWindow(parentWin))
	{
		HWND hwnd = CreateWindowEx(
			WS_EX_TOOLWINDOW,
			LPWSTR(g_szClassName),
			L"The title of my window",
			WS_CLIPCHILDREN | WS_CLIPSIBLINGS
			| WS_CHILD
			,
			CW_USEDEFAULT, CW_USEDEFAULT, 240, 120,
			parentWin, 
			NULL, _hInst, NULL);

		if (hwnd == NULL)
		{
			wchar_t buf[256];
			FormatMessageW(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
				NULL, GetLastError(), MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
				buf, (sizeof(buf) / sizeof(wchar_t)), NULL);

			MessageBox(NULL, L"null fromCreateWindowEx!", L"Error!", MB_ICONEXCLAMATION | MB_OK);
			return 0;
		}
		else
		{
			SetParent(hwnd, parentWin);
			ShowWindow(hwnd, WS_VISIBLE);
			RECT rect;
			GetWindowRect(parentWin, &rect);
			MoveWindow(hwnd, 0, 0, rect.right - rect.left, rect.top - rect.bottom, true);
			_cHwnds.insert(hwnd);
			return hwnd;
		}
	}
	else
		return 0;
}

BOOL ChildWindowManager::DeleteChildWindow(HWND childWin)
{
 	if (IsWindow(childWin) && (_cHwnds.find(childWin) != _cHwnds.end()))
	{
		_cHwnds.erase(childWin);

		return DestroyChildWindow(childWin);
	}
	else
		return false;
}

BOOL ChildWindowManager::DestroyChildWindow(HWND childWin)
{
	if (IsWindow(childWin))
	{
		ShowWindow(childWin, SW_HIDE);
		SetParent(childWin, 0);
		return DestroyWindow(childWin);
	}
	else
		return false;
}

ChildWindowManager* childWindowManager;

extern "C" __declspec(dllexport) int NewChildWindow(int parentHwnd)
{
	return (int)childWindowManager->NewChildWindow((HWND)parentHwnd);
}

extern "C" __declspec(dllexport) bool DeleteChildWindow(int childHwnd)
{
	return childWindowManager->DeleteChildWindow((HWND)childHwnd);
}

BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH) {
		childWindowManager = new ChildWindowManager((HINSTANCE)hModule);
	}
	else if (ul_reason_for_call == DLL_PROCESS_DETACH) {
		delete childWindowManager;
	}
	return TRUE;
}