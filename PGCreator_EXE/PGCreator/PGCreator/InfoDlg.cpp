// InfoDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "PGCreatorDlg.h"
#include "InfoDlg.h"
#include "afxdialogex.h"
#include <queue>


// CInfoDlg 对话框

IMPLEMENT_DYNAMIC(CInfoDlg, CDialog)

CInfoDlg::CInfoDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CInfoDlg::IDD, pParent)
{
	SHGetSpecialFolderPath(this->GetSafeHwnd(),filePath,CSIDL_PERSONAL,0);
}

CInfoDlg::~CInfoDlg()
{
}

void CInfoDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_BUTTON_PATH, m_Bpath);
	//  DDX_Control(pDX, IDC_EDIT_PATH, m_Epath);
	//  DDX_Control(pDX, IDC_EDIT_FILE, m_Efile);
	//  DDX_Control(pDX, IDC_EDIT_BLDG, m_Ebldg);
	//  DDX_Control(pDX, IDC_EDIT_ADDRESS, m_Eaddress);
}


BEGIN_MESSAGE_MAP(CInfoDlg, CDialog)
	ON_WM_DRAWITEM()
	ON_BN_CLICKED(IDC_BUTTON_PATH, &CInfoDlg::OnClickedButtonPath)
	//ON_EN_KILLFOCUS(IDC_EDIT_FILE, &CInfoDlg::OnKillfocusEditFile)
	//ON_EN_KILLFOCUS(IDC_EDIT_BLDG, &CInfoDlg::OnKillfocusEditBldg)
	//ON_EN_KILLFOCUS(IDC_EDIT_ADDRESS, &CInfoDlg::OnKillfocusEditAddress)
	//ON_EN_KILLFOCUS(IDC_EDIT_PROJ, &CInfoDlg::OnKillfocusEditProj)
END_MESSAGE_MAP()


// CInfoDlg 消息处理程序

BOOL CInfoDlg::OnInitDialog()
{
	SetDlgItemTextW(IDC_EDIT_PATH,filePath);
	return TRUE;
}


void CInfoDlg::OnClickedButtonPath()
{
	// TODO: 在此添加控件通知处理程序代码
	BROWSEINFO bi;    
	ZeroMemory(&bi, sizeof(BROWSEINFO));  
	bi.hwndOwner = m_hWnd;     
	bi.pidlRoot = NULL;     
	bi.pszDisplayName = filePath;     
	bi.lpszTitle = L"请选择路径";   
	bi.ulFlags = 0;     
	bi.lpfn = NULL;     
	bi.lParam = 0;     
	bi.iImage = 0;     

	LPITEMIDLIST lp = SHBrowseForFolder(&bi);     

	if(lp && SHGetPathFromIDList(lp, filePath))     
	{  
		SetDlgItemTextW(IDC_EDIT_PATH,filePath);
	}
}

void CInfoDlg::InFileInterpret(char* inFile,std::queue<int> &infoQueue)
{
	char _rvtFileName[MAX_INFO_LENGTH];
	char _bldgName[MAX_INFO_LENGTH];
	TCHAR rvtFileName[MAX_INFO_LENGTH];
	TCHAR bldgName[MAX_INFO_LENGTH];
	int i=2;
	strcpy_s(_rvtFileName,inFile + infoQueue.front());	infoQueue.pop();
	strcpy_s(_bldgName,inFile + infoQueue.front());	infoQueue.pop();
	CPGCreatorDlg::CharToTchar(_rvtFileName,rvtFileName);
	CPGCreatorDlg::CharToTchar(_bldgName,bldgName);
	SetDlgItemTextW(IDC_EDIT_FILE,rvtFileName);
	SetDlgItemTextW(IDC_EDIT_BLDG,bldgName);
	CEdit* p_Efile = (CEdit*)GetDlgItem(IDC_EDIT_FILE);
	CEdit* p_Ebldg = (CEdit*)GetDlgItem(IDC_EDIT_BLDG);
	CEdit* p_Ebldguse = (CEdit*)GetDlgItem(IDC_EDIT_BLDGUSE);
	CEdit* p_Estru = (CEdit*)GetDlgItem(IDC_EDIT_STRU);
	CEdit* p_Eyear = (CEdit*)GetDlgItem(IDC_EDIT_YEAR);
	p_Efile->SetLimitText(15);
	p_Ebldguse->SetLimitText(15);
	p_Ebldg->SetLimitText(15);
	p_Estru->SetLimitText(15);
	p_Eyear->SetLimitText(15);
}

/*
void CInfoDlg::OnKillfocusEditFile()
{
	// TODO: 在此添加控件通知处理程序代码
	CString temp;
	GetDlgItem(IDC_EDIT_FILE)->GetWindowTextW(temp);
	wcscpy_s(rvtFileName,temp);
}


void CInfoDlg::OnKillfocusEditBldg()
{
	// TODO: 在此添加控件通知处理程序代码
	CString temp;
	GetDlgItem(IDC_EDIT_BLDG)->GetWindowTextW(temp);
	wcscpy_s(bldgName,temp);
}


void CInfoDlg::OnKillfocusEditAddress()
{
	// TODO: 在此添加控件通知处理程序代码
	CString temp;
	GetDlgItem(IDC_EDIT_ADDRESS)->GetWindowTextW(temp);
	wcscpy_s(projAdd,temp);
}


void CInfoDlg::OnKillfocusEditProj()
{
	// TODO: 在此添加控件通知处理程序代码
	CString temp;
	GetDlgItem(IDC_EDIT_PROJ)->GetWindowTextW(temp);
	wcscpy_s(projName,temp);
}
*/

void CInfoDlg::OutputInfo(FILE* fp)
{
	CString temp;
	char _rvtFileName[MAX_INFO_LENGTH];
	char _bldgName[MAX_INFO_LENGTH];
	char _bldgUse[MAX_INFO_LENGTH];
	char _struUse[MAX_INFO_LENGTH];
	char _year[MAX_INFO_LENGTH];
	GetDlgItem(IDC_EDIT_FILE)->GetWindowTextW(temp);
	CPGCreatorDlg::TcharToChar(temp,_rvtFileName);
	GetDlgItem(IDC_EDIT_BLDG)->GetWindowTextW(temp);
	CPGCreatorDlg::TcharToChar(temp,_bldgName);
	GetDlgItem(IDC_EDIT_BLDGUSE)->GetWindowTextW(temp);
	CPGCreatorDlg::TcharToChar(temp,_bldgUse);
	GetDlgItem(IDC_EDIT_STRU)->GetWindowTextW(temp);
	CPGCreatorDlg::TcharToChar(temp,_struUse);
	GetDlgItem(IDC_EDIT_YEAR)->GetWindowTextW(temp);
	CPGCreatorDlg::TcharToChar(temp,_year);
	fprintf_s(fp,"%s\t",_rvtFileName);
	fprintf_s(fp,"%s\t",_bldgName);
	fprintf_s(fp,"%s\t",_bldgUse);
	fprintf_s(fp,"%s\t",_struUse);
	fprintf_s(fp,"%s\t\n",_year);
}


char* CInfoDlg::GetFilePath()
{
	char* temp_path = new char[MAX_PATH];
	CPGCreatorDlg::TcharToChar(filePath,temp_path);
	return temp_path;
}


BOOL CInfoDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN ) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}
