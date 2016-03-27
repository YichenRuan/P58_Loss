#pragma once

#define MAX_SETTING_LENGTH 600
#define NUM_SETTING 11

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
	int setting[NUM_SETTING];
	afx_msg void OnBnClickedOk();
	bool IsLegalInput(char file[]);
	char lastSetting[MAX_SETTING_LENGTH];
	void OutputInfo(FILE* fp);
	void ReadExternalSetting();
	CString m_ENotepad;
};
