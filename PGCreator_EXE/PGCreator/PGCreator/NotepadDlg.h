#pragma once

#define MAX_SETTING_LENGTH 3072
#define NUM_SETTING 62
#define LENGTH_FILENAME 30

// CNotepadDlg �Ի���

class CNotepadDlg : public CDialog
{
	DECLARE_DYNAMIC(CNotepadDlg)

public:
	CNotepadDlg(CWnd* pParent = NULL);   // ��׼���캯��
	virtual ~CNotepadDlg();

// �Ի�������
	enum { IDD = IDD_NOTEPAD };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV ֧��

	DECLARE_MESSAGE_MAP()
public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	CEdit* p_ENote;
	virtual BOOL OnInitDialog();
	int* setting;
	afx_msg void OnBnClickedOk();
	bool IsLegalInput_(char file[]);
	char lastSetting[MAX_SETTING_LENGTH];
	char settingFileName[LENGTH_FILENAME];
	void OutputInfo(FILE* fp);
	void ReadExternalSetting();
	CString m_ENotepad;
	bool isSet;
	char* commandLine;
};
