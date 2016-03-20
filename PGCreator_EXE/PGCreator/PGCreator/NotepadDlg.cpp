// NotepadDlg.cpp : 实现文件
//

#include "stdafx.h"
#include "PGCreator.h"
#include "NotepadDlg.h"
#include "afxdialogex.h"
#include "direct.h"
#include "PGCreatorDlg.h"


// CNotepadDlg 对话框

IMPLEMENT_DYNAMIC(CNotepadDlg, CDialog)

CNotepadDlg::CNotepadDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CNotepadDlg::IDD, pParent)
{

}

CNotepadDlg::~CNotepadDlg()
{
}

void CNotepadDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}


BEGIN_MESSAGE_MAP(CNotepadDlg, CDialog)
	ON_BN_CLICKED(IDOK, &CNotepadDlg::OnBnClickedOk)
END_MESSAGE_MAP()


// CNotepadDlg 消息处理程序


BOOL CNotepadDlg::PreTranslateMessage(MSG* pMsg)
{
	// TODO: 在此添加专用代码和/或调用基类
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_ESCAPE ) return TRUE;
	if(pMsg->message==WM_KEYDOWN && pMsg->wParam==VK_RETURN && GetFocus() != GetDlgItem(IDC_EDIT_NOTE)) return TRUE;
	else return CDialog::PreTranslateMessage(pMsg);
}


BOOL CNotepadDlg::OnInitDialog()
{
	if (!CPGCreatorDlg::isSettingChanged)
	{
		char inPath[MAX_PATH];
		char* settingFileName = "\\PGCreator\\DS.SET";
		_getcwd(inPath,MAX_PATH);
		strcat_s(inPath,settingFileName);
		CString C_inPath(inPath);
		SetFileAttributes(C_inPath,FILE_ATTRIBUTE_NORMAL);
		FILE* fp;
		fopen_s(&fp,inPath,"r+");
		if (fp != NULL)
		{
			fread(&settingDoc,sizeof(char),MAX_SETTING_LENGTH,fp);
			fclose(fp);
			SetFileAttributes(C_inPath,FILE_ATTRIBUTE_HIDDEN);
			settingDoc[0] = '/';
			for (int i=0;i<MAX_SETTING_LENGTH;++i)
			{
				if (settingDoc[i] == '#')
				{
					settingDoc[i] = '\0';
					break;
				}
			}
			CString C_settingDoc(settingDoc);
			C_settingDoc.Replace(L"\n",L"\r\n");
			p_ENote = (CEdit*)GetDlgItem(IDC_EDIT_NOTE);
			p_ENote->SetWindowTextW(C_settingDoc);
		}
		else
		{
			AfxMessageBox(L"您没有设置权限");
			EndDialog(IDCANCEL);
		}
	}
	
	else
	{
		p_ENote = (CEdit*)GetDlgItem(IDC_EDIT_NOTE);
		p_ENote->SetWindowTextW(CPGCreatorDlg::GiveSettingCopy());
	}

	return TRUE;
}



void CNotepadDlg::OnBnClickedOk()
{
	// TODO: 在此添加控件通知处理程序代码
	CString C_settingDoc;
	p_ENote = (CEdit*)GetDlgItem(IDC_EDIT_NOTE);
	p_ENote->GetWindowTextW(C_settingDoc);
	C_settingDoc.Replace(L"\r\r\r\r\r\r\r\r\r",L"\r");
	C_settingDoc.Replace(L"\r\r\r\r\r\r\r\r",L"\r");
	C_settingDoc.Replace(L"\r\r\r",L"\r");
	CPGCreatorDlg::TcharToChar(C_settingDoc,settingDoc);
	CPGCreatorDlg::GetSetting(settingDoc);
	CDialog::OnOK();
}
