
// PGCreatorDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "PGCreatorDlg.h"
#include "afxdialogex.h"
#include "direct.h"
#include "string.h"
#include <queue>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CPGCreatorDlg 对话框

bool isFireProOpen = true;

CPGCreatorDlg::CPGCreatorDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CPGCreatorDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_brush.CreateSolidBrush(RGB(200,200,200));
	char* temp_inName = "\\PGCreator\\PGCTF.IN";
	char* temp_in2Name = "\\PGCreator\\FPTF.IN2";
	char* temp_outName = "\\PGCreator\\PGCTF.OUT";
	char* temp_out2Name = "\\PGCreator\\FPTF.OUT2";
	strcpy_s(inFileName,temp_inName);
	strcpy_s(in2FileName,temp_in2Name);
	strcpy_s(outFileName,temp_outName);
	strcpy_s(out2FileName,temp_out2Name);
}

void CPGCreatorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_TAB, m_tab);
}

BEGIN_MESSAGE_MAP(CPGCreatorDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_NOTIFY(TCN_SELCHANGE, IDC_TAB, &CPGCreatorDlg::OnSelchangeTab)
	ON_WM_CTLCOLOR()
	ON_BN_CLICKED(IDOK, &CPGCreatorDlg::OnBnClickedOk)
	ON_WM_DRAWITEM()
	ON_BN_CLICKED(IDCANCEL, &CPGCreatorDlg::OnBnClickedCancel)
END_MESSAGE_MAP()


// CPGCreatorDlg 消息处理程序

BOOL CPGCreatorDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// 设置此对话框的图标。当应用程序主窗口不是对话框时，框架将自动
	//  执行此操作
	SetIcon(m_hIcon, TRUE);			// 设置大图标
	SetIcon(m_hIcon, FALSE);		// 设置小图标

	// TODO: 在此添加额外的初始化代码
	//USES_CONVERSION;
	//AfxMessageBox(A2T(GetProgramDir()));
	_getcwd(inPath,MAX_PATH);
	_getcwd(in2Path,MAX_PATH);
	_getcwd(outPath,MAX_PATH);
	_getcwd(out2Path,MAX_PATH);
	if (!ReadInFile())
	{
		AfxMessageBox(L"无法启动PGCreator，请通过Revit进入程序.");
		EndDialog(IDCANCEL);
	}
	else
	{
		m_tab.InsertItem(0,L"项目信息");
		m_tab.InsertItem(1,L"标高调整");
		m_tab.InsertItem(2,L"默认值设置");
		m_tab.InsertItem(3,L"构件设置");
		m_tab.InsertItem(4,L"材质映射");

		m_infoDlg.Create(IDD_INFO_DIALOG,GetDlgItem(IDC_TAB));
		m_levelDlg.Create(IDD_LEVEL_DIALOG,GetDlgItem(IDC_TAB));
		m_defaultDlg.Create(IDD_DEFAULT_DIALOG,GetDlgItem(IDC_TAB));
		m_compDlg.Create(IDD_COMP_DIALOG,GetDlgItem(IDC_TAB));
		m_mateDlg.Create(IDD_MATERIAL_DIALOG,GetDlgItem(IDC_TAB));

		CRect rs;
		m_tab.GetClientRect(&rs);
		rs.top += 22;
		m_infoDlg.MoveWindow(&rs);
		m_levelDlg.MoveWindow(&rs);
		m_defaultDlg.MoveWindow(&rs);
		m_compDlg.MoveWindow(&rs);
		m_mateDlg.MoveWindow(&rs);

		if (isFireProOpen)
		{
			if (ReadIn2File())
			{
				m_tab.InsertItem(5,L"消防设备");
				m_fpDlg.Create(IDD_FIREPRO_DIALOG,GetDlgItem(IDC_TAB));
				m_fpDlg.MoveWindow(&rs);
				m_fpDlg.In2FileInterpret(in2File);
			}
			else isFireProOpen = false;
		}

		m_infoDlg.ShowWindow(TRUE);
		m_tab.SetCurSel(0);
		UseInFile();
	}

	return TRUE;  // 除非将焦点设置到控件，否则返回 TRUE
}

// 如果向对话框添加最小化按钮，则需要下面的代码
//  来绘制该图标。对于使用文档/视图模型的 MFC 应用程序，
//  这将由框架自动完成。

void CPGCreatorDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // 用于绘制的设备上下文

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// 使图标在工作区矩形中居中
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// 绘制图标
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

//当用户拖动最小化窗口时系统调用此函数取得光标
//显示。
HCURSOR CPGCreatorDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



