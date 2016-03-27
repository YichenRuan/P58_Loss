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
	, m_ENotepad(_T(""))
{
}

CNotepadDlg::~CNotepadDlg()
{
}

void CNotepadDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_NOTE, m_ENotepad);
	DDV_MaxChars(pDX, m_ENotepad, 500);
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
	p_ENote = (CEdit*)GetDlgItem(IDC_EDIT_NOTE);
	p_ENote->SetWindowTextW((CString)lastSetting);
	return TRUE;
}



void CNotepadDlg::OnBnClickedOk()
{
	// TODO: 在此添加控件通知处理程序代码
	char settingDoc[MAX_SETTING_LENGTH];
	if (UpdateData(TRUE))
	{
		CPGCreatorDlg::TcharToChar(m_ENotepad,settingDoc);
		if(!IsLegalInput(settingDoc))
		{
			AfxMessageBox(L"格式错误，请重新输入");
		}
		else
		{
			strcpy_s(lastSetting,settingDoc);
			AfxMessageBox(L"默认值设置成功！");
			CDialog::OnOK();
		}
	}
}

bool CNotepadDlg::IsLegalInput(char file[])
{
	int length = ((CString)file).GetLength();
	--length;
	int i = 0;
	int count = 0;
	while (i <= length)
	{
		if (file[i] == '\n' && file[++i] != '/')
		{
			if (!('0' <= file[i] && file[i] <= '9')) return false;
			if (file[i+1] != ';') return false;
			setting[count++] = file[i] - '0';
		}
		++i;
	}
	if (count != NUM_SETTING)	return false;
	else return true;
}

void CNotepadDlg::OutputInfo(FILE* fp)
{
	for (int i=0;i<NUM_SETTING;++i)
	{
		fprintf_s(fp,"%d\t",setting[i]);
	}
	fprintf_s(fp,"\n");

	int fileEndPosi = ((CString)lastSetting).GetLength();
	lastSetting[fileEndPosi] = '#';
	lastSetting[fileEndPosi + 1] = '\0';
	char inPath[MAX_PATH];
	char* settingFileName = "\\PGCreator\\DS.SET";
	_getcwd(inPath,MAX_PATH);
	strcat_s(inPath,settingFileName);
	SetFileAttributes((CString)inPath,FILE_ATTRIBUTE_NORMAL);
	FILE* fp_setting;
	fopen_s(&fp_setting,inPath,"w+");
	fprintf_s(fp_setting,lastSetting);
	SetFileAttributes((CString)inPath,FILE_ATTRIBUTE_HIDDEN);
	fclose(fp_setting);
}

void CNotepadDlg::ReadExternalSetting()
{
	char inPath[MAX_PATH];
	char* settingFileName = "\\PGCreator\\DS.SET";
	_getcwd(inPath,MAX_PATH);
	strcat_s(inPath,settingFileName);
	SetFileAttributes((CString)inPath,FILE_ATTRIBUTE_NORMAL);
	FILE* fp_setting;
	fopen_s(&fp_setting,inPath,"r+");
	bool isLegal = true;
	if (fp_setting != NULL)
	{
		fread(&lastSetting,sizeof(char),MAX_SETTING_LENGTH,fp_setting);
		fclose(fp_setting);
		for (int i=0;i<MAX_SETTING_LENGTH;++i)
		{
			if (lastSetting[i] == '#')
			{
				lastSetting[i] = '\0';
				break;
			}
		}
		if (!IsLegalInput(lastSetting)) isLegal = false;
	}
	if(fp_setting == NULL || !isLegal)
	{
		char* settingTemplate = "//Default Value Setting\r\n//Please read the help file for more information\r\n//---Shear Wall---B1044\r\n0;\r\n0;\r\n//---Gypsum Wall---C1011\r\n0;\r\n0;\r\n//---Ceiling---C3032\r\n0;\r\n0;\r\n//---Ceiling Lighting---C3033-C3034\r\n0;\r\n0;\r\n//---Masonry Wall---B105\r\n0;\r\n0;\r\n//---Duct---D3041\r\n0;\r\n//End";
		strcpy_s(lastSetting,settingTemplate);
		for (int i=0;i<NUM_SETTING;++i)
		{
			setting[i] = 0;
		}
	}
}