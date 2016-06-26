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
	isSet = false;
	setting = new int[NUM_SETTING];
	char* fileName = "DS.SET";
	strcpy_s(settingFileName,fileName);
}

CNotepadDlg::~CNotepadDlg()
{
}

void CNotepadDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_NOTE, m_ENotepad);
	DDV_MaxChars(pDX, m_ENotepad, 3000);
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
		if(!IsLegalInput_(settingDoc))
		{
			AfxMessageBox(L"格式错误，请重新输入");
		}
		else
		{
			strcpy_s(lastSetting,settingDoc);
			AfxMessageBox(L"默认值设置成功！");
			isSet = true;
			CDialog::OnOK();
		}
	}
}

bool CNotepadDlg::IsLegalInput_(char file[])
{
	int length = ((CString)file).GetLength();
	--length;
	int i = 0;
	int count = 0;
	while (i <= length)
	{
		if (file[i] == '\n' && file[++i] != '/')
		{
			if (file[i] == '\0')
			{
				break;
			}
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

	if (isSet)
	{
		
		int fileEndPosi = ((CString)lastSetting).GetLength();
		lastSetting[fileEndPosi] = '#';
		lastSetting[fileEndPosi + 1] = '\0';
		char inPath[MAX_PATH] = {0};
		USES_CONVERSION;
		commandLine = T2A(CPGCreatorDlg::GetMyCommandLine());
		strcat_s(inPath,commandLine);
		strcat_s(inPath,settingFileName);
		
		SetFileAttributes((CString)inPath,FILE_ATTRIBUTE_NORMAL);
		FILE* fp_setting;
		fopen_s(&fp_setting,inPath,"w+");
		fprintf_s(fp_setting,lastSetting);
		SetFileAttributes((CString)inPath,FILE_ATTRIBUTE_HIDDEN);
		fclose(fp_setting);
	}
}

void CNotepadDlg::ReadExternalSetting()
{
	char inPath[MAX_PATH] = {0};
	USES_CONVERSION;
	commandLine	= T2A(CPGCreatorDlg::GetMyCommandLine());
	strcat_s(inPath,commandLine);
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
		if (!IsLegalInput_(lastSetting)) isLegal = false;
	}
	if(fp_setting == NULL || !isLegal)
	{
		char* settingTemplate = "//Default Value Setting\r\n//Please read the help file for more information\r\n//---Braced Frame---B1031\r\n0;\r\n0;\r\n0;\r\n0;\r\n//---Steel Joints---B1035\r\n0;\r\n//---Link Beam---B1042\r\n0;\r\n//---Shear Wall---B1044\r\n0;\r\n0;\r\n//---Flat Slab---B1049\r\n0;\r\n0;\r\n0;\r\n//---Masonry Wall---B105\r\n0;\r\n0;\r\n//---Roof---B3011\r\n0;\r\n//---Gypsum Wall---C1011\r\n0;\r\n0;\r\n//---Stair---C2011\r\n0;\r\n0;\r\n//---Ceiling---C3032\r\n0;\r\n0;\r\n//---Ceiling Lighting---C3033-C3034\r\n0;\r\n0;\r\n//---Pipe---D2021, D2022, D2031, D2051, D2061, D4011\r\n0;\r\n//---Chiller---D3031\r\n0;\r\n0;\r\n0;\r\n//---Cooling Tower---D3031\r\n0;\r\n0;\r\n0;\r\n//---Compressor---D3032\r\n0;\r\n0;\r\n0;\r\n//---HVAC Fan in Line Fan---D3041.00\r\n0;\r\n//---Duct---D3041\r\n0;\r\n//---HVAC Fan---D3041\r\n0;\r\n0;\r\n//---Diffusser---D3041\r\n0;\r\n//---Air Handling Unit---D3052\r\n0;\r\n0;\r\n0;\r\n//---Control Panel---D3067\r\n0;\r\n0;\r\n//---Fire Sprinkler---D4011\r\n0;\r\n0;\r\n//---Transformer---D5011\r\n0;\r\n0;\r\n0;\r\n//---Motor Control Center---D5012.01\r\n0;\r\n0;\r\n//---Low Voltage Switchgear---D5012.02\r\n0;\r\n0;\r\n0;\r\n//---Distribution Panel---D5012.03\r\n0;\r\n0;\r\n0;\r\n//---Battery Rack---D5092.01\r\n0;\r\n0;\r\n//---Battery Charger---D5092.02\r\n0;\r\n0;\r\n//---Diesel Generator---D5092.03\r\n0;\r\n0;\r\n0;\r\n//End\r\n";
		strcpy_s(lastSetting,settingTemplate);
		for (int i=0;i<NUM_SETTING;++i)
		{
			setting[i] = 0;
		}
	}
}