void CPGCreatorDlg::OnSelchangeTab(NMHDR *pNMHDR, LRESULT *pResult)
{
	// TODO: 在此添加控件通知处理程序代码
	*pResult = 0;
	int CurSel = m_tab.GetCurSel(); 
	if (isFireProOpen) m_fpDlg.ShowWindow(FALSE);
	switch (CurSel)
	{
	case 0:
		m_infoDlg.ShowWindow(TRUE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		break;
	case 1:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(TRUE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		break;
	case 2:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(TRUE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		break;
	case 3:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(TRUE);
		m_mateDlg.ShowWindow(FALSE);
		break;
	case 4:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(TRUE);
		break;
	case 5:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		m_fpDlg.ShowWindow(TRUE);
		break;
	default:
		break;
	}
}


HBRUSH CPGCreatorDlg::OnCtlColor(CDC* pDC, CWnd* pWnd, UINT nCtlColor)
{
	return m_brush;
}

bool CPGCreatorDlg::ReadInFile()
{
	FILE* fp;
	//char reWrite[30] = "DO NOT EDIT OR DELETE!!!";
	strcat_s(inPath,inFileName);
	fopen_s(&fp,inPath,"r+");
	if (fp != NULL)
	{
		fread(&inFile,sizeof(char),BUFF_IN,fp);
		SetFileAttributes((CString)inPath,FILE_ATTRIBUTE_HIDDEN);
		fclose(fp);
		return true;
	}
	else
	{
		return false;
	}
	/*
	SetFileAttributes(C_inPath,FILE_ATTRIBUTE_NORMAL);
	fopen_s(&fp,inPath,"w");
	fprintf_s(fp,"%s",reWrite);
	SetFileAttributes(C_inPath,FILE_ATTRIBUTE_HIDDEN);
	fclose(fp);
	*/
}

bool CPGCreatorDlg::ReadIn2File()
{
	FILE* fp;
	strcat_s(in2Path,in2FileName);
	fopen_s(&fp,in2Path,"r+");
	if (fp != NULL)
	{
		fread(&in2File,sizeof(char),BUFF_IN,fp);
		SetFileAttributes((CString)in2Path,FILE_ATTRIBUTE_HIDDEN);
		fclose(fp);
		return true;
	}
	else return false;
}

void CPGCreatorDlg::UseInFile()
{
	std::queue<int> infoQueue;
	std::queue<int> levelQueue;
	int i = 2;
	infoQueue.push(2);
	while (inFile[i] != '\n')
	{
		if (inFile[i] == '\t')
		{
			inFile[i] = '\0';
			infoQueue.push(i+1);
		}
		++i;
	}

	levelQueue.push(++i);
	while (inFile[i] != '\n')
	{
		if (inFile[i] == '\t')
		{
			inFile[i] = '\0';
			levelQueue.push(i+1);
		}
		++i;
	}
	
	m_infoDlg.InFileInterpret(inFile,infoQueue);
	m_levelDlg.InFileInterpret(inFile,levelQueue);
}

void CPGCreatorDlg::OnBnClickedOk()
{
	// TODO: 在此添加控件通知处理程序代码
	FILE* fp;
	strcat_s(outPath,outFileName);
	SetFileAttributes((CString)outPath,FILE_ATTRIBUTE_NORMAL);
	fopen_s(&fp,outPath,"w+");
	fprintf_s(fp,"0\n%s\n",m_infoDlg.GetFilePath());
	m_infoDlg.OutputInfo(fp);
	m_levelDlg.OutputInfo(fp);
	m_defaultDlg.OutputInfo(fp);
	m_compDlg.OutputInfo(fp);
	m_mateDlg.OutputInfo(fp);
	m_defaultDlg.OutputSetting(fp);
	fclose(fp);

	if (isFireProOpen)
	{
		FILE* fp2;
		strcat_s(out2Path,out2FileName);
		SetFileAttributes((CString)out2Path,FILE_ATTRIBUTE_NORMAL);
		fopen_s(&fp2,out2Path,"w+");
		m_fpDlg.OutputInfo(fp2);
		fclose(fp2);
	}

	CDialogEx::OnOK();
}

/*
char* CPGCreatorDlg::GetProgramDir()  
{   
	TCHAR pFileName[MAX_PATH]; 
	int nPos = GetCurrentDirectory( MAX_PATH, pFileName);
	char* c_fileName = new char[MAX_PATH];
	m_infoDlg.TcharToChar(pFileName,c_fileName);
	return c_fileName;
}   
*/


BOOL CPGCreatorDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	else	return CDialogEx::PreTranslateMessage(pMsg);
}


void CPGCreatorDlg::OnBnClickedCancel()
{
	// TODO: 在此添加控件通知处理程序代码
	FILE* fp;
	strcat_s(outPath,outFileName);
	SetFileAttributes((CString)outPath,FILE_ATTRIBUTE_NORMAL);
	fopen_s(&fp,outPath,"w+");
	fprintf_s(fp,"1\n");
	fclose(fp);
	CDialogEx::OnCancel();
}

void CPGCreatorDlg::TcharToChar (const TCHAR * tchar, char * _char)  
{  
	int iLength ;    
	iLength = WideCharToMultiByte(CP_ACP, 0, tchar, -1, NULL, 0, NULL, NULL);    
	WideCharToMultiByte(CP_ACP, 0, tchar, -1, _char, iLength, NULL, NULL);   
}  

void CPGCreatorDlg::CharToTchar (const char * _char, TCHAR * tchar)  
{  
	int iLength ;  

	iLength = MultiByteToWideChar (CP_ACP, 0, _char, strlen (_char) + 1, NULL, 0) ;  
	MultiByteToWideChar (CP_ACP, 0, _char, strlen (_char) + 1, tchar, iLength) ;  
} 