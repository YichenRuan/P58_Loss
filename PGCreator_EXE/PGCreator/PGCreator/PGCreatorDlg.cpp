
// PGCreatorDlg.cpp : ʵ���ļ�
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

// CPGCreatorDlg �Ի���

bool isFireProOpen = false;

CString CPGCreatorDlg::ccl = L"";

CPGCreatorDlg::CPGCreatorDlg(CWnd* pParent /*=NULL*/)
	: CDialogEx(CPGCreatorDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_brush.CreateSolidBrush(RGB(200,200,200));
	char* temp_inName = "PGCTF.IN";
	char* temp_in2Name = "FPTF.IN2";
	char* temp_outName = "PGCTF.OUT";
	char* temp_out2Name = "FPTF.OUT2";
	char* temp_binName = "MEPTF.dat";
	strcpy_s(inFileName,temp_inName);
	strcpy_s(in2FileName,temp_in2Name);
	strcpy_s(outFileName,temp_outName);
	strcpy_s(out2FileName,temp_out2Name);
	strcpy_s(binFileName,temp_binName);
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


// CPGCreatorDlg ��Ϣ�������

BOOL CPGCreatorDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// ���ô˶Ի����ͼ�ꡣ��Ӧ�ó��������ڲ��ǶԻ���ʱ����ܽ��Զ�
	//  ִ�д˲���
	SetIcon(m_hIcon, TRUE);			// ���ô�ͼ��
	SetIcon(m_hIcon, FALSE);		// ����Сͼ��

	// TODO: �ڴ���Ӷ���ĳ�ʼ������
	//USES_CONVERSION;
	//AfxMessageBox(A2T(GetProgramDir()));
	int nArgs = 0;
	LPWSTR *szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);

	if (nArgs <= 1)
	{
		AfxMessageBox(L"�޷�����PGCreator");
		EndDialog(IDCANCEL);
		return TRUE;
	}

	CString cs(szArglist[1]);
	for (int i = 2; i < nArgs; ++i)
	{
		CString temp(szArglist[i]);
		cs += L" " + temp;
	}
	
	commandLine = nullptr;
	USES_CONVERSION;
	commandLine = T2A(cs);
	ccl = cs;

	if (commandLine == nullptr || commandLine[0] == '\0')
	{
		AfxMessageBox(L"�޷�����PGCreator");
		EndDialog(IDCANCEL);
		return TRUE;
	}
	strcpy_s(inPath, commandLine);
	strcpy_s(in2Path, commandLine);
	strcpy_s(outPath, commandLine);
	strcpy_s(out2Path, commandLine);
	strcpy_s(binPath, commandLine);
	if (!ReadInFile())
	{
		AfxMessageBox(L"�޷�����PGCreator");
		EndDialog(IDCANCEL);
	}
	else
	{
		m_tab.InsertItem(0,L"��Ŀ��Ϣ");
		m_tab.InsertItem(1,L"��ߵ���");
		m_tab.InsertItem(2,L"Ĭ��ֵ����");
		m_tab.InsertItem(3,L"��������");
		m_tab.InsertItem(4,L"����ӳ��");
		m_tab.InsertItem(5,L"MEP����");

		m_infoDlg.Create(IDD_INFO_DIALOG,GetDlgItem(IDC_TAB));
		m_levelDlg.Create(IDD_LEVEL_DIALOG,GetDlgItem(IDC_TAB));
		m_defaultDlg.Create(IDD_DEFAULT_DIALOG,GetDlgItem(IDC_TAB));
		m_compDlg.Create(IDD_COMP_DIALOG,GetDlgItem(IDC_TAB));
		m_mateDlg.Create(IDD_MATERIAL_DIALOG,GetDlgItem(IDC_TAB));
		m_mepDlg.Create(IDD_MEP_DIALOG,GetDlgItem(IDC_TAB));

		CRect rs;
		m_tab.GetClientRect(&rs);
		rs.top += 22;
		m_infoDlg.MoveWindow(&rs);
		m_levelDlg.MoveWindow(&rs);
		m_defaultDlg.MoveWindow(&rs);
		m_compDlg.MoveWindow(&rs);
		m_mateDlg.MoveWindow(&rs);


		
		m_mepDlg.MoveWindow(&rs);

		if (isFireProOpen)
		{
			if (ReadIn2File())
			{
				m_tab.InsertItem(6,L"�����豸");
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

	return TRUE;  // ���ǽ��������õ��ؼ������򷵻� TRUE
}

// �����Ի��������С����ť������Ҫ����Ĵ���
//  �����Ƹ�ͼ�ꡣ����ʹ���ĵ�/��ͼģ�͵� MFC Ӧ�ó���
//  �⽫�ɿ���Զ���ɡ�

void CPGCreatorDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // ���ڻ��Ƶ��豸������

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// ʹͼ���ڹ����������о���
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// ����ͼ��
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

//���û��϶���С������ʱϵͳ���ô˺���ȡ�ù��
//��ʾ��
HCURSOR CPGCreatorDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}



void CPGCreatorDlg::OnSelchangeTab(NMHDR *pNMHDR, LRESULT *pResult)
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
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
		m_mepDlg.ShowWindow(FALSE);
		break;
	case 1:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(TRUE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		m_mepDlg.ShowWindow(FALSE);
		break;
	case 2:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(TRUE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		m_mepDlg.ShowWindow(FALSE);
		break;
	case 3:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(TRUE);
		m_mateDlg.ShowWindow(FALSE);
		m_mepDlg.ShowWindow(FALSE);
		break;
	case 4:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(TRUE);
		m_mepDlg.ShowWindow(FALSE);
		break;
	case 5:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		m_mepDlg.ShowWindow(TRUE);
		break;
	case 6:
		m_infoDlg.ShowWindow(FALSE);
		m_levelDlg.ShowWindow(FALSE);
		m_defaultDlg.ShowWindow(FALSE);
		m_compDlg.ShowWindow(FALSE);
		m_mateDlg.ShowWindow(FALSE);
		m_mepDlg.ShowWindow(FALSE);
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
	m_mepDlg.InFileInterpret(inFile, ++i);
}

void CPGCreatorDlg::OnBnClickedOk()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
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

	FILE* fp_b;
	strcat_s(binPath,binFileName);
	SetFileAttributes((CString)binPath,FILE_ATTRIBUTE_NORMAL);
	fopen_s(&fp_b,binPath,"wb");
	m_mepDlg.OutputInfo(fp_b);
	fclose(fp_b);

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
	// TODO: �ڴ����ר�ô����/����û���
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	else	return CDialogEx::PreTranslateMessage(pMsg);
}


void CPGCreatorDlg::OnBnClickedCancel()
{
	// TODO: �ڴ���ӿؼ�֪ͨ����������
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

CString CPGCreatorDlg::GetMyCommandLine()
{
	return ccl;
